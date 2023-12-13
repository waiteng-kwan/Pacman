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
        TransitToGameplay,
        Gameplay,
        GameOver
    }

    public class GameModeState
    {
        private Dictionary<int, IPlayerAttributes> m_pcToStatDictionary;

        public UnityEvent<int, int> EPlayerScored { get; private set; }
        public UnityEvent<int, int> EPlayerLifeChanged { get; private set; }

        //state
        public GameplayState CurrentState { get; private set; }
        public GameplayState PrevState { get; private set; }
        public UnityEvent EGameplayStateChanged { get; private set; }

        //time
        private float m_elapsedTime = 0f;

        public void Initialize()
        {
            m_pcToStatDictionary = new Dictionary<int, IPlayerAttributes>();

            EPlayerScored = new UnityEvent<int, int>();
            EPlayerLifeChanged = new UnityEvent<int, int>();
            EGameplayStateChanged = new UnityEvent();
        }

        public bool RegisterPlayer(PlayerController pc)
        {
            if (m_pcToStatDictionary.TryGetValue(pc.Index, out var stat))
            {
                Debug.Log($"PC index {pc.Index} already registered!");
                return false;
            }

            IPlayerAttributes attrib = pc.GetComponent<PlayerControllerAttributes>();

            attrib.SetHealth(GameModeBase.Instance.Settings.StartingHealth);

            m_pcToStatDictionary.Add(pc.Index, attrib);
            return true;
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

        public void SetState(GameplayState newState)
        {
            PrevState = CurrentState;
            CurrentState = newState;
            EGameplayStateChanged?.Invoke();
        }
    }
}