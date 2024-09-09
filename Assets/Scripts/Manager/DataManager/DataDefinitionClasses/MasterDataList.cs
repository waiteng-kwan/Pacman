using Client.UI;
using Core;
using Core.Ghost;
using System.Collections.Generic;
using UnityEngine;
using Utils;

[CreateAssetMenu(fileName = "MasterDataList",
menuName = "Scriptable Objects/MasterDataList Data", order = 1)]
public class MasterDataList : ScriptableObject
{
    [Header("Data")]
    public MasterSceneList SceneData;

    [Header("Characters")]
    public PacmanBehaviour PlayerCharacterPrefab;
    public List<PacmanBaseData> CharModelDataList = new List<PacmanBaseData>();
    public PacmanColorToMaterialColorSO PacManColorList;

    [Header("Ghosts")]
    public GhostBehaviourBase GhostPrefab;
    public List<GhostDataBase> GhostModelDataList = new List<GhostDataBase>();

    [Header("Dots/Pellets")]
    public List<EdibleData> DotDataList = new List<EdibleData>();

    [Header("Player Controller")]
    public Core.PlayerController PlayerControllerPrefab;

    [Header("UI")]
    public UIAssetList_Data UIAssetList;
}