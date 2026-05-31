namespace Yotf;

public interface IPlayerState
{
    PlayerState Type { get; }
    void Enter(Player player);
    void Exit(Player player);
    void Update(Player player, float delta);
}
