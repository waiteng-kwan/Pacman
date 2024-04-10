using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ExitApplicationUIEvent",
menuName = "Scriptable Objects/UI/Event/ExitApplication", order = 100)]
public class ExitApplicationUIEvent : UIEvent
{
    public override void Execute()
    {
        Debug.Log("[Application] Exiting program...");

        Application.Quit();
    }
}