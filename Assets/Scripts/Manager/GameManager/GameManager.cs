using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        private GameManagerAttributes m_data = new GameManagerAttributes();

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
            m_data.RegisterStateChangeCallback(ChangeState);
        }

        public void ChangeState(GameInstanceStates newState)
        {

        }

        private void InitializeState()
        {

        }
    }
}