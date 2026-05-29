using System;
using Godot;

namespace Yotf;

// we should probably use a growing visibility notifier to check if there is ink on the screen that is obscuring the camera?
// not sure the best course of action.
[Meta(typeof(IAutoNode))]
public partial class Inkfish : Fish, IHearNoise
{
    public override void _Notification(int what) => this.Notify(what);

    [Node]
    public required GpuParticles3D InkEmitter { set; get; }

    private readonly StateMachine<State> stateMachine = new();

    public enum State
    {
        Wander,
        Flee,
    }

    public override void _Ready()
    {
        stateMachine.AddState(State.Wander, WanderUpdate);
        stateMachine.AddState(State.Flee, FleeUpdate, FleeEnter);
    }

    public override void _PhysicsProcess(double delta)
    {
        stateMachine.Update(delta);
        MoveAndSlide();
    }

    public void WanderUpdate(float delta) { }

    public void FleeEnter()
    {
        var tween = CreateTween();
        var fleeDir = (GlobalPosition - ThreatTarget!.GlobalPosition).Normalized();
        //LookAt(GlobalPosition - fleeDir);
        tween.TweenFn<Vector3>(
            (target) => LookAt(GlobalPosition - target),
            -GlobalTransform.Basis.Z,
            fleeDir,
            1
        );
        tween.Fn(() =>
        {
            Velocity = fleeDir * 10;
            InkEmitter.Emitting = true;
        });
    }

    public void FleeUpdate(float delta) { }

    public void OnNoiseHeard(Node3D NoiseSource, float dB, SFX noise)
    {
        ThreatTarget = NoiseSource;
        ThreatPosition = NoiseSource.GlobalPosition;
        stateMachine.State = State.Flee;
    }
}
