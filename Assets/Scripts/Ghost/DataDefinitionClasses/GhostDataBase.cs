using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "GhostDataBase",
menuName = "Scriptable Objects/GhostDataBase Data", order = 2)]
public class GhostDataBase : ScriptableObject
{
    public enum GhostType
    {
        White = 0,        //this is default!
        Red,
        Cyan,
        Blue,
        Orange,
        Pink
    }

    //base
    [Header("Basic")]
    public GhostBehaviourBase ParentGhostPrefab;

    //chara
    [Header("Character")]
    public GhostType GhostCharType;
    public MeshRenderer GhostCharModel;
    public string GhostDisplayName;        //prolly not needed

    [Header("Movement")]
    public float Speed = 5f;
    public bool HasAccelDeccel = false;

    [Header("AI")]
    public bool UseNavMesh = false;          //replace with AI Tree next time
    //the time before it changes state, x = min, y = max
    [NaughtyAttributes.InfoBox("X = min, Y = max")]
    public Vector2 ChangeStateDampingRange = Vector2.one;
    [NaughtyAttributes.InfoBox("X = min, Y = max")]
    public Vector2 ChangeIdleWaitngRange = Vector2.one;
    public float DetectionRange = 8f;

    [Header("Gameplay")]
    public float RespawnTime = 5f;
    public float GhostIdleTime = 3.5f;
}