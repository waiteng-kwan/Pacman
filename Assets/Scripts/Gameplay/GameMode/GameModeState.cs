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

    public class GameModeState
    {
        private Dictionary<int, IPlayerAttributes> m_pcToStatDictionary;
        public int Score = 0;
        public int Health = 0;

        public UnityEvent<int, int> PlayerScoredEvent { get; private set; }
        public UnityEvent<int, int> PlayerLifeChangedEvent { get; private set; }

        public void Initialize()
        {
            m_pcToStatDictionary = new Dictionary<int, IPlayerAttributes>();

            PlayerScoredEvent = new UnityEvent<int, int>();
            PlayerLifeChangedEvent = new UnityEvent<int, int>();
        }

        public void RegisterPlayer(PlayerController pc)
        {
            if (m_pcToStatDictionary.TryGetValue(pc.Index, out var stat))
            {
                Debug.Log($"PC index {pc.Index} already registered!");
                return;
            }

            IPlayerAttributes attrib = pc.GetComponent<PlayerControllerAttributes>();

            attrib.SetHealth(GameModeBase.gameMode.StageData.StartingHealth);

            m_pcToStatDictionary.Add(pc.Index,attrib );
        }

        public void UpdatePlayerScore(int pcIndex, int newScore)
        {
            if (m_pcToStatDictionary.TryGetValue(pcIndex, out IPlayerAttributes stat))
            {
                stat.SetScore(newScore);
            }
        }

        public int GetPlayerScore(int pcIndex)
        {
            if (m_pcToStatDictionary.TryGetValue(pcIndex, out IPlayerAttributes stat))
            {
                return stat.Score;
            }

            return int.MinValue;
        }

        public void UpdatePlayerHealth(int pcIndex, int newHealth)
        {
            if (m_pcToStatDictionary.TryGetValue(pcIndex, out IPlayerAttributes stat))
            {
                stat.SetHealth(newHealth);
            }
        }

        public int GetPlayerHealth(int pcIndex)
        {
            if (m_pcToStatDictionary.TryGetValue(pcIndex, out IPlayerAttributes stat))
            {
                return stat.Health;
            }

            return int.MinValue;
        }

        public IPlayerAttributes GetStats(int pcIndex)
        {
            if (m_pcToStatDictionary.TryGetValue(pcIndex, out IPlayerAttributes stat))
            {
                return stat;
            }

            return default(IPlayerAttributes);
        }
    }
}