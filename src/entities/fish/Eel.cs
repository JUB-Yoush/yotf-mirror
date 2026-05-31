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

    enum State
    {
        Wander,
        Electric,
    }

    float elecTimer = 3f;
    float wanderTimer = 3f;

    private readonly StateMachine<State> stateMachine = new();

    public override void _Ready()
    {
        stateMachine.AddState(State.Wander, WanderUpdate, WanderEnter);
        stateMachine.AddState(State.Electric, ElectricUpdate, ElectricEnter, ElectricExit);
        stateMachine.State = State.Wander;
    }

    private void WanderEnter() { }

    private void WanderUpdate(float delta)
    {
        //SmoothMoveTo(GlobalPosition + Vector3.Forward, 2, delta);
        elecTimer -= delta;
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

        Velocity = MiscExt.V3Lerp(Velocity, target * speed, Profile.RotationSpeed * delta);
        //Velocity = dir * speed;

        float targetYaw = Mathf.Atan2(dir.X, dir.Z);
        GlobalRotation = GlobalRotation with
        {
            Y = Mathf.LerpAngle(GlobalRotation.Y, targetYaw, Profile.RotationSpeed * delta),
        };

        return target.LengthSquared() < arrivalThreshold;
    }

    public override void _PhysicsProcess(double delta)
    {
        stateMachine.Update(delta);
        MoveAndSlide();
    }
}
