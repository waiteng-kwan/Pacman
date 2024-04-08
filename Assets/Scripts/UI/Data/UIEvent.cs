using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UIEvent",
menuName = "Scriptable Objects/UI/UIEvent Data", order = 1)]
public class UIEvent : ScriptableObject
{
    public void Execute()
    {
        Debug.Log("Execute");
    }    
}