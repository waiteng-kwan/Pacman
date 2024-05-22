using Game.Ghost;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Ghost
{
    public class GhostAIBehaviour
    {
        //data
        public GhostAIBehaviourDataBase AiSettings { get; private set; }

        //states
        private GhostAiState m_currState;
        [NaughtyAttributes.ReadOnly, SerializeField]
        private GhostAiState m_nextState;
        private bool m_enqueueChangingState = false;
        private float m_currChangeStateTime = 0f;
        private float m_maxChangeStateTime = 0f;

        //functions
        Dictionary<GhostAiState, System.Action> m_stateToFuncDict = new();

        #region Constructor
        public GhostAIBehaviour(GhostAIBehaviourDataBase data)
        {
            AiSettings = data;
        }
        #endregion
    }
}