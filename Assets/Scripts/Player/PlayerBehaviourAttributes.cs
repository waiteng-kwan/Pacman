using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public enum PlayerCharacterStates
    {
        Alive,
        Dead,
        Respawning,
        Invul
    }

    public class PlayerBehaviourAttributes : MonoBehaviour
    {
        private bool m_canEatGhosts = false;
        public bool CanEatGhosts => m_canEatGhosts;

        private PlayerCharacterStates m_currState;
        public PlayerCharacterStates CurrentState => m_currState;

        public void SetState(PlayerCharacterStates state)
        {
            m_currState = state;
        }

        public void SetCanEatGhostState(bool canEatGhost)
        {
            m_canEatGhosts = canEatGhost;

            Invoke("SetEatGhostStateInactive", 5f);
        }

        public void SetEatGhostStateInactive()
        {
            m_canEatGhosts = false;
        }
    }
}