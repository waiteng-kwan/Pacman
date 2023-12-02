using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public enum GameInstanceStates
    {
        None,
        Menu,
        CharSelect,
        Gameplay,
        Result
    }

    public class GameManagerAttributes : MonoBehaviour
    {
        public GameInstanceStates PreviousState { get; private set; }
        public GameInstanceStates CurrentState { get; private set; }
        public GameInstanceStates NextState { get; private set; }
        private Action<GameInstanceStates> m_stateChangeCallback;

        public void Initialize()
        {
            PreviousState = CurrentState = NextState =
                GameInstanceStates.None;
        }

        public void RegisterStateChangeCallback(Action<GameInstanceStates> callback)
        {
            m_stateChangeCallback = callback;
        }
    }
}