using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PacmanBaseData",
menuName = "Scriptable Objects/PacmanBaseData Data", order = 1)]
public class PacmanBaseData : ScriptableObject
{
    public enum PacmanType
    {
        Yellow = 0,        //this is default!
        Pink,
        Purple,
        Blue,
        Red,
        Black
    }

    //chara
    [Header("Character")]
    public PacmanType PacmanCharType;
    public MeshRenderer PacmanCharModel;
    public string CharDisplayName;

    [Header("Movement")]
    public float Speed = 5f;
    public bool HasAccelDeccel = false;
}