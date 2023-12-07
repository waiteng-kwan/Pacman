public interface IPlayerAttributes
{
    int Score { get; }
    int Health { get; }
    void SetHealth(int value);
    void SetScore(int value);
}