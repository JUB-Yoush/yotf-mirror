using System;
using Godot;

namespace Yotf;

// we should probably use a growing visibility notifier to check if there is ink on the screen that is obscuring the camera?
// not sure the best course of action.
[Meta(typeof(IAutoNode))]
public partial class Inkfish : Fish, IHearNoise, IBubbleable
{
    public override void _Notification(int what) => this.Notify(what);

    [Export]
    float DecendSpeed = 5f;

    [Export]
    float PushForce = 5f;

    [Export]
    float InkCollisderRaidus = 3f;

    [Export]
    float InkColliderLength = 10f;

    [Node]
    public required GpuParticles3D InkEmitter { set; get; }

    [Node]
    public required Area3D InkArea { set; get; }

    [Node]
    public required CollisionShape3D InkCollider { set; get; }

    public float MeshScale
    {
        get => 100;
    }

    private readonly StateMachine<State> stateMachine = new();

    public enum State
    {
        Wander,
        Flee,
        Bubbled,
    }

    public bool AxolotlTargets
    {
        get => true;
    }

    public float Period
    {
        private set { field = (float)Mathf.Wrap(value, 0, 2 * Math.PI); }
        get;
    }

    public Bubble? BubbleJail { get; set; }

    Mesh IBubbleable.Mesh => Mesh.Mesh;

    public override void _Ready()
    {
        navGraph = this.SceneRoot().GetNode<NavGraph>()!;
        stateMachine.AddState(State.Wander, WanderUpdate, WanderEnter);
        stateMachine.AddState(State.Flee, FleeUpdate, FleeEnter);
        stateMachine.AddState(State.Bubbled, BubbleUpdate);
    }

    private void BubbleUpdate(float delta)
    {
        if (BubbleJail == null)
        {
            stateMachine.State = State.Wander;
        }

        GlobalPosition = BubbleJail!.GlobalPosition;
        Velocity = Vector3.Zero;
    }

    public void WanderEnter()
    {
        CurrentRoom = AssignCurrentRoom();
    }

    public override void _PhysicsProcess(double delta)
    {
        stateMachine.Update(delta);
        MoveAndSlide();
    }

    public void WanderUpdate(float delta)
    {
        if (ReturningHome)
        {
            ReturningHome = !(
                SmoothMoveTo(CurrentRoom!.GlobalPosition, WanderSpeed, delta, WanderRadius * 5)
            );
            return;
        }

        Period += delta * WanderSpeed;

        // TODO(j) custom inkfish movement
        var target = new Vector3(
            WanderRadius * MathF.Sin(Period),
            WanderRadius * MathF.Sin(Period * NavRandomOffsetRange),
            WanderRadius * MathF.Cos(Period)
        );
        target += CurrentRoom!.GlobalPosition;
        SmoothMoveTo(target, WanderSpeed, delta);
    }

    public void FleeEnter()
    {
        var tween = CreateTween();
        var fleeDir = (GlobalPosition - ThreatTarget!.GlobalPosition).Normalized();
        tween.TweenFn<Vector3>(
            (target) => LookAt(GlobalPosition - target),
            -GlobalTransform.Basis.Z,
            fleeDir,
            .3f
        );

        tween.Fn(() =>
        {
            Velocity = fleeDir * 10;
            ToggleInk(true);
        });

        tween.TweenFn<float>(
            (value) => ((CapsuleShape3D)InkCollider.Shape).Height = value,
            0f,
            InkColliderLength,
            1f
        );

        tween.TweenFn<float>(
            (value) => ((CapsuleShape3D)InkCollider.Shape).Radius = value,
            0f,
            InkCollisderRaidus,
            1f,
            true
        );
        var fleeVec = (GlobalPosition - ThreatTarget!.GlobalPosition).Normalized() * FleeDistance;
        CurrentNode = navGraph.NodeClosestTo(fleeVec);
    }

    public void FleeUpdate(float delta)
    {
        var fleeVec = (GlobalPosition - ThreatTarget!.GlobalPosition).Normalized() * FleeDistance;
        if (SmoothMoveTo(CurrentNode!.GlobalPosition, FleeSpeed, delta, 10))
        {
            if ((ThreatTarget!.GlobalPosition - GlobalPosition).Length() >= FleeDistance / 2)
            {
                CreateTween().Fn(() => ToggleInk(false), 3);
                stateMachine.State = State.Wander;
                return;
            }
            CurrentNode = navGraph.NodeClosestTo(fleeVec, CurrentNode);
        }
    }

    public void OnNoiseHeard(Node3D NoiseSource, float dB, SFX noise)
    {
        ThreatTarget = NoiseSource;
        ThreatPosition = NoiseSource.GlobalPosition;
        stateMachine.State = State.Flee;
    }

    public void PutInBubble()
    {
        stateMachine.State = State.Bubbled;
        Mesh.Visible = false;
        DetectionZone.Monitoring = false;
    }

    public void FreeFromBubble()
    {
        stateMachine.State = State.Wander;
        Mesh.Visible = true;
        DetectionZone.Monitoring = true;
    }

    void ToggleInk(bool state)
    {
        InkCollider.SetDeferred(CollisionShape3D.PropertyName.Disabled, !state);
        InkEmitter.Visible = state;
        InkEmitter.Emitting = state;
    }
}
