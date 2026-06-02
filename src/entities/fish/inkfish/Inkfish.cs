using System;
using Godot;

namespace Yotf;

// we should probably use a growing visibility notifier to check if there is ink on the screen that is obscuring the camera?
// not sure the best course of action.
[Meta(typeof(IAutoNode))]
public partial class Inkfish : Fish, IHearNoise
{
    public override void _Notification(int what) => this.Notify(what);

    [Export]
    float DecendSpeed = 5f;

    [Export]
    float PushForce = 5f;

    [Node]
    public required GpuParticles3D InkEmitter { set; get; }

    private readonly StateMachine<State> stateMachine = new();

    public enum State
    {
        Wander,
        Flee,
    }

    public float Period
    {
        private set { field = (float)Mathf.Wrap(value, 0, 2 * Math.PI); }
        get;
    }

    public override void _Ready()
    {
        navGraph = this.SceneRoot().GetNode<NavGraph>()!;
        stateMachine.AddState(State.Wander, WanderUpdate, WanderEnter);
        stateMachine.AddState(State.Flee, FleeUpdate, FleeEnter);
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

    private void ReturningToHomeUpdate(float delta)
    {
        if (SmoothMoveTo(CurrentRoom!.GlobalPosition, WanderSpeed, delta, WanderRadius * 5))
            stateMachine.State = State.Wander;
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
        var target = new Vector3(
            WanderRadius * MathF.Sin(Period),
            WanderRadius * MathF.Sin(Period * NavRandomOffsetRange),
            WanderRadius * MathF.Cos(Period)
        );
        target += CurrentRoom!.GlobalPosition;
        SmoothMoveTo(target, WanderSpeed, delta);
    }

    /*
     * on flee:
     * - point away from fear
     * - velocity = that * flee speed
     * - find closest node
     * - smooth move towards it
     * - move AWAY from threat
    */
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
            InkEmitter.Emitting = true;
        });
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
                stateMachine.State = State.Wander;
                return;
            }
            CurrentNode = navGraph.NodeClosestTo(fleeVec, CurrentNode);
        }

        Log.PrintLn((ThreatTarget.GlobalPosition - GlobalPosition).Length());
    }

    public void OnNoiseHeard(Node3D NoiseSource, float dB, SFX noise)
    {
        ThreatTarget = NoiseSource;
        ThreatPosition = NoiseSource.GlobalPosition;
        stateMachine.State = State.Flee;
    }
}
