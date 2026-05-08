using Godot;

namespace Yotf;

public class WalkingState : IPlayerState
{
    public PlayerState Type => PlayerState.Walking;

    public void Enter(PlayerController player) { }

    public void Exit(PlayerController player) { }

    public void Update(PlayerController player, float delta)
    {
        Vector3 direction = player.GetCameraRelativeDirection();
        Vector2 hVeloc = new Vector2(direction.X, direction.Z).Normalized() * player.MoveSpeed;

        player.UpdateBodyDirection(direction, delta);

        Vector3 velocity = player.Velocity;

        if (Input.IsActionPressed("jump") && player.IsOnFloor())
            velocity.Y = player.JumpSpeed;

        if (Input.IsKeyPressed(Key.Shift))
            hVeloc *= 2;

        velocity.X = hVeloc.X;
        velocity.Z = hVeloc.Y;

        if (!player.IsOnFloor())
            velocity.Y -= player.Gravity * player.Weight * delta;

        player.Velocity = velocity;
        player.MoveAndSlide();
    }
}
