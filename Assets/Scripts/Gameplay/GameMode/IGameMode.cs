using Client;
using Game;
using System;
using System.Collections;

public interface IGameMode
{
    //get instance
    static GameModeBase Instance { get; }
    GameBoardInstance Board { get; }
    GameModeData Settings { get; }

    bool RegisterPlayer(PlayerController pc);
    void PlayerScored(int index, int addScoreBy = 1);
    void PlayerUpdateHealth(int index, int addHpBy = -1);
    void PlayerDied(PlayerController pc);
    void StartPlayerRespawn(PlayerController pc);
    void PlayerCollidedWithGhost(GhostBehaviourBase ghost, PacmanBehaviour pChar);
    
    GhostBehaviourBase SpawnGhost(int ind = 0);
    void GhostDied(GhostBehaviourBase ghost);

    void StartSetupGameMode();
    IEnumerator GameModeSetupProcess();

    bool IsGamePaused();
    bool CanGameEnd();
}