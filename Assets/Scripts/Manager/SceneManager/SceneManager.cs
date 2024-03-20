using Eflatun.SceneReference;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            var toUnload = m_state.ActiveSceneQueue;

            foreach (var sceneData in toUnload)
            {
                m_state.AddSceneToUnload(sceneData);
            }
        }

        public void OnStateChange(GameInstanceStates prevState, GameInstanceStates currState, GameInstanceStates nextState)
        {
            //curr state will always be loading
            if (USceneMgr.GetActiveScene().name != "Initialization")
                LoadScene(m_state.LoadingScene);

            List<CustomSceneData> scene = new List<CustomSceneData>();
            switch (nextState)
            {
                case GameInstanceStates.None:
                    break;
                case GameInstanceStates.Menu:
                    scene = m_state.MasterSceneList.Find(GameInstanceStates.Menu);
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

            foreach (var s in scene)
            {
                LoadScene(s);
            }
        }

        public void PostStateChange(GameInstanceStates nextState)
        {
            if(GetAllScenes().Count() > 2)
                UnloadScene(m_state.LoadingScene);

            foreach (var sceneData in m_state.NextToUnloadSceneQueue)
            {
                UnloadScene(sceneData);
            }
        }

        public IManager RetrieveInterface()
        {
            return this;
        }

        public void RegisterManager(GameManager mgr)
        {
            var dataMgr = mgr.GetManager<DataManager>(ManagerType.Data);

            m_state.SetSceneListRef(dataMgr.MasterDataList.SceneData);

            USceneMgr.activeSceneChanged += OnActiveSceneSwitched;
            USceneMgr.sceneUnloaded += OnSceneUnloaded;
            USceneMgr.sceneLoaded += OnSceneLoaded;

            var currScenes = GetAllScenes();

            //first load
            foreach (var s in currScenes)
            {
                var data = m_state.MasterSceneList.Find(s.name);
                m_state.AddActiveScenes(data);
            }

            m_state.Initialize();
        }

        public void ShutdownManager(GameManager mgr)
        {
            USceneMgr.activeSceneChanged -= OnActiveSceneSwitched;
            USceneMgr.sceneUnloaded -= OnSceneUnloaded;
            USceneMgr.sceneLoaded -= OnSceneLoaded;
        }
        #endregion

        private void LoadScene(CustomSceneData data, bool async = true)
        {
            if (async)
            {
                LoadNonAddressableSceneAsync(data);
            }
            else
            {
                USceneMgr.LoadScene(data.SceneObj.Name, data.LoadSceneMode);
            }
        }

        private void UnloadScene(CustomSceneData data)
        {
            UnloadNonAddressableSceneAsync(data);
        }

        #region Default Unity Scene Manager
        Scene[] GetAllScenes()
        {
            Scene[] rt = new Scene[USceneMgr.sceneCount];

            for (int i = 0; i < USceneMgr.sceneCount; i++)
            {
                rt[i] = USceneMgr.GetSceneAt(i);
            }

            return rt;
        }

        void LoadNonAddressableSceneAsync(CustomSceneData data)
        {
            AsyncOperation handle = USceneMgr.LoadSceneAsync(data.SceneName, data.LoadSceneMode);
            handle.completed += OnNonAddressableSceneLoaded;
        }

        private void OnNonAddressableSceneLoaded(AsyncOperation operation)
        {
            //clean up
            operation.completed -= OnNonAddressableSceneLoaded;

            if (operation.isDone)
            {
                Debug.Log("whee");
            }
        }

        void UnloadNonAddressableSceneAsync(CustomSceneData data)
        {
            try
            {
                AsyncOperation handle = USceneMgr.UnloadSceneAsync(data.SceneObj.Name);

                handle.completed += OnNonAddressableSceneUnloaded;
            }
            catch (Exception e)
            {
                Debug.Log("Error unloading async with unity scene manager " + e);
            }
        }


        private void OnNonAddressableSceneUnloaded(AsyncOperation operation)
        {
            //clean up
            operation.completed -= OnNonAddressableSceneUnloaded;

            if (operation.isDone)
            {
                Debug.Log("boooo");
            }
        }

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
            Debug.Log($"Scene loaded {newScene.name}");
        }

        void OnSceneUnloaded(Scene prev)
        {
            Debug.Log($"Scene unloaded {prev.name}");
        }
        #endregion
    }
}