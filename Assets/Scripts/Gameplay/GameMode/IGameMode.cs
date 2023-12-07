using Client;
using Game;
using System.Collections;

public interface IGameMode
{
    //get instance
    static GameModeBase Instance { get; }
    GameBoardInstance Board { get; }
    GameModeData Settings { get; }

    //game board instance
    void SetGameBoardInstance(GameBoardInstance board);

    bool RegisterPlayer(PlayerController pc);
    void PlayerScored(int index, int addScoreBy = 1);
    void PlayerUpdateHealth(int index, int addHpBy = -1);
    void PlayerDecHealth(int index);
    void PlayerDied(int index);
    void StartPlayerRespawn(PlayerController pc);
    void PlayerCollidedWithGhost(GhostBehaviourBase ghost);
    
    GhostBehaviourBase SpawnGhost(int ind = 0);
    void GhostDied(GhostBehaviourBase ghost);

    void StartSetupGameMode();
    IEnumerator GameModeSetupProcess();

    void GameEnded();
    void GameStarted();
    void GamePaused();

    bool IsGamePaused();
    bool CanGameEnd();
    int CalculateWhichPlayerWon();
}