using System;
using Godot;

namespace Yotf;

/// <summary>
/// WIP
/// Jumps towards the player and crushes anything it lands on (including the player and other fish)
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class ThwompFish : Fish
{
    public override void _Notification(int what) => this.Notify(what);

    public enum State
    {
        Idle,
        Jumping,
    }

    [Export]
    float JumpRange = 10f;

    [Export]
    float JumpForce = 10f;

    [Export]
    float speed = 5f;

    [Export]
    float gravity = 9.8f;

    Vec3 target = Vec3.Zero;
    Vec2 jumpTarget = Vec2.Zero;
    Vec2 velocityXZ = Vec2.Zero;
    float velocityY = 0;

    [Node]
    public required Area3D VisionArea { set; get; }

    readonly StateMachine<State> stateMachine = new();

    public override void _Ready()
    {
        stateMachine.AddState(State.Idle, IdleUpdate);
        VisionArea.BodyEntered += VisionAreaBodyEntered;
    }

    private void VisionAreaBodyEntered(Node3D body)
    {
        if (body is Player player && stateMachine.State == State.Idle)
        {
            target = player.GlobalPosition;
            jumpTarget = CalculateJumpTarget();
            stateMachine.State = State.Jumping;
            velocityY = -JumpForce;
        }
    }

    private Vec2 CalculateJumpTarget()
    {
        if ((target - GlobalPosition).Length() < JumpRange)
        {
            return target.XZ();
        }
        return ((target - GlobalPosition).Normalized() * JumpRange).XZ();
    }

    private void IdleUpdate(float delta) { }

    private void JumpUpdate(float delta)
    {
        SmoothMoveXZ(jumpTarget, speed, delta);
        velocityY += gravity;
        Velocity = new(velocityXZ[0], velocityY, velocityXZ[1]);
    }

    internal bool SmoothMoveXZ(Vec2 target, float speed, float delta, float arrivalThreshold = 0.1f)
    {
        Vec2 dir = target.Normalized();

        velocityXZ.Lerp(target, speed * delta);

        float targetYaw = Mathf.Atan2(dir.X, dir.Y);
        GlobalRotation = GlobalRotation with
        {
            Y = Mathf.LerpAngle(GlobalRotation.Y, targetYaw, WanderRotationSpeed * delta),
        };

        return target.LengthSquared() < arrivalThreshold;
    }
}
