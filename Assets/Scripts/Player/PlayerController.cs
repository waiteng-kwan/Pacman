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
        public PlayerBehaviour PlayerCharacter => m_playerCharacter;

        [SerializeField, ReadOnly]
        private int m_playerIndex;
        public int Index => m_playerIndex;

        private void Awake()
        {
            if(GameModeBase.Instance)
                GameModeBase.Instance.RegisterPlayer(this);
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

        public void PossessCharacter(PlayerBehaviour character)
        {
            character.SetOwner(this);

            m_playerCharacter = character;
        }

        public void SetIndex(int index)
        {
            m_playerIndex = index;
        }
    }
}