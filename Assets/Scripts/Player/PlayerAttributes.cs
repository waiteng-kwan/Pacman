using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttributes : MonoBehaviour
{
    private int m_health = 3;

    public void UpdateHealth(int newHealth)
    {
        m_health = newHealth;
    }
}
