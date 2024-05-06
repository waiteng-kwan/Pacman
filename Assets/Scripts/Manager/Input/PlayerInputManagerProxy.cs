using System.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;
using System;
using UnityEngine.InputSystem.XR;

namespace Game
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

        [Header("Character Prefab")]
        [SerializeField]
        private PlayerBehaviour m_charPrefab;

        //temp
        private static int pCount = 0;

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
        }

        private void OnPlayerLeft(PlayerInput obj)
        {
            print("Player left");

            PlayerController controller = obj.GetComponent<PlayerController>();

            EOnPlayerRemoved?.Invoke(controller);
        }

        private void SetUpPlayer(PlayerController controller)
        {
            controller.SetIndex(pCount++);

            string guid = Guid.NewGuid().ToString();
            controller.SetPlayerHash(GameManager.Instance.IsLocalMode ? "local-" : "remote-" + guid);

            //PlayerBehaviour character = SpawnAndSetCharacter(ref controller);

            //do other stuff here
        }

        private PlayerBehaviour SpawnAndSetCharacter(ref PlayerController pc)
        {
            PlayerBehaviour ch = Instantiate(m_charPrefab, Vector3.right, Quaternion.identity);

            pc.PossessCharacter(ch);
            return ch;
        }

        public void SetPlayerManagerReference(PlayerManager mgrRef)
        {
            m_pMgrRef = mgrRef;
        }
    }
}