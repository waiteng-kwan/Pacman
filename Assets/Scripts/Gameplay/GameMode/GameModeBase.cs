using Client;
using Core.Ghost;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Core
{
    public class GameModeBase : MonoBehaviour, IGameMode
    {
        private GameModeState m_data = new GameModeState();

        //temp
        static GameModeBase m_instance;
        public static GameModeBase Instance => m_instance;
        [field: SerializeField]
        public GameModeData Settings { get; private set; }

        [field: SerializeField, ReadOnly]
        public GameBoardInstance Board { get; private set; }

        private List<GhostBehaviourBase> m_ghosts = new List<GhostBehaviourBase>();

        public UnityEvent<int, int> EPlayerScored => m_data.EPlayerScored;
        public UnityEvent<int, int> EPlayerLifeChanged => m_data.EPlayerLifeChanged;
        public UnityEvent EGameStateChanged => m_data.EGameplayStateChanged;

        #region Set Up
        private void OnValidate()
        {
            Board = FindFirstObjectByType<GameBoardInstance>();
        }
        private void Awake()
        {
            m_instance = this;
            Board = FindFirstObjectByType<GameBoardInstance>();
        }

        private void Start()
        {
            m_data.EGameplayStateChanged.AddListener(OnGameStateChange);

            m_data.SetState(GameplayState.Standby);
        }

        private void Update()
        {

        }

        private void OnGameStateChange()
        {
            GameplayState currState = m_data.CurrentState;
            GameplayState prevState = m_data.PrevState;

            switch (currState)
            {
                case GameplayState.Standby:
                    StartCoroutine(StartInitializeProcess());
                    break;
                case GameplayState.Pause:
                    break;
                //321
                case GameplayState.TransitToGameplay:
                    print("321 go!");

                    //ready up stuff here

                    //then go to game play
                    m_data.SetState(GameplayState.Gameplay);
                    break;
                case GameplayState.Gameplay:
                    foreach (var g in m_ghosts)
                    {
                        g.GetComponent<GhostAiController>().SetNextState(GhostAiState.Chasing);
                    }
                    break;
                case GameplayState.GameOver:
                    break;
                default:
                    break;
            }
        }


        /// <summary>
        /// Async start process, so can wait for things to happen one by one
        /// THIS IS FOR THE STAGE INIT
        /// </summary>
        /// <returns></returns>
        private IEnumerator StartInitializeProcess()
        {
            yield return new WaitForSeconds(0.01f);

            //wait until game manager isnt null
            yield return GameManager.Instance != null;
            yield return GameManager.GetManager<DataManager>() != null;

            //get data manager
            var dMgr = GameManager.GetManager<DataManager>();

            if (dMgr != null)
            {
#if UNITY_EDITOR
                InstantiatePlayer(0);
#endif
                //spawn ghosts!!
                InstantiateGhosts(4);

                //instantiate UI
                Instantiate(Settings.StageUI, Vector3.zero, Quaternion.identity);
            }

            //init done, wait for stuff

            //wait for count down
            yield return new WaitForSeconds(Settings.SecondsBeforeGameStart);

            //move to next state
            m_data.SetState(GameplayState.TransitToGameplay);

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
            var dMgr = GameManager.GetManager<DataManager>();

            if (dMgr != null)
            {
                var masterList = dMgr.MasterDataList;

                //spawn player controller
                PlayerController pc = Instantiate(masterList.PlayerControllerPrefab, Vector3.zero, Quaternion.identity);
                m_data.RegisterPlayer(pc, ind);
                m_data.UpdatePlayerHealth(ind, Settings.StartingHealth);

                //get player spawn position
                Vector3 spawnPos = Board.GetPlayerSpawnPoint();

                //spawn character
                PacmanBehaviour character = Instantiate(masterList.PlayerCharacterPrefab, spawnPos, Quaternion.identity);
                character.SetSettingsData(masterList.CharModelDataList[0]);

                pc.PossessPawn(character);

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
            var dMgr = GameManager.GetManager<DataManager>();

            if (dMgr != null)
            {
                var masterList = dMgr.MasterDataList;

                GhostBehaviourBase g = Instantiate(masterList.GhostPrefab, Vector3.zero, Quaternion.identity);

                //spawn character
                g.SetSettingsData(masterList.GhostModelDataList[0]);

                //set position
                (Vector3 pos, Collider c) = Board.GetRandomPointAndGhostSpawnZone();
                g.transform.position = pos;
                g.PickRespawnZone(c);
                g.SetIsAI();

                return g;
            }

            return null;
        }
        #endregion

        public void GhostDied(GhostBehaviourBase ghost)
        {
            ghost.Die();

            //pick random respawn zone
            Collider col = Board.GetGhostSpawnZone(0, true);
            ghost.PickRespawnZone(col);
        }

        public void SetGameBoardInstance(GameBoardInstance board)
        {
            Board = board;
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
            //pc.PlayerCharacter.Attributes.SetState(PacmanStates.Dead);

            StartPlayerRespawn(pc);
        }

        public void StartPlayerRespawn(PlayerController pc)
        {
            pc.Pawn.gameObject.SetActive(false);
            //pc.PlayerCharacter.Attributes.SetState(PacmanStates.Respawning);
        }

        public void PlayerCollidedWithGhost(GhostBehaviourBase ghost,
            PacmanBehaviour pChar)
        {
            //check current state
            if (pChar.Attributes.CanEatGhosts)
            {
                PlayerScored(pChar.OwnerIndex, 5);
                GhostDied(ghost);
            }
            else
            {
                //get eaten
                PlayerDecHealth(pChar.OwnerIndex);
                PlayerDied(pChar.Owner as PlayerController);
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
            return Board.CanGameEnd();
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