using Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MasterDataList",
menuName = "Scriptable Objects/MasterDataList Data", order = 1)]
public class MasterDataList : ScriptableObject
{
    [Header("Data")]
    public MasterSceneList SceneData;

    [Header("Characters")]
    public PlayerBehaviour PlayerCharacterPrefab;
    public List<PacmanBaseData> m_charModelDataList = new List<PacmanBaseData>();

    [Header("Ghosts")]
    public GhostBehaviourBase GhostPrefab;
    public List<GhostDataBase> m_ghostModelDataList = new List<GhostDataBase>();

    [Header("Dots/Pellets")]
    public List<DotData> m_dotDataList = new List<DotData>();

    [Header("Player Controller")]
    public PlayerController PlayerControllerPrefab;
}