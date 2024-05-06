public interface IPlayerState
{
    int Score { get; }
    int Health { get; }
    void SetHealth(int value);
    void SetScore(int value);
    
    Game.PlayerController Owner { get; }
}