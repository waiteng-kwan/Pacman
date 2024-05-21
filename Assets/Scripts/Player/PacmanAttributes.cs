using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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
        public bool CanEatGhosts { get; private set; } = false;
        public PacmanStates CurrentState { get; private set; }
         = PacmanStates.Alive;

        private bool m_isInvul;

        //ghost eating
        private float m_ghostEatingDurationLeft = 0f;

        public void SetState(PacmanStates state)
        {
            CurrentState = state;
        }

        public void SetCanEatGhostState(bool canEatGhost)
        {
            CanEatGhosts = canEatGhost;
        }

        public bool IsPacmanInvul()
        {
            //only invul when dead or respawning
            return CurrentState != PacmanStates.Alive;
        }

        public void SetIsPlayerInvul(bool isInvul)
        {
            m_isInvul = isInvul;
        }
    }
}