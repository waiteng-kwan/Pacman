using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public enum PlayerCharacterStates
    {
        Alive,
        Dead,
        Respawning
    }

    public class PlayerBehaviourAttributes : MonoBehaviour
    {
        private bool m_canEatGhosts = false;
        public bool CanEatGhosts => m_canEatGhosts;

        private PlayerCharacterStates m_currState;
        public PlayerCharacterStates CurrentState => m_currState;
        private bool m_isInvul;

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

        public bool IsPlayerInvul()
        {
            //only invul when dead or respawning
            return m_currState != PlayerCharacterStates.Alive;
        }

        public void SetIsPlayerInvul(bool isInvul)
        {
            m_isInvul = isInvul;
        }
    }
}