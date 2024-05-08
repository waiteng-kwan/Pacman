using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public enum PacmanStates
    {
        Alive,
        Dead,
        Respawning
    }

    public class PacmanAttributes : MonoBehaviour
    {
        private bool m_canEatGhosts = false;
        public bool CanEatGhosts => m_canEatGhosts;

        private PacmanStates m_currState;
        public PacmanStates CurrentState => m_currState;
        private bool m_isInvul;

        public void SetState(PacmanStates state)
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
            return m_currState != PacmanStates.Alive;
        }

        public void SetIsPlayerInvul(bool isInvul)
        {
            m_isInvul = isInvul;
        }
    }
}