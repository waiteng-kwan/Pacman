using Client;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Rendering.InspectorCurveEditor;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        private GameManagerState m_data = new GameManagerState();

        [Header("Debug purposes")]
        [SerializeField]
        private GameInstanceStates m_startStateDEBUG = GameInstanceStates.Menu;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            m_data.Initialize();
            m_data.RegisterStateChangeCallback(OnStateChange);
        }

        /// <summary>
        /// The following list shows the execution order of the RuntimeInitializeLoadType callbacks:
        /// 1. First various low level systems are initialized(window, assemblies, gfx etc.)
        /// 2. Then SubsystemRegistration and AfterAssembliesLoaded callbacks are invoked.
        /// 3. More setup(input systems etc.)
        /// 4. Then BeforeSplashScreen callback is invoked.
        /// 5. Now the first scene starts loading
        /// 6. Then BeforeSceneLoad callback is invoked.Here objects of the scene is loaded but Awake() has not been called yet. All objects are considered inactive here.
        /// 7. Now Awake() and OnEnable() are invoked on MonoBehaviours.
        /// 8. Then AfterSceneLoad callback is invoked.Here objects of the scene are considered fully loaded and setup. Active objects can be found with FindObjectsByType.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnAfterFirstSceneLoad()
        {
            Debug.Log("First scene has loaded! 1 time only");

            Instance.m_data.Services = new Services();
            Instance.m_data.Services.Initialize();

            var mgrs = Instance.m_data.MgrList;

            //create all subsystems
            mgrs.Add(ManagerType.Scene, SceneManager.CreateInstance());
            mgrs.Add(ManagerType.Data, DataManager.CreateInstance(Instance.gameObject));
            mgrs.Add(ManagerType.Audio, AudioManager.CreateInstance());

            //register subsystems
            foreach(var elem in mgrs.Values)
            {
                IManager m = elem as IManager;

                if(m != null)
                {
                    m.RegisterManager(Instance);
                }
            }

            //switch out state
#if DEV || UNITY_EDITOR || DEVELOPMENT_BUILD
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Initialization")
            {
                Instance.ChangeState(Instance.m_startStateDEBUG);
            }
#else
            Instance.ChangeState(GameInstanceStates.Menu);
#endif
        }

        public void ChangeState(GameInstanceStates newState)
        {
            //prep state change stuff here
            foreach (var mgr in Instance.m_data.MgrList.Values)
            {
                mgr.PreStateChange(m_data.CurrentState);
            }

            //data change state
            m_data.ChangeState(newState);

            //after state change stuff
            foreach (var mgr in Instance.m_data.MgrList.Values)
            {
                mgr.PostStateChange(newState);
            }
        }

        private void OnStateChange(GameInstanceStates prevState,
            GameInstanceStates currState, GameInstanceStates nextState)
        {
            print("henshin1");

            foreach(var mgr in  Instance.m_data.MgrList.Values)
            {
                mgr.OnStateChange(prevState, currState, nextState);
            }
        }

        private void InitializeState()
        {

        }

        public T GetManager<T>(ManagerType type)
        {
            return m_data.GetManager<T>(type);
        }

        private void OnApplicationQuit()
        {
            if(Services.Instance != null)
                Services.Instance.Shutdown();

            foreach (var elem in m_data.MgrList.Values)
            {
                IManager m = elem as IManager;

                if (m != null)
                {
                    m.ShutdownManager(Instance);
                }
            }
        }

    }
}