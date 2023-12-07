using Game;

public interface IGhostAi
{
    void SetPawn(GhostBehaviourBase pawn);

    //state
    void SetNextState(GhostAiState value);
    GhostAiState GetCurrentState();
}