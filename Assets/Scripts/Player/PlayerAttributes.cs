using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttributes : MonoBehaviour
{
    public enum PlayerState
    {
        Alive,
        Dead,
        Respawning,
        Invul
    }

    private bool m_canEatGhosts = false;
    public bool CanEatGhosts => m_canEatGhosts;

    //health
    private int m_health = 3;

    public void UpdateHealth(int newHealth)
    {
        m_health = newHealth;
    }

    public void SetCanEatGhostState(bool canEatGhost)
    {
        m_canEatGhosts = canEatGhost;

        Invoke("SetEatGhostStateInactive", 5f);
    }

    public void SetEatGhostStateInactive()
    {
        m_canEatGhosts = false;
    }
}
