using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerAttributes : MonoBehaviour
{
    public enum PlayerState
    {
        Alive,
        Dead,
        Respawning,
        Invul
    }

    //health
    private int m_health = 3;

    public void UpdateHealth(int newHealth)
    {
        m_health = newHealth;
    }
}
