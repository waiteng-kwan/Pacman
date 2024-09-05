using UnityEngine;
using UnityEngine.AI;
using GhostState = Core.GhostBehaviourBase.GhostState;

namespace Core.Ghost
{
    /// <summary>
    /// This class is a child of the GhostController class which is child of Controller class
    /// (Ancestor) Controller -> (Parent) GhostController -> (Child) GhostAiController
    /// It DOES NOT contain the actual behaviour, it acts as the controller for the pawn
    /// So that in the future we can just swap out the behaviour if needed, eg player controls ghost
    /// </summary>
    public class GhostAiController : Controller
    {
        [Header("Debug View")]
        [NaughtyAttributes.ReadOnly, SerializeField]
        private GhostBehaviourBase m_ghost;
        private GhostDataBase m_settings;

        [Header("Behaviour Tree")]
        [SerializeField]
        private GhostAIBehaviourDataBase m_behaviourTree;
        private GhostAIBehaviour m_gBehaviour = null;
        private NavMeshAgent m_agent;

        //navigation
        private PacmanBehaviour m_playerTarget = null;

        protected override void OnValidate()
        {
            //this is for hot swapping behavior trees
            if(m_gBehaviour != null && Application.isPlaying)
            {
                if(m_gBehaviour.AiSettings != m_behaviourTree)
                    m_gBehaviour = new GhostAIBehaviour(m_behaviourTree, transform);
            }
        }

        protected override void Awake()
        {
            //! testing
            m_behaviourTree = Resources.Load("Data/GhostBehaviourDataBase", typeof(GhostAIBehaviourDataBase)) as GhostAIBehaviourDataBase;
            m_gBehaviour = new GhostAIBehaviour(m_behaviourTree, transform);

            Invoke("Test", 1f);
        }

        private void OnDisable()
        {
            m_gBehaviour?.Pause(true);
        }

        private void OnEnable()
        {
            m_gBehaviour?.Pause(false);
        }

        void Test()
        {
            m_playerTarget = FindFirstObjectByType<PacmanBehaviour>();
            m_gBehaviour.FollowPlayerTargetReference(m_playerTarget);
            m_gBehaviour.SwitchState(GhostAiState.Idle);
        }

        private void Update()
        {
            //in future check if is AI or not
            if(m_gBehaviour != null && UpdateAI)
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

        public void HotSwapAI(GhostAIBehaviourDataBase data)
        {
            //this is for hot swapping behavior trees
            if (m_gBehaviour != null && Application.isPlaying)
            {
                if (m_gBehaviour.AiSettings != m_behaviourTree)
                    m_gBehaviour = new GhostAIBehaviour(m_behaviourTree, transform);
            }
        }

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

            return m_gBehaviour.GetRandomSpawnPointInZone(col.bounds, modelSize, col.transform.position.y);
        }
    }
}