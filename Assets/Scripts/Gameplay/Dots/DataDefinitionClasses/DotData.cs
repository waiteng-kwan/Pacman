using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BasicDotData",
menuName = "Scriptable Objects/BasicDotData Data", order = 1)]
public class DotData : ScriptableObject
{
    public enum DotType
    {
        Normal,
        Multiply,
        Add,
        EatGhost
    }

    public DotType EdibleDotType;

    [Header("Effect")]
    [ShowIf("EdibleDotType", DotType.Normal)]
    public int Score = 1;
    [ShowIf("EdibleDotType", DotType.Multiply)]
    public int Multiplier = 2;
    [ShowIf("EdibleDotType", DotType.Add)]
    public int AddBy = 1;
    [ShowIf("EdibleDotType", DotType.EatGhost)]
    public float Duration = 3f;

    public int CalculateScoreToAdd()
    {
        switch (EdibleDotType)
        {
            case DotData.DotType.Normal:
                return Score;
            case DotData.DotType.Multiply:
                return Score * Multiplier;
            case DotData.DotType.Add:
                return Score * AddBy;
            default:
                return 0;
        }
    }
}