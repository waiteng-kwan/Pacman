using UnityEngine;

[CreateAssetMenu(fileName = "ReturnToMainMenu",
menuName = "Scriptable Objects/UI/Event/ReturnToMainMenu", order = 99)]
public class ReturnToMainMenu : UIEvent
{
    public override void Execute()
    {


        Debug.Log("Returning to menu...");
    }    
}