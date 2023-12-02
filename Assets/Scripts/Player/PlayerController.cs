using Game;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Client
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        private PlayerBehaviour m_playerCharacter;

        [SerializeField, ReadOnly]
        private int m_playerIndex;
        public int Index => m_playerIndex;

        private void Awake()
        {
            if (m_playerCharacter == null)
                Debug.Log("Missing player character!");
            else
                m_playerCharacter.SetOwner(this);
        }

        // Start is called before the first frame update
        void Start()
        {
            GameMode.gameMode.RegisterPlayer(this);
        }

        // Update is called once per frame
        void Update()
        {
            //temp test code
            if(Input.GetKeyDown(KeyCode.W))
            {
                m_playerCharacter.SetMoveDir(Vector2.up);
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                m_playerCharacter.SetMoveDir(Vector2.left);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                m_playerCharacter.SetMoveDir(Vector2.down);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                m_playerCharacter.SetMoveDir(Vector2.right);
            }
            else if(Input.GetKeyDown(KeyCode.Escape))
            {
                m_playerCharacter.ResetMovement();
            }
        }
    }
}