using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class of all UI Events
/// </summary>
public class UIEvent : ScriptableObject
{
    public virtual void Execute()
    {
        Debug.Log("Execute");
    }    
}