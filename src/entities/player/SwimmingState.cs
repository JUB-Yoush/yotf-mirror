using Godot;

namespace Yotf;

public class SwimmingState : IPlayerState
{
    public PlayerState Type => PlayerState.Swimming;

    public void Enter(PlayerController player)
    {
        player.Velocity = player.Velocity with { Y = 0f };
    }

    public void Exit(PlayerController player)
    {
        player.UpdateBodyRotation(player.Skin.Rotation with { X = 0f });
    }

    public void Update(PlayerController player, float delta)
    {
        player.UpdateBodyRotation(
            new Vector3(player.Camera.GlobalRotation.X, player.Camera.GlobalRotation.Y, 0f)
        );

        Vector3 moveDir = Vector3.Zero;
        moveDir -=
            player.Camera.GlobalTransform.Basis.Z
            * (Input.GetActionStrength("up") - Input.GetActionStrength("down"));
        moveDir +=
            player.Camera.GlobalTransform.Basis.X
            * (Input.GetActionStrength("right") - Input.GetActionStrength("left"));

        float speed = player.SwimSpeed;
        if (Input.IsActionJustPressed("jump"))
            speed *= player.SwimBoostMultiplier;

        if (moveDir != Vector3.Zero)
            player.Velocity = moveDir.Normalized() * speed;
        else
            player.Velocity = player.Velocity.Lerp(Vector3.Zero, player.SwimDamping * delta);

        if (Input.IsActionJustPressed("pickup"))
        {
            GD.Print("woosh");
            player.Velocity = player.Velocity with { Y = player.Velocity.Y + 5 };
        }

        player.MoveAndSlide();
    }
}
