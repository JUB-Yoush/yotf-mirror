using Godot;

namespace Yotf;

public class SwimmingState : IPlayerState
{
    public PlayerState Type => PlayerState.Swimming;

    private float swimYaw;
    private float swimPitch;

    public void Enter(PlayerController player)
    {
        player.Velocity = player.Velocity with { Y = 0f };
        swimYaw = player.Skin.Rotation.Y;
        swimPitch = player.Skin.Rotation.X;
    }

    public void Exit(PlayerController player)
    {
        player.UpdateBodyRotation(player.Skin.Rotation with { X = 0f });
        swimPitch = 0f;
    }

    public void Update(PlayerController player, float delta)
    {
        float yawInput = Input.GetActionStrength("right") - Input.GetActionStrength("left");
        float pitchInput = Input.GetActionStrength("up") - Input.GetActionStrength("down");

        swimYaw -= yawInput * player.SwimRotationSpeed * delta;
        swimPitch -= pitchInput * player.SwimRotationSpeed * delta;

        player.UpdateBodyRotation(new Vector3(swimPitch, swimYaw, 0f));

        if (Input.IsActionPressed("move_modifier"))
            player.Velocity = player.Skin.GlobalTransform.Basis.Y * player.SwimSpeed;
        else
            player.Velocity = player.Velocity.Lerp(Vector3.Zero, player.SwimDamping * delta);

        player.MoveAndSlide();
    }
}
