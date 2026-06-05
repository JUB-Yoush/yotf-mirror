using Godot;

namespace Yotf;

public class SwimmingState : IPlayerState
{
    public static readonly Material UnderwaterCameraMat = GD.Load<Material>(
        "res://assets/materials/underwater_cam.tres"
    );
    public PlayerState Type => PlayerState.Swimming;

    PlayerStats Stats = null!;

    public void Enter(Player player)
    {
        player.Velocity = player.Velocity with { Y = 0f };
        Stats = player.Stats;
        player.UnderwaterRect.Visible = true;
    }

    public void Exit(Player player)
    {
        Stats.RestoreOxygen();
        Stats.Oxygen = Stats.MaxOxygen;
        Stats.Battery = Stats.MaxBattery;
        player.UpdateBodyRotation(player.Skin.Rotation with { X = 0f });
        player.UnderwaterRect.Visible = false;
    }

    public void Update(Player player, float delta)
    {
        Stats.SpendOxygen(delta);

        Basis cam = player.Camera.GlobalTransform.Basis;
        Vec3 bodyUp = -cam.Z;
        Vec3 bodyRight = cam.X;
        Vec3 bodyBack = bodyUp.Cross(bodyRight);
        player.UpdateBodyRotation(
            new Basis(bodyRight, bodyUp, bodyBack).Orthonormalized().GetEuler()
        );

        Vec3 moveDir = Vec3.Zero;
        moveDir -=
            player.Camera.GlobalTransform.Basis.Z
            * (Input.GetActionStrength("up") - Input.GetActionStrength("down"));
        moveDir +=
            player.Camera.GlobalTransform.Basis.X
            * (Input.GetActionStrength("right") - Input.GetActionStrength("left"));

        float speed = player.SwimSpeed + PlayerStats.ExtraSwimSpeed;
        if (Input.IsActionJustPressed("jump"))
            speed *= player.SwimBoostMultiplier;

        if (moveDir != Vec3.Zero)
            player.Velocity = moveDir.Normalized() * speed;
        else
            player.Velocity = player.Velocity.Lerp(Vec3.Zero, player.SwimDamping * delta);

        player.MoveAndSlide();
    }
}
