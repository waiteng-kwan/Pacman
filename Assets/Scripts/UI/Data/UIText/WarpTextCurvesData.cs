using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

[CreateAssetMenu(fileName = "WarpTextCurvesData",
menuName = "Scriptable Objects/UI/WarpTextCurvesData Data", order = 1)]
public class WarpTextCurvesData : ScriptableObject
{
    public List<AnimationCurve> CurveList = new();

    public AnimationCurve GetRandomCurve()
    {
        return CurveList[Random.Range(0, CurveList.Count)];
    }


    [Header("Generate Curve")]
    public int MinCurveCount = 4;
    public int MaxCurveCount = 4;
    public float MinFrameStep;
    public float MaxFrameStep;
    public Vector2 RandomInRangeY = Vector2.up;

    [Button]
    void GenerateRandomCurve()
    {
        int frameCount = Random.Range(MinCurveCount, MaxCurveCount + 1);
        float x = 0f, y = 0f;

        Keyframe[] frames = new Keyframe[4];
        for (int i = 0; i < frameCount; ++i)
        {
            frames[i] = new Keyframe(x, y);

            x += Random.Range(MinFrameStep, MaxFrameStep);
            y = Random.Range(RandomInRangeY.x, RandomInRangeY.y);
        }

        CurveList.Add(new AnimationCurve(frames));
    }

    [Button]
    void ClearCurves()
    {
        CurveList.Clear();
    }
}