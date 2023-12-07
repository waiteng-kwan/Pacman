using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public enum GhostAiState
    {
        Patrol,      //waiting to leave zone
        Chasing,     //wobbling around,
        Returning    //chasing playerng
    }
    
    public class GhostAiController : MonoBehaviour, IGhostAi
    {
        [Header("Debug View")]
        [NaughtyAttributes.ReadOnly, SerializeField]
        private GhostBehaviourBase m_ghost;
        private GhostDataBase m_settings;

        [Header("State")]
        [NaughtyAttributes.ReadOnly, SerializeField]
        private GhostAiState m_currState;
        [NaughtyAttributes.ReadOnly, SerializeField]
        private GhostAiState m_nextState;

        [Header("Behaviour Tree")]
        [SerializeField]
        private int m_behaviourTree;

        public void GetCurrentState()
        {
            throw new System.NotImplementedException();
        }

        public void SetNextState()
        {
            throw new System.NotImplementedException();
        }

        public void SetPawn()
        {
            throw new System.NotImplementedException();
        }
    }
}