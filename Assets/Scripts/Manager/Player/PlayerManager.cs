using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    public class PlayerManager : /*MonoBehaviour,*/ IManager
    {
        private PlayerInputManagerProxy m_inputMgr;
        private Dictionary<string, IPlayerState> PlayerHashToState = new();

        //local only
        private PlayerController m_localDefaultPc = null;
        private PlayerController m_pcInControl = null;   //in case the owner died

        public static PlayerManager CreateInstance()
        {
            return new PlayerManager();
        }

        public Type GetManagerType()
        {
            return typeof(PlayerManager);
        }

        public void OnStateChange(GameInstanceStates prevState, GameInstanceStates currState, GameInstanceStates nextState)
        {
        }

        public void PostStateChange(GameInstanceStates nextState)
        {
        }

        public void PreStateChange(GameInstanceStates currentState)
        {
        }

        public void RegisterManager(GameManager mgr)
        {
            Transform input = GameManager.Instance.transform.Find("MultiplayerInput");

            if(input)
            {
                SetInputManager(input.GetComponent<PlayerInputManagerProxy>());

                m_inputMgr.EOnPlayerJoinedAndSetUp += AddPlayer;
            }
        }

        public IManager RetrieveInterface()
        {
            return this as IManager;
        }

        public void ShutdownManager(GameManager mgr)
        {
            PlayerHashToState.Clear();
        }

        private void SetInputManager(PlayerInputManagerProxy inputMgr)
        {
            m_inputMgr = inputMgr;
        }

        private void AddPlayer(PlayerController pc)
        {
            Debug.Log("add to dictionary " + pc.PlayerHash);

            if (!m_localDefaultPc)
            {
                m_localDefaultPc = pc;
                m_pcInControl = pc;
            }

            if(!PlayerHashToState.ContainsKey(pc.PlayerHash))
                PlayerHashToState.Add(pc.PlayerHash, null);
        }

        private void RemovePlayer(PlayerController pc)
        {
            Debug.Log("Remove dictionary");

            if (m_localDefaultPc == pc)
            {
                Debug.Log("Owner of game left, find next player");

                if (PlayerHashToState.ContainsKey(pc.PlayerHash))
                    PlayerHashToState.Remove(pc.PlayerHash);

                if(PlayerHashToState.Count > 0)
                    m_pcInControl = PlayerHashToState.Values.First().Owner;

                m_localDefaultPc = null;
            }
        }
    }
}