using Eflatun.SceneReference;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using USceneMgr = UnityEngine.SceneManagement.SceneManager;

namespace Game
{
    public class SceneManager : IManager
    {
        SceneManagerState m_state = new SceneManagerState();

        public static SceneManager CreateInstance()
        {
            return new SceneManager();
        }

        #region IManager
        public Type GetManagerType()
        {
            return typeof(SceneManager);
        }

        public void PreStateChange(GameInstanceStates currentState)
        {
            
            switch (currentState)
            {
                case GameInstanceStates.None:

                    break;
                case GameInstanceStates.Menu:
                    break;
                case GameInstanceStates.CharSelect:
                    break;
                case GameInstanceStates.Gameplay:
                    break;
                case GameInstanceStates.Result:
                    break;
                case GameInstanceStates.Loading:
                    
                    break;
                default:
                    break;
            }
        }

        public void OnStateChange(GameInstanceStates prevState, GameInstanceStates currState, GameInstanceStates nextState)
        {
            //curr state will always be loading
            LoadScene(m_state.LoadingScene);

            switch (nextState)
            {
                case GameInstanceStates.None:
                    break;
                case GameInstanceStates.Menu:
                    LoadScene(m_state.MasterSceneList.SceneList[1]);
                    break;
                case GameInstanceStates.CharSelect:
                    break;
                case GameInstanceStates.Gameplay:
                    break;
                case GameInstanceStates.Result:
                    break;
                case GameInstanceStates.Loading:
                    break;
                default:
                    break;
            }
        }

        public void PostStateChange(GameInstanceStates nextState)
        {
            switch (nextState)
            {
                case GameInstanceStates.None:
                    break;
                case GameInstanceStates.Menu:
                    break;
                case GameInstanceStates.CharSelect:
                    break;
                case GameInstanceStates.Gameplay:
                    break;
                case GameInstanceStates.Result:
                    break;
                case GameInstanceStates.Loading:
                    break;
                default:
                    break;
            }
        }

        public void RegisterManager(GameManager mgr)
        {
            var dataMgr = mgr.GetManager<DataManager>(ManagerType.Data);

            m_state.SetSceneListRef(dataMgr.MasterDataList.SceneData);

            USceneMgr.activeSceneChanged += OnActiveSceneSwitched;
            USceneMgr.sceneUnloaded += OnSceneUnloaded;
            USceneMgr.sceneLoaded += OnSceneLoaded;

            m_state.Initialize();
        }
        public IManager RetrieveInterface()
        {
            return this;
        }

        public void ShutdownManager(GameManager mgr)
        {
            USceneMgr.activeSceneChanged -= OnActiveSceneSwitched;
            USceneMgr.sceneUnloaded -= OnSceneUnloaded;
            USceneMgr.sceneLoaded -= OnSceneLoaded;
        }
        #endregion

        private void FirstSceneSwitch()
        {

        }

        private void LoadScene(CustomSceneData data)
        {
            USceneMgr.LoadScene(data.SceneObj.Name, data.LoadSceneMode);
        }

        #region Default Unity Scene Manager
        #endregion

        #region By Addressables
        #endregion

        #region Events
        void OnActiveSceneSwitched(Scene prev, Scene newScene)
        {
            Debug.Log($"Scene switch from {prev.name} to {newScene.name}");
        }


        private void OnSceneLoaded(Scene newScene, LoadSceneMode mode)
        {
            Debug.Log($"Scene unloded from {newScene.name}");
        }

        void OnSceneUnloaded(Scene prev)
        {
            Debug.Log($"Scene unloded from {prev.name}");
        }
        #endregion
    }
}