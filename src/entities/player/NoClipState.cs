namespace Yotf;

public class NoClipState : IPlayerState
{
    public PlayerState Type => PlayerState.NoClip;

    const float Speed = 10f;

    public void Enter(Player player)
    {
        player.CollisionEnabled = false;
    }

    public void Exit(Player player)
    {
        player.CollisionEnabled = true;
        player.Velocity = Vec3.Zero;
    }

    public void Update(Player player, float delta)
    {
        Vec3 moveDir = Vec3.Zero;
        moveDir -= player.Camera.GlobalTransform.Basis.Z
            * (Input.GetActionStrength("up") - Input.GetActionStrength("down"));
        moveDir += player.Camera.GlobalTransform.Basis.X
            * (Input.GetActionStrength("right") - Input.GetActionStrength("left"));

        float speed = Speed;
        if (Input.IsKeyPressed(Key.Shift))
            speed *= 3f;

        player.Velocity = moveDir == Vec3.Zero ? Vec3.Zero : moveDir.Normalized() * speed;
        player.MoveAndSlide();
    }
}
