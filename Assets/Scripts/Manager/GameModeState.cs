using Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public enum GameplayState
    {
        Standby,
        Pause,
        Gameplay,
        GameOver
    }

    public class GameModeState
    {
        private Dictionary<int, int> m_pcToScoreDictionary;
        public int Score = 0;
        public int Health = 0;

        public UnityEvent<int, int> PlayerScoredEvent {  get; private set; }
        public UnityEvent<int, int> PlayerLifeChangedEvent { get; private set; }

        public void Initialize()
        {
            m_pcToScoreDictionary = new Dictionary<int, int>();

            PlayerScoredEvent = new UnityEvent<int, int>();
            PlayerLifeChangedEvent = new UnityEvent<int, int>();
        }

        public void RegisterPlayer(int playerId)
        {
            m_pcToScoreDictionary.Add(playerId, 0);
        }

        public void UpdatePlayerScore(int pcIndex, int newScore)
        {
            if (m_pcToScoreDictionary.TryGetValue(pcIndex, out int score))
            {
                m_pcToScoreDictionary[pcIndex] = newScore;
            }
        }

        public int GetPlayerScore(int pcIndex) 
        {
            if (m_pcToScoreDictionary.TryGetValue(pcIndex, out int score))
            {
                return score;
            }

            return int.MinValue;
        }
    }
}