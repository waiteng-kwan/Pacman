using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

        [Header("Character Prefab")]
        [SerializeField]
        private PlayerBehaviour m_charPrefab;

        //temp
        private static int pCount = 0;

        private void OnValidate()
        {
            m_mgrCmp = GetComponent<PlayerInputManager>();
        }

        private void Awake()
        {
            //if doing local testing with controllers etc
            m_mgrCmp = GetComponent<PlayerInputManager>();

            if (m_mgrCmp)
            {
                m_mgrCmp.onPlayerJoined += OnPlayerJoined;
                m_mgrCmp.onPlayerLeft += OnPlayerLeft;
            }
        }

        private void OnDestroy()
        {
            //handle local testing
            if (m_mgrCmp)
            {
                m_mgrCmp.onPlayerJoined -= OnPlayerJoined;
                m_mgrCmp.onPlayerLeft -= OnPlayerLeft;
            }
        }

        private void OnPlayerJoined(PlayerInput obj)
        {
            Debug.Log("Player has joined!");

            var pc = obj.GetComponent<PlayerController>();

            SetUpPlayer(pc);
        }

        private void OnPlayerLeft(PlayerInput obj)
        {
            print("Player left");
        }

        private void SetUpPlayer(PlayerController controller)
        {
            controller.SetIndex(pCount++);
            //PlayerBehaviour character = SpawnAndSetCharacter(ref controller);

            //do other stuff here
        }

        private PlayerBehaviour SpawnAndSetCharacter(ref PlayerController pc)
        {
            PlayerBehaviour ch = Instantiate(m_charPrefab, Vector3.right, Quaternion.identity);

            pc.PossessCharacter(ch);
            return ch;
        }
    }
}