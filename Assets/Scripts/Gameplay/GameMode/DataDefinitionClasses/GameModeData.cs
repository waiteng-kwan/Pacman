using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameModeData",
menuName = "Scriptable Objects/GameModeData Data", order = 1)]
public class GameModeData : ScriptableObject
{
    //stage stuff
    public string DisplayName;
    public string SceneName;

    [Header("Set up")]
    public int StartingHealth = 3;
    public float GhostDefaultRespawnTime = 5f;
    public bool CanPlayerRespawn = false;
    public float PlayerRespawnTime = 5f;

    [Header("Ghost Set")]
    public GhostDataBase[] Ghosts;
    public int MaxGhostsAtOnce = 4;
    public bool OneOfEach = true;

    [Header("UI")]
    public GameObject StageUI;

    [Header("Gameplay")]
    public float SecondsBeforeGameStart = 3f;
}