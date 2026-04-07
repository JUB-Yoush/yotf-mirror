public interface IPlayerState
{
    PlayerState Type { get; }
    void Enter(PlayerController player);
    void Exit(PlayerController player);
    void Update(PlayerController player, float delta);
}
