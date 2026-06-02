using System;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Eel : Fish
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

    enum State
    {
        Wander,
        ReturningToHome,
        Electric,
    }

    float elecTimer = 3f;
    float wanderTimer = 3f;

    private readonly StateMachine<State> stateMachine = new();

    public override void _Ready()
    {
        stateMachine.AddState(State.Wander, WanderUpdate, WanderEnter);
        stateMachine.AddState(State.Electric, ElectricUpdate, ElectricEnter, ElectricExit);
        stateMachine.AddState(State.ReturningToHome, ReturningToHomeUpdate);
        stateMachine.State = State.Wander;
    }

    private void WanderEnter() { }

    private void ReturningToHomeUpdate(float delta)
    {
        if (SmoothMoveTo(CurrentRoom!.GlobalPosition, WanderSpeed, delta, WanderRadius * 5))
            stateMachine.State = State.Wander;
    }

    private void WanderUpdate(float delta)
    {
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
        ZapShape.Disabled = false;
    }

    private void ElectricExit()
    {
        ZapShape.Disabled = true;
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

    internal bool SmoothMoveTo(
        Vector3 target,
        float speed,
        float delta,
        float arrivalThreshold = 0.1f
    )
    {
        target -= GlobalPosition;
        Vector3 dir = target.Normalized();

        Velocity = MiscExt.V3Lerp(Velocity, target * speed, WanderRotationSpeed * delta);

        float targetYaw = Mathf.Atan2(dir.X, dir.Z);
        GlobalRotation = GlobalRotation with
        {
            Y = Mathf.LerpAngle(GlobalRotation.Y, targetYaw, WanderRotationSpeed * delta),
        };

        return target.LengthSquared() < arrivalThreshold;
    }

    public override void _PhysicsProcess(double delta)
    {
        stateMachine.Update(delta);
        MoveAndSlide();
    }
}
