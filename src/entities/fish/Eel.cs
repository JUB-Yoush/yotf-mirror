using System;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Eel : Fish, IBubbleable, IHearNoise, IDoesAction
{
    public override void _Notification(int what) => this.Notify(what);

    [Node]
    public required CollisionShape3D ZapShape { set; get; }

    [Node]
    public required Area3D ZapArea { set; get; }

    public float Period
    {
        private set { field = (float)Mathf.Wrap(value, 0, 2 * Math.PI); }
        get;
    }

    public Bubble? BubbleJail { get; set; }

    Mesh IBubbleable.Mesh => Mesh.Mesh;
    float IBubbleable.MeshScale => .3f;

    public bool InAction { get; set; }

    enum State
    {
        Wander,
        Electric,
        Bubbled,
    }

    float elecTimer = 3f;
    float wanderTimer = 3f;

    private readonly StateMachine<State> stateMachine = new();

    public override void _Ready()
    {
        stateMachine.AddState(State.Wander, WanderUpdate, WanderEnter);
        stateMachine.AddState(State.Electric, ElectricUpdate, ElectricEnter, ElectricExit);
        stateMachine.AddState(State.Bubbled, BubbledUpdate);
        stateMachine.State = State.Wander;
    }

    private void BubbledUpdate(float delta)
    {
        GlobalPosition = BubbleJail!.GlobalPosition;
        Velocity = Vector3.Zero;
    }

    private void WanderEnter()
    {
        CurrentRoom = AssignCurrentRoom();
        ReturningHome = true;
    }

    private void WanderUpdate(float delta)
    {
        if (ReturningHome)
        {
            ReturningHome = !(
                SmoothMoveTo(CurrentRoom!.GlobalPosition, WanderSpeed, delta, WanderRadius * 5)
            );
            return;
        }

        Period += delta * WanderSpeed;
        var target = new Vector3(
            WanderRadius * MathF.Sin(Period),
            WanderRadius * MathF.Sin(Period * NavRandomOffsetRange),
            WanderRadius * MathF.Cos(Period)
        );
        target += CurrentRoom!.GlobalPosition;
        SmoothMoveTo(target, WanderSpeed, delta);
        // Velocity = new(
        //     (float)-(WanderRadius * Math.Sin(Period)),
        //     0,
        //     (float)-(WanderRadius * -Math.Cos(Period))
        // );
        //elecTimer -= delta;
        if (elecTimer < 0)
        {
            stateMachine.State = State.Electric;
            elecTimer = 3f;
        }
    }

    private void ElectricEnter()
    {
        ZapShape.SetDeferred(CollisionShape3D.PropertyName.Disabled, false);
        InAction = true;
    }

    private void ElectricExit()
    {
        ZapShape.SetDeferred(CollisionShape3D.PropertyName.Disabled, true);
        InAction = false;
    }

    private void ElectricUpdate(float delta)
    {
        foreach (var overlapper in ZapArea.GetOverlappingBodies())
        {
            if (overlapper == this)
                continue;
            if (overlapper is Player player)
            {
                player.GetShocked(this.GlobalPosition);
            }
        }
        elecTimer -= delta;
        if (elecTimer < 0)
        {
            stateMachine.State = State.Wander;
            elecTimer = 3f;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        stateMachine.Update(delta);
        MoveAndSlide();
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

    public void OnNoiseHeard(Node3D NoiseSource, float dB, SFX noise)
    {
        stateMachine.State = State.Electric;
    }
}
