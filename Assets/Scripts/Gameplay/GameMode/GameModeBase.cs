using Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SocialPlatforms.Impl;

namespace Game
{
    public class GameModeBase : MonoBehaviour
    {
        private GameModeState m_data = new GameModeState();
         

        //temp
        public static GameModeBase gameMode;
        public GameModeData StageData;

        public Collider[] GhostRespawnZone;

        private List<GhostBehaviourBase> m_ghosts = new List<GhostBehaviourBase>();

        public UnityEvent<int, int> EPlayerScored => m_data.PlayerScoredEvent;
        public UnityEvent<int, int> EPlayerLifeChanged => m_data.PlayerLifeChangedEvent;

        private void Awake()
        {
            gameMode = this;

            m_data.Initialize();
        }

        private void Start()
        {
            Initialize();        }

        private void Initialize()
        {
            //register event listeners

            SpawnGhost();
        }

        public void RegisterPlayer(PlayerController player)
        {
            m_data.RegisterPlayer(player);
        }

        public void OnPlayerScored(int index, int addScoreBy)
        {
            int newScore = m_data.GetPlayerScore(index) + addScoreBy;
            m_data.UpdatePlayerScore(index, newScore);
            EPlayerScored?.Invoke(index, newScore);
        }

        public void PlayerLoseHealth(int index)
        {
            m_data.UpdatePlayerHealth(index, 
                m_data.GetStats(index).Health - 1);

            EPlayerLifeChanged?.Invoke(index,
                m_data.GetStats(index).Health);

            if (m_data.GetStats(index).Health <= 0)
            {
                Debug.Log("GameOver!");
            }
        }

        public void SpawnGhost()
        {
            for(int i = 0; i < StageData.Ghosts.Length; i++)
            {
                GhostBehaviourBase ghost = Instantiate(StageData.Ghosts[i].ParentGhostPrefab, GhostRespawnZone[0].transform.position, Quaternion.identity);

                m_ghosts.Add(ghost);
            }
        }

        public void GhostHasDied(GhostBehaviourBase ghost)
        {

        }
    }
}