namespace Yotf;

public class WalkingState : IPlayerState
{
    public PlayerState Type => PlayerState.Walking;

    public void Enter(Player player)
    {
        Audio.PlaySfx(Sfx.Oxygen);
    }

    public void Exit(Player player)
    {
        Audio.PlaySfx(Sfx.Dive);
    }

    public void Update(Player player, float delta)
    {
        Vec3 direction = player.GetCameraRelativeDirection();
        Vec2 hVeloc = new Vec2(direction.X, direction.Z).Normalized() * player.MoveSpeed;

        player.UpdateBodyWalkDirection(direction, delta);

        Vec3 velocity = player.Velocity;

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
