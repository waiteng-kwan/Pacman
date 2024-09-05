using System.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;
using System;
using UnityEngine.InputSystem.XR;

namespace Core
{
    /// <summary>
    /// This script handles business logic with the unity component 
    /// PlayerInputManager, ie what to do when joining
    /// </summary>
    [RequireComponent(typeof(PlayerInputManager))]
    public class PlayerInputManagerProxy : MonoBehaviour
    {
        private PlayerInputManager m_mgrCmp;
        private PlayerManager m_pMgrRef;

        //temp
        private static int pCount = 0;
        //custom because the one in PlayerInputManager is not writable
        private int m_maxPlayerCount = 1;

        //events
        public Action<PlayerController> EOnPlayerJoinedAndSetUp;
        public Action<PlayerController> EOnPlayerRemoved;

        private void OnValidate()
        {
            m_mgrCmp = GetComponent<PlayerInputManager>();
        }

        private void Awake()
        {
            //event listeners
            m_mgrCmp.onPlayerJoined += OnPlayerJoined;
            m_mgrCmp.onPlayerLeft += OnPlayerLeft;
        }

        private void OnDestroy()
        {
            //event listeners
            m_mgrCmp.onPlayerJoined -= OnPlayerJoined;
            m_mgrCmp.onPlayerLeft -= OnPlayerLeft;

            SystemActionUtils.ClearEvent(EOnPlayerJoinedAndSetUp);
            SystemActionUtils.ClearEvent(EOnPlayerRemoved);
        }

        private void OnPlayerJoined(PlayerInput obj)
        {
            Debug.Log("Player has joined!");

            var pc = obj.GetComponent<PlayerController>();
            SetUpPlayer(pc);

            EOnPlayerJoinedAndSetUp?.Invoke(pc);

            /*if(pCount >= m_maxPlayerCount)
            {
                print("no");
                m_mgrCmp.DisableJoining();
            }*/
        }

        private void OnPlayerLeft(PlayerInput obj)
        {
            print("Player left");

            PlayerController controller = obj.GetComponent<PlayerController>();

            //enable joining again
            /*if (--pCount < m_maxPlayerCount)
            {
                print("can join again :)");
                m_mgrCmp.EnableJoining();
            }*/

            EOnPlayerRemoved?.Invoke(controller);
        }

        private void SetUpPlayer(PlayerController controller)
        {
            controller.SetIndex(pCount++);

            string guid = Guid.NewGuid().ToString();
            controller.SetPlayerHash(GameManager.Instance.IsLocalMode ? "local-" : "remote-" + guid);
            //do other stuff here
        }

        public void SetPlayerManagerReference(PlayerManager mgrRef)
        {
            m_pMgrRef = mgrRef;
        }
    }
}