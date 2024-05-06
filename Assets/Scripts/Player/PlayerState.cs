using Game;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerState : MonoBehaviour, IPlayerState
{
    //health
    [SerializeField, ReadOnly]
    private int m_health = 3;
    public int Health => m_health;

    //score
    [SerializeField, ReadOnly]
    private int m_score = 0;
    public int Score => m_score;

    public PlayerController Owner => null;

    public void SetHealth(int value)
    {
        m_health = value;
    }

    public void SetScore(int value)
    {
        m_score = value;
    }
}
