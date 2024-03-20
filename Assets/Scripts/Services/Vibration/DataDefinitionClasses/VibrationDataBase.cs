using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Client.Services.Vibration;
using NaughtyAttributes;

[CreateAssetMenu(fileName = "VibrationData",
menuName = "Scriptable Objects/Data/Vibration Data", order = 0)]
public class VibrationDataBase : ScriptableObject
{
    [InfoBox("Constant = same strength for x seconds, Linear = increasing strength")]
    public RumblePattern Pattern;

    [Header("Time")]
    public float Duration = 3f;

    [Header("Frequency Data")]
    public float minFreq = 0.2f;
    public float maxFreq = 1f;

    //pulse
    [Header("Pulse Data"), ShowIf("Pattern", RumblePattern.Pulse)]
    public float PulseDuration = 0.3f;
    public float PulseWaitBetweenDuration = 0.2f;

    //linear
    [Header("Linear Data"), ShowIf("Pattern", RumblePattern.Linear)]
    public float LowStep = 1f;
    public float HighStep = 5f;
}