using Game;
using UnityEngine;

public interface IGhostAi
{
    void SetPawn(GhostBehaviourBase pawn);

    //state
    void SetNextState(GhostAiState value);
    GhostAiState GetCurrentState();
    void PrepChangeState(GhostAiState nextState);
    void SwitchState();

    //Movement
    void SetDestination(Vector3 destination);
}