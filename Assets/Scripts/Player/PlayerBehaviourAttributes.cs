using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviourAttributes : MonoBehaviour
{
    private bool m_canEatGhosts = false;
    public bool CanEatGhosts => m_canEatGhosts;

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
