using UnityEngine;

public interface IGameBoard
{
    public int RemainingTotalPellets { get; }
    public int GetRemainingPellets(DotData.DotType type);
    public int GetMaxScorePellets();
    public Vector3 GetPlayerSpawnPoint(bool rand = false);
}