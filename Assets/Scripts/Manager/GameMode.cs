using Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameMode : MonoBehaviour
{
    //temp
    public static GameMode gameMode;

    private Dictionary<PlayerController, int> m_pcToScoreDictionary = 
        new Dictionary<PlayerController, int>();
    public int Score = 0;

    public UnityEvent<int, int> PlayerScoredEvent;
    public UnityEvent<int, int> PlayerLifeChangedEvent;

    private void Awake()
    {
        gameMode = this;

        PlayerScoredEvent = new UnityEvent<int, int>();
        PlayerLifeChangedEvent = new UnityEvent<int, int>();
    }

    private void Initialize()
    {
        //register event listeners
    }

    private void OnPlayerScore(int index, int score)
    {
        PlayerScoredEvent?.Invoke(index, score);
    }

    public void PlayerScored(int index, int addScoreBy)
    {
        Score += addScoreBy;
        OnPlayerScore(index, Score);
    }

    public void PlayerGhostTouch(int index)
    {

    }
}
