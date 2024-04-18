using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BasicDotData",
menuName = "Scriptable Objects/BasicDotData Data", order = 1)]
public class EdibleData : ScriptableObject
{
    public enum EdibleType
    {
        Normal,
        Multiply,
        Add,
        EatGhost
    }

    public EdibleType EdibleDotType;

    [Header("Effect")]
    [ShowIf("EdibleDotType", EdibleType.Normal)]
    public int Score = 1;
    [ShowIf("EdibleDotType", EdibleType.Multiply)]
    public int Multiplier = 2;
    [ShowIf("EdibleDotType", EdibleType.Add)]
    public int AddBy = 1;
    [ShowIf("EdibleDotType", EdibleType.EatGhost)]
    public float Duration = 3f;

    [Header("Respawn")]
    public bool WillRespawn = false;
    [ShowIf("WillRespawn")]
    public float TimeToRespawn = 5f;

    public int CalculateScoreToAdd()
    {
        switch (EdibleDotType)
        {
            case EdibleData.EdibleType.Normal:
                return Score;
            case EdibleData.EdibleType.Multiply:
                return Score * Multiplier;
            case EdibleData.EdibleType.Add:
                return Score * AddBy;
            default:
                return 0;
        }
    }
}