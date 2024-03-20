using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public enum GameInstanceStates
    {
        None = 0,
        Menu,
        CharSelect,
        Gameplay,
        Result,
        Loading
    }

    public enum ManagerType
    {
        Scene,
        Audio,
        Data,
        Ui,
        Fx,   //might split into vfx and sfx
        Player,
        Input
    }

    public class GameManagerState
    {
        public GameInstanceStates PreviousState { get; private set; }
        public GameInstanceStates CurrentState { get; private set; }
        public GameInstanceStates NextState { get; private set; }
        //the current state is always loading
        private Action<GameInstanceStates, GameInstanceStates,
            GameInstanceStates> m_stateChangeCallback;

        //managers
        private Dictionary<ManagerType, IManager> m_managers;
        public Dictionary<ManagerType, IManager> MgrList => m_managers;

        public Service.Services Services;

        public void Initialize()
        {
            PreviousState = CurrentState = NextState =
                GameInstanceStates.None;

            m_managers = new Dictionary<ManagerType, IManager>();
        }

        public void Shutdown()
        {
            m_stateChangeCallback = null;
        }

        public void RegisterStateChangeCallback(Action<GameInstanceStates,
            GameInstanceStates, GameInstanceStates> callback)
        {
            m_stateChangeCallback += callback;
        }

        public T GetManager<T>(ManagerType managerType)
        {
            if(m_managers.TryGetValue(managerType, out var manager))
            {
                return (T)manager;
            }

            return default;
        }

        public void ChangeState(GameInstanceStates newState)
        {
            PreviousState = CurrentState;
            CurrentState = GameInstanceStates.Loading;
            NextState = newState;

            m_stateChangeCallback(PreviousState, CurrentState, NextState);
        }
    }
}