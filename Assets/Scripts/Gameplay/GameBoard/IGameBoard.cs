using UnityEngine;

public interface IGameBoard
{
    public int RemainingTotalPellets { get; }
    public int GetRemainingPellets(EdibleData.EdibleType type);
    public int GetMaxScorePellets();
    public Vector3 GetPlayerSpawnPoint(bool rand = false);
}