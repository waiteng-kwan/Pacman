using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.Rendering.DebugUI;

namespace Game
{
    public enum GhostAiState
    {
        Idle,
        Patrol,      //waiting to leave zone
        Chasing,     //wobbling around,
        Returning,   //chasing player
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
        private bool m_enqueueChangingState = false;
        private float m_currChangeStateTime = 0f;
        private float m_maxChangeStateTime = 0f;

        [Header("Behaviour Tree")]
        [SerializeField]
        private int m_behaviourTree;
        private NavMeshAgent m_agent;

        //functions
        Dictionary<GhostAiState, System.Action> m_stateToFuncDict = new Dictionary<GhostAiState, System.Action>();

        //navigation
        private Vector3 m_destination = Vector3.zero;
        private PlayerBehaviour m_playerTarget = null;

        private void Awake()
        {
            //set up function dictionary
            m_stateToFuncDict.Add(GhostAiState.Idle, Idle);
            m_stateToFuncDict.Add(GhostAiState.Patrol, Patrol);
            m_stateToFuncDict.Add(GhostAiState.Chasing, ChasePlayer);
            m_stateToFuncDict.Add(GhostAiState.Returning, Returning);

            //set up navmesh agent
            m_agent = gameObject.AddComponent<NavMeshAgent>();
            m_agent.agentTypeID = GameObject.FindGameObjectWithTag("Floor").GetComponent<NavMeshSurface>().agentTypeID;
            m_agent.baseOffset = 0.5f;

            Invoke("Test", 3f);
        }

        void Test()
        {
            SetNextState(GhostAiState.Chasing);
        }

        private void Update()
        {
            if (m_agent == null)
                return;

            if (m_currState != m_nextState)
            {
                if (!m_enqueueChangingState)
                {
                    m_enqueueChangingState = true;

                    m_maxChangeStateTime = GetRandomStateChangeDampingPoint();
                    m_currChangeStateTime = m_maxChangeStateTime;
                }

                //countdown to change state time
                m_currChangeStateTime -= Time.deltaTime;

                //finally change state
                if (m_currChangeStateTime <= 0)
                {
                    PrepChangeState(m_nextState);
                    SwitchState();

                    //reset flag
                    m_enqueueChangingState = false;
                }
            }

            //exec
            m_stateToFuncDict[m_currState]();
        }

        #region IGhostAi
        public GhostAiState GetCurrentState()
        {
            return m_currState;
        }

        public void SetNextState(GhostAiState value)
        {
            m_nextState = value;
        }

        public void PrepChangeState(GhostAiState nextState)
        {
            switch (nextState)
            {
                case GhostAiState.Idle:
                    m_agent.autoBraking = true;

                    break;
                case GhostAiState.Patrol:
                    m_agent.autoBraking = false;

                    break;
                case GhostAiState.Chasing:
                    m_agent.autoBraking = false;

                    //next time get closest player or something

                    m_playerTarget = FindObjectOfType<PlayerBehaviour>();
                    break;
                case GhostAiState.Returning:
                    m_agent.autoBraking = false;

                    break;
                default:
                    break;
            }
        }

        public void SwitchState()
        {
            m_currState = m_nextState;
        }

        public void SetPawn(GhostBehaviourBase pawn)
        {
            m_ghost = pawn;
            m_settings = pawn.Data;
        }

        public void SetDestination(Vector3 destination)
        {
            m_agent.SetDestination(destination);
        }
        #endregion

        void Idle()
        {
            print("Idkle");
        }

        void Patrol()
        {
            print("pattrol");
        }

        void ChasePlayer()
        {
            print("chgase");

            //set object of interest to player
            m_agent.SetDestination(m_playerTarget.transform.position);

            if(m_playerTarget.Attributes.CurrentState != PlayerCharacterStates.Alive)
            {
                SetNextState(GhostAiState.Idle);
            }
        }

        void Returning()
        {
            print("return");

        }

        float GetRandomStateChangeDampingPoint()
        {
            return Random.Range(m_settings.ChangeStateDampingRange.x, m_settings.ChangeStateDampingRange.y);
        }
    }
}