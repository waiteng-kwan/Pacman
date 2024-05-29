using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using GhostState = Game.GhostBehaviourBase.GhostState;

namespace Game.Ghost
{
    /// <summary>
    /// This class is a subset of the Controller class
    /// It DOES NOT contain the actual behaviour, it acts as the controller for the pawn
    /// So that in the future we can just swap out the behaviour if needed, eg player controls ghost
    /// </summary>
    public class GhostAiController : Controller
    {
        [Header("Debug View")]
        [NaughtyAttributes.ReadOnly, SerializeField]
        private GhostBehaviourBase m_ghost;
        private GhostDataBase m_settings;

        [Header("State")]
        [NaughtyAttributes.ReadOnly, SerializeField]
        private GhostAiState m_currState;

        [Header("Behaviour Tree")]
        [SerializeField]
        private GhostAIBehaviourDataBase m_behaviourTree;
        private GhostAIBehaviour m_gBehaviour = null;
        private NavMeshAgent m_agent;

        //navigation
        private PacmanBehaviour m_playerTarget = null;
        private GhostAiNavigationData m_navData = new GhostAiNavigationData();

        protected override void OnValidate()
        {
            //this is for hot swapping behavior trees
            if(m_gBehaviour != null && Application.isPlaying)
            {
                if(m_gBehaviour.AiSettings != m_behaviourTree)
                    m_gBehaviour = new GhostAIBehaviour(m_behaviourTree, transform);
                print("aaaAAAAaaAAa");
            }
        }

        protected override void Awake()
        {
            //! testing
            m_behaviourTree = Resources.Load("Data/GhostBehaviourDataBase", typeof(GhostAIBehaviourDataBase)) as GhostAIBehaviourDataBase;
            m_gBehaviour = new GhostAIBehaviour(m_behaviourTree, transform);

            //set up navmesh agent
            m_agent = gameObject.AddComponent<NavMeshAgent>();
            m_agent.agentTypeID = GameObject.FindGameObjectWithTag("Floor").GetComponent<NavMeshSurface>().agentTypeID;
            m_agent.baseOffset = 0.5f;
            m_agent.stoppingDistance = 0.5f;
            m_agent.angularSpeed = 270f;

            m_gBehaviour.Agent = m_agent;

            Invoke("Test", 1f);
        }

        private void OnDisable()
        {
            if (m_agent != null)
                m_agent.enabled = false;

            m_playerTarget = null;
        }

        private void OnEnable()
        {
            if (m_agent != null)
                m_agent.enabled = true;

            m_playerTarget = null;
        }

        void Test()
        {
            m_gBehaviour.SwitchState(GhostAiState.Idle);

            m_playerTarget = FindFirstObjectByType<PacmanBehaviour>();
            m_gBehaviour.FollowPlayerTarget(m_playerTarget);
        }

        private void Update()
        {
            if (m_agent == null)
                return;

            //exec
            if(m_gBehaviour != null)
                m_gBehaviour.Update();
        }

        #region IGhostAi
        public void SetNextState(GhostAiState value)
        {
            m_gBehaviour.SetNextState(value);
        }

        public override void PossessPawn(PawnBase pawn)
        {
            base.PossessPawn(pawn);

            m_ghost = pawn as GhostBehaviourBase;
            m_settings = pawn.Settings as GhostDataBase;

            m_ghost.EOnStateChange.AddListener(OnPawnStateChange);
        }

        public override void UnposessPawn()
        {
            //clean up
            SetNextState(GhostAiState.StandBy);

            m_ghost.EOnStateChange.RemoveListener(OnPawnStateChange);

            m_ghost = null;
            m_settings = null;

            base.UnposessPawn();
        }
        #endregion

        void OnPawnStateChange(GhostState oldState, GhostState newState)
        {
            switch (newState)
            {
                case GhostState.InitialSpawn:
                    break;
                case GhostState.Standby:
                    SetNextState(GhostAiState.Idle);
                    break;
                case GhostState.Active:
                    break;
                case GhostState.Dying:
                    //need to overwrite the queue, we want immediate reaction
                    //m_enqueueChangingState = true;

                    SetNextState(GhostAiState.StandBy);
                    break;
                case GhostState.Dead:
                    break;
                case GhostState.Respawning:
                    //teleport the ghost
                    m_ghost.transform.position = GetRandomSpawnPointInZone();
                    break;
                default:
                    break;
            }
        }

        Vector3 GetRandomSpawnPointInZone()
        {
            var col = m_ghost.GhostRespawnZone;

            if (!col)
            {
                Debug.Log("No collider for ghost respawn zone found!");
                return Vector3.zero;
            }

            //enable so extents can be calculated
            col.enabled = true;

            Vector3 modelSize = m_ghost.GetComponent<Collider>().bounds.size;

            //get random point on extent
            float randX = Random.Range(col.bounds.min.x + modelSize.x, col.bounds.max.x - modelSize.x);
            float randZ = Random.Range(col.bounds.min.z + modelSize.z, col.bounds.max.z - modelSize.z);

            return new Vector3(randX, col.transform.position.y, randZ);
        }
    }
}