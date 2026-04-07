using Godot;

public class FlyingState : IPlayerState
{
    public PlayerState Type => PlayerState.Flying;

    public void Enter(PlayerController player)
    {
        player.Velocity = player.Velocity with { Y = 0f };
    }

    public void Exit(PlayerController player)
    {
        player.Velocity = player.Velocity with { Y = 0f };
    }

    public void Update(PlayerController player, float delta)
    {
        Vector3 direction = player.GetCameraRelativeDirection();
        Vector2 hVeloc = new Vector2(direction.X, direction.Z).Normalized() * player.MoveSpeed;

        player.UpdateBodyDirection(direction, delta);

        Vector3 velocity = player.Velocity;

        if (Input.IsActionPressed("move_modifier"))
            hVeloc *= 2;

        velocity.X = hVeloc.X;
        velocity.Z = hVeloc.Y;

        if (Input.IsKeyPressed(Key.E))
            velocity.Y += player.FlySpeed + player.MoveSpeed * 0.016f;
        if (Input.IsKeyPressed(Key.Q))
            velocity.Y -= player.FlySpeed + player.MoveSpeed * 0.016f;

        player.Velocity = velocity;
        player.MoveAndSlide();
    }
}
