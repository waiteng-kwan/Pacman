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
        Standby = 0,
        Pause,
        TransitToGameplay,
        Gameplay,
        GameOver
    }

    public class GameModeState
    {
        private Dictionary<int, IPlayerAttributes> m_pcToStatDictionary =
            new Dictionary<int, IPlayerAttributes>();

        public UnityEvent<int, int> EPlayerScored { get; private set; } =
            new UnityEvent<int, int>();
        public UnityEvent<int, int> EPlayerLifeChanged { get; private set; } =
            new UnityEvent<int, int>();

        //state
        public GameplayState CurrentState { get; private set; } = GameplayState.Standby;
        public GameplayState PrevState { get; private set; } = GameplayState.Standby;
        public UnityEvent EGameplayStateChanged { get; private set; } =
            new UnityEvent();

        //time
        private float m_elapsedTime = 0f;

        public bool RegisterPlayer(PlayerController pc, int index = 0)
        {
            if (m_pcToStatDictionary.TryGetValue(pc.Index, out var stat))
            {
                Debug.Log($"PC index {pc.Index} already registered!");
                return false;
            }

            IPlayerAttributes attrib = pc.GetComponent<PlayerControllerAttributes>();

            attrib.SetHealth(GameModeBase.Instance.Settings.StartingHealth);

            m_pcToStatDictionary.Add(pc.Index, attrib);

            pc.SetIndex(index);
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