using Client;
using System;
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

    public struct PlayerStats
    {
        public int Score;
        public int Health;
        public PlayerControllerAttributes Attributes { get; private set; }

        public PlayerStats(PlayerControllerAttributes attributes)
        {
            Attributes = attributes;
            Score = 0;
            Health = 0;
        }

        public void UpdatePlayerAttributes(PlayerControllerAttributes attrib)
        {
            Attributes = attrib;
        }
    }

    public class GameModeState
    {
        private Dictionary<int, PlayerStats> m_pcToStatDictionary;
        public int Score = 0;
        public int Health = 0;

        public UnityEvent<int, int> PlayerScoredEvent {  get; private set; }
        public UnityEvent<int, int> PlayerLifeChangedEvent { get; private set; }

        public void Initialize()
        {
            m_pcToStatDictionary = new Dictionary<int, PlayerStats>();

            PlayerScoredEvent = new UnityEvent<int, int>();
            PlayerLifeChangedEvent = new UnityEvent<int, int>();
        }

        public void RegisterPlayer(PlayerController pc)
        {
            PlayerControllerAttributes attrib = pc.GetComponent<PlayerControllerAttributes>();

            attrib.UpdateHealth(GameModeBase.gameMode.StageData.StartingHealth);

            m_pcToStatDictionary.Add(pc.Index, new PlayerStats(attrib));
        }

        public void UpdatePlayerScore(int pcIndex, int newScore)
        {
            if (m_pcToStatDictionary.TryGetValue(pcIndex, out PlayerStats stat))
            {
                stat.Score = newScore;
            }
        }

        public int GetPlayerScore(int pcIndex) 
        {
            if (m_pcToStatDictionary.TryGetValue(pcIndex, out PlayerStats stat))
            {
                return stat.Score;
            }

            return int.MinValue;
        }

        public void UpdatePlayerHealth(int pcIndex, int newHealth)
        {
            if (m_pcToStatDictionary.TryGetValue(pcIndex, out PlayerStats stat))
            {
                stat.Health = newHealth;
            }
        }

        public int GetPlayerHealth(int pcIndex)
        {
            if (m_pcToStatDictionary.TryGetValue(pcIndex, out PlayerStats stat))
            {
                return stat.Health;
            }

            return int.MinValue;
        }

        public PlayerStats GetStats(int pcIndex)
        {
            if (m_pcToStatDictionary.TryGetValue(pcIndex, out PlayerStats stat))
            {
                return stat;
            }

            return default(PlayerStats);
        }
    }
}