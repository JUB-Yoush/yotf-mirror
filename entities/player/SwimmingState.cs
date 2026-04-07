using Godot;

public class SwimmingState : IPlayerState
{
    public PlayerState Type => PlayerState.Swimming;

    private float _swimYaw;
    private float _swimPitch;

    public void Enter(PlayerController player)
    {
        player.Velocity = player.Velocity with { Y = 0f };
        _swimYaw = player._skin.Rotation.Y;
        _swimPitch = player._skin.Rotation.X;
    }

    public void Exit(PlayerController player)
    {
        player.UpdateBodyRotation(player._skin.Rotation with { X = 0f });
        _swimPitch = 0f;
    }

    public void Update(PlayerController player, float delta)
    {
        float yawInput = Input.GetActionStrength("right") - Input.GetActionStrength("left");
        float pitchInput = Input.GetActionStrength("up") - Input.GetActionStrength("down");

        _swimYaw -= yawInput * player.SwimRotationSpeed * delta;
        _swimPitch -= pitchInput * player.SwimRotationSpeed * delta;

        player.UpdateBodyRotation(new Vector3(_swimPitch, _swimYaw, 0f));

        if (Input.IsActionPressed("move_modifier"))
            player.Velocity = player._skin.GlobalTransform.Basis.Y * player.SwimSpeed;
        else
            player.Velocity = player.Velocity.Lerp(Vector3.Zero, player.SwimDamping * delta);

        player.MoveAndSlide();
    }
}
