using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UICanvasTransitionList",
menuName = "Scriptable Objects/UITransitionAnimationList Data", order = 1)]
public class UITransitionAnimationList : ScriptableObject
{
    [Header("Dev Description")]
    [Multiline, ReadOnly]
    public string DeveloperDescription; //this should not be used in code

    [Space]
    [Header("List")]
    public Animator Animators;
}