using Client;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class GameModeBase : MonoBehaviour
    {
        private GameModeState m_data = new GameModeState();

        //temp
        public static GameModeBase gameMode;
        public GameModeData StageData;

        [ReadOnly]
        private GameBoardInstance m_gameBoard;

        private List<GhostBehaviourBase> m_ghosts = new List<GhostBehaviourBase>();

        public UnityEvent<int, int> EPlayerScored => m_data.PlayerScoredEvent;
        public UnityEvent<int, int> EPlayerLifeChanged => m_data.PlayerLifeChangedEvent;

        #region Set Up
        private void OnValidate()
        {
            m_gameBoard = FindObjectOfType<GameBoardInstance>();
        }
        private void Awake()
        {
            gameMode = this;
            m_gameBoard = FindObjectOfType<GameBoardInstance>();
        }

        private void Start()
        {
            StartCoroutine(StartInitializeProcess());
        }

        /// <summary>
        /// Async start process, so can wait for things to happen one by one
        /// </summary>
        /// <returns></returns>
        private IEnumerator StartInitializeProcess()
        {
            m_data.Initialize();

            yield return new WaitForSeconds(0.01f);

            //wait until game manager isnt null
            yield return GameManager.Instance != null;
            yield return GameManager.Instance.GetManager<DataManager>(ManagerType.Data);

            //get data manager
            var dMgr = GameManager.Instance.GetManager<DataManager>(ManagerType.Data);

            if (dMgr != null)
            {
#if UNITY_EDITOR
                InstantiatePlayer(0);
#endif
                //spawn ghosts!!
                InstantiateGhosts(4);
            }
            yield return null;
        }

        /// <summary>
        /// For Debug
        /// </summary>
        /// <param name="numToInstantiate"></param>
        private void InstantiatePlayers_EditorTest(int numToInstantiate)
        {
            for (int i = 0; i < numToInstantiate; i++)
            {
                InstantiatePlayer(i);
            }
        }

        private PlayerController InstantiatePlayer(int ind = 0)
        {
            var dMgr = GameManager.Instance.GetManager<DataManager>(ManagerType.Data);

            if (dMgr != null)
            {
                var masterList = dMgr.MasterDataList;

                //spawn player controller
                PlayerController pc = Instantiate(masterList.PlayerControllerPrefab, Vector3.zero, Quaternion.identity);
                m_data.RegisterPlayer(pc);
                m_data.UpdatePlayerHealth(ind, StageData.StartingHealth);

                //spawn character
                PlayerBehaviour character = Instantiate(masterList.PlayerCharacterPrefab);
                character.SetData(masterList.m_charModelDataList[0]);

                pc.PossessCharacter(character);

                return pc;
            }

            return null;
        }

        public void RegisterPlayer(PlayerController player)
        {
            m_data.RegisterPlayer(player);
        }

        private void InstantiateGhosts(int numToInstantiate = 1)
        {
            for (int i = 0; i < numToInstantiate; i++)
            {
                m_ghosts.Add(InstantiateGhost(i));
            }
        }

        private GhostBehaviourBase InstantiateGhost(int ind = 0)
        {
            var dMgr = GameManager.Instance.GetManager<DataManager>(ManagerType.Data);

            if (dMgr != null)
            {
                var masterList = dMgr.MasterDataList;

                GhostBehaviourBase g = Instantiate(masterList.GhostPrefab, Vector3.zero, Quaternion.identity);

                //spawn character
                g.SetData(masterList.m_ghostModelDataList[0]);

                //set position
                g.transform.position = m_gameBoard.GetRandomPointInGhostSpawnZone();
                return g;
            }

            return null;
        }
        #endregion

        public void OnPlayerScored(int index, int addScoreBy)
        {
            int newScore = m_data.GetPlayerScore(index) + addScoreBy;
            m_data.UpdatePlayerScore(index, newScore);
            EPlayerScored?.Invoke(index, newScore);
        }

        public void PlayerLoseHealth(int index)
        {
            int newHealth = m_data.GetPlayerHealth(index) - 1;
            m_data.UpdatePlayerHealth(index, newHealth);

            EPlayerLifeChanged?.Invoke(index, newHealth);

            if (newHealth <= 0)
            {
                Debug.Log("GameOver!");
            }
        }

        public void GhostHasDied(GhostBehaviourBase ghost)
        {

        }
    }
}