using Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SocialPlatforms.Impl;

namespace Game
{
    public class GameMode : MonoBehaviour
    {
        private GameModeState m_data = new GameModeState();
        //temp
        public static GameMode gameMode;

        public int Health = 0;

        public Collider GhostRespawnZone;

        public UnityEvent<int, int> EPlayerScored => m_data.PlayerScoredEvent;
        public UnityEvent<int, int> EPlayerLifeChanged => m_data.PlayerLifeChangedEvent;

        private void Awake()
        {
            gameMode = this;

            m_data.Initialize();
        }

        private void Initialize()
        {
            //register event listeners
        }

        public void RegisterPlayer(PlayerController player)
        {
            m_data.RegisterPlayer(player.Index);
        }

        public void OnPlayerScored(int index, int addScoreBy)
        {
            int newScore = m_data.GetPlayerScore(index) + addScoreBy;
            m_data.UpdatePlayerScore(index, newScore);
            EPlayerScored?.Invoke(index, newScore);
        }

        public void PlayerLoseHealth(int index)
        {
            Health -= 1;

            EPlayerLifeChanged?.Invoke(index, Health);

            if (Health <= 0)
            {
                Debug.Log("GameOver!");
            }
        }

        public void GhostHasDied(GhostBehaviourBase ghost)
        {

        }
    }
}