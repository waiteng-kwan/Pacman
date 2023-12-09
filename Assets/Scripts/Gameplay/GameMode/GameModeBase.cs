using Client;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class GameModeBase : MonoBehaviour, IGameMode
    {
        private GameModeState m_data = new GameModeState();

        //temp
        static GameModeBase m_instance;
        public static GameModeBase Instance => m_instance;
        [SerializeField] private GameModeData m_settings;
        public GameModeData Settings => m_settings;

        [ReadOnly]
        private GameBoardInstance m_gameBoard;
        public GameBoardInstance Board => m_gameBoard;

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
            m_instance = this;
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
                InstantiateGhosts(1);

                Instantiate(m_settings.StageUI, Vector3.zero, Quaternion.identity);
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
                m_data.UpdatePlayerHealth(ind, Settings.StartingHealth);

                //spawn character
                PlayerBehaviour character = Instantiate(masterList.PlayerCharacterPrefab);
                character.SetData(masterList.m_charModelDataList[0]);

                pc.PossessCharacter(character);

                return pc;
            }

            return null;
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

        public void GhostDied(GhostBehaviourBase ghost)
        {
            print("Ghost died");
            ghost.Die();
        }

        public void SetGameBoardInstance(GameBoardInstance board)
        {
            m_gameBoard = board;
        }

        #region Player Crap
        public bool RegisterPlayer(PlayerController pc)
        {
            return m_data.RegisterPlayer(pc);
        }

        public void PlayerScored(int index, int addScoreBy = 1)
        {
            int newScore = m_data.GetPlayerScore(index) + addScoreBy;
            m_data.UpdatePlayerScore(index, newScore);
            EPlayerScored?.Invoke(index, newScore);

            if (CanGameEnd())
            {
                GameEnded();
            }
        }
        #region Health
        public void PlayerUpdateHealth(int index, int addHpBy = -1)
        {
            int newHealth = m_data.GetPlayerHealth(index) + addHpBy;
            m_data.UpdatePlayerHealth(index, newHealth);

            EPlayerLifeChanged?.Invoke(index, newHealth);

            if (newHealth <= 0)
            {
                Debug.Log("GameOver!");
                GameEnded();
            }
        }

        public void PlayerDecHealth(int index)
        {
            PlayerUpdateHealth(index);
        }

        public void PlayerIncHealth(int index)
        {
            PlayerUpdateHealth(index, 1);
        }
        #endregion

        public void PlayerDied(PlayerController pc)
        {
            //StartPlayerRespawn();
        }

        public void StartPlayerRespawn(PlayerController pc)
        {
            pc.PlayerCharacter.gameObject.SetActive(false);
            pc.PlayerCharacter.Attributes.SetState(PlayerCharacterStates.Invul);
        }

        public void PlayerCollidedWithGhost(GhostBehaviourBase ghost,
            PlayerBehaviour pChar)
        {
            //check current state
            if (pChar.Attributes.CanEatGhosts)
            {
                PlayerScored(pChar.BelongToPlayerIndex, 5);
                GhostDied(ghost);
            }
            else
            {
                //get eaten
                PlayerDecHealth(pChar.BelongToPlayerIndex);
                PlayerDied(pChar.Owner);
                

                //temp
                StartPlayerRespawn(pChar.Owner);
                pChar.Attributes.SetState(PlayerCharacterStates.Respawning);
            }
        }
        #endregion

        public GhostBehaviourBase SpawnGhost(int ind = 0)
        {
            throw new NotImplementedException();
        }

        public void StartSetupGameMode()
        {
            throw new NotImplementedException();
        }

        public IEnumerator GameModeSetupProcess()
        {
            throw new NotImplementedException();
        }

        public void GameEnded()
        {
            print("Game Ended! You win!");
        }

        public void GameStarted()
        {
            throw new NotImplementedException();
        }

        public void GamePaused()
        {
            throw new NotImplementedException();
        }

        public bool IsGamePaused()
        {
            throw new NotImplementedException();
        }

        public bool CanGameEnd()
        {
            return m_gameBoard.CanGameEnd();
        }

        public int CalculateWhichPlayerWon()
        {
            throw new NotImplementedException();
        }

        public void RegisterSubsystem(GameModeBase gameMode)
        {
            throw new NotImplementedException();
        }
    }
}