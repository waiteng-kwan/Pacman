using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    //chara
    [Header("Character")]
    public GhostType GhostCharType;
    public MeshRenderer GhostCharModel;
    public string GhostDisplayName;        //prolly not needed

    [Header("Movement")]
    public float Speed = 5f;
    public bool HasAccelDeccel = false;

    [Header("AI Type")]
    public bool UseNavMesh = false;       //replace with AI Tree next time

    [Header("Gameplay")]
    public float RespawnTime = 5f;
}