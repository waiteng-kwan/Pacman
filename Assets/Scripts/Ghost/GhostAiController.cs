using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using GhostState = GhostBehaviourBase.GhostState;

namespace Game.Ghost
{
    public class GhostAiController : CharacterController, IGhostAi
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
        private GhostBehaviourDataBase m_behaviourTree;
        private NavMeshAgent m_agent;

        //functions
        Dictionary<GhostAiState, System.Action> m_stateToFuncDict = new Dictionary<GhostAiState, System.Action>();

        //navigation
        private Vector3 m_destination = Vector3.zero;
        private PlayerBehaviour m_playerTarget = null;
        private GhostAiNavigationData m_navData = new GhostAiNavigationData();

        private void Awake()
        {
            //set up function dictionary
            m_stateToFuncDict.Add(GhostAiState.Idle, Idle);
            m_stateToFuncDict.Add(GhostAiState.Patrol, Patrol);
            m_stateToFuncDict.Add(GhostAiState.Chasing, ChasePlayer);
            m_stateToFuncDict.Add(GhostAiState.Returning, Returning);
            m_stateToFuncDict.Add(GhostAiState.RunAway, RunAway);
            m_stateToFuncDict.Add(GhostAiState.StandBy, null);

            //set up navmesh agent
            m_agent = gameObject.AddComponent<NavMeshAgent>();
            m_agent.agentTypeID = GameObject.FindGameObjectWithTag("Floor").GetComponent<NavMeshSurface>().agentTypeID;
            m_agent.baseOffset = 0.5f;
            m_agent.stoppingDistance = 0.5f;
            m_agent.angularSpeed = 270f;

            Invoke("Test", 1f);
        }

        private void OnDisable()
        {
            if (m_agent != null)
                m_agent.enabled = false;

            m_nextState = GhostAiState.Idle;
            m_playerTarget = null;
        }

        private void OnEnable()
        {
            if (m_agent != null)
                m_agent.enabled = true;

            m_nextState = GhostAiState.Idle;
            m_playerTarget = null;
        }

        void Test()
        {
            PrepChangeState(GhostAiState.Idle);
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
            if (m_stateToFuncDict[m_currState] != null)
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
                    m_agent.enabled = true;
                    m_agent.autoBraking = true;

                    //calculate path?
                    m_navData.ClearIdle();
                    var col = m_ghost.GhostRespawnZone;

                    if (!col)
                    {
                        Debug.Log("No collider for ghost respawn zone found!");
                        return;
                    }

                    //enable so extents can be calculated
                    col.enabled = true;

                    //doing in pairs so it will always be 1 point on the min, 1 on the max
                    //fake the idle movement
                    for (int i = 0; i < 2; i++)
                    {
                        //get random point on extent
                        float randX = Random.Range(col.transform.position.x, col.bounds.min.x);
                        float randZ = Random.Range(col.transform.position.z, col.bounds.min.z);

                        m_navData.AddPointToIdlePath(new Vector3(randX, col.transform.position.y, randZ));

                        //get random point on extent
                        randX = Random.Range(col.transform.position.x, col.bounds.max.x);
                        randZ = Random.Range(col.transform.position.z, col.bounds.max.z);

                        m_navData.AddPointToIdlePath(new Vector3(randX, col.transform.position.y, randZ));
                    }

                    m_destination = m_navData.CurrentPointOnIdlePath();

                    if (!m_ghost.IsDead)
                        SetDestination(m_destination);
                    break;
                case GhostAiState.Patrol:
                    m_agent.autoBraking = false;

                    break;
                case GhostAiState.Chasing:
                    m_agent.autoBraking = false;

                    //next time get closest player or something

                    //temp!!
                    m_playerTarget = FindObjectOfType<PlayerBehaviour>();
                    break;
                case GhostAiState.Returning:
                    m_agent.autoBraking = false;

                    break;
                case GhostAiState.RunAway:
                    m_agent.autoBraking = false;

                    break;

                case GhostAiState.StandBy:
                    print("standy");
                    //have to turn this off if not it resets position on its own
                    m_agent.enabled = false;

                    m_agent.autoBraking = true;

                    //clear path data
                    m_agent.velocity = Vector3.zero;
                    break;
                default:
                    break;
            }
        }

        public void SwitchState()
        {
            m_currState = m_nextState;
        }

        public void PossessPawn(GhostBehaviourBase pawn)
        {
            m_ghost = pawn;
            m_settings = pawn.Data;

            pawn.EOnStateChange.AddListener(OnPawnStateChange);
        }

        public void UnposessPawn(GhostBehaviourBase pawn)
        {
            //clean up
            SetNextState(GhostAiState.StandBy);

            m_ghost = null;
            m_settings = null;

            pawn.EOnStateChange.RemoveListener(OnPawnStateChange);
        }

        public void SetDestination(Vector3 destination)
        {
            m_agent.SetDestination(destination);
        }
        #endregion

        /// <summary>
        /// This is the function that will be called before the ghost exits the respawn zone
        /// </summary>
        void Idle()
        {
            var distance = (transform.position - m_destination).sqrMagnitude;

            if (distance <= 1f)
            {
                if (m_currChangeStateTime > 0)
                {
                    m_currChangeStateTime -= Time.deltaTime;
                    return;
                }
                m_destination = m_navData.NextPointOnIdlePath();
                SetDestination(m_destination);

                m_maxChangeStateTime = GetRandomIdleWaitingTime();
                m_currChangeStateTime = m_maxChangeStateTime;
            }
        }

        /// <summary>
        /// This is the state where the player has not been detected and ghost isn't chasing it.
        /// By default this is the state that will happen most frequently outside of idle.
        /// Can get out of this state when detecting player
        /// </summary>
        void Patrol()
        {
            print("pattrol");
        }

        /// <summary>
        /// This fn is called when state is chasing player.
        /// Stop condition: when player is no longer alive
        /// @todo: 
        /// - set different ways of chasing player, coordination etc
        /// - get new target (multiplayer)
        /// </summary>
        void ChasePlayer()
        {
            print("chgase");

            //set object of interest to player
            m_destination = m_playerTarget.transform.position;
            SetDestination(m_destination);

            if (m_playerTarget.Attributes.CurrentState != PlayerCharacterStates.Alive)
            {
                //stop movement
                m_agent.isStopped = true;

                //add recalculate here next time

                SetNextState(GhostAiState.Returning);
            }
        }

        /// <summary>
        /// This fn is called when player has finished chasing.
        /// </summary>
        void Returning()
        {
            print("return");

            //once finish returning to og point, then set to patrol
            SetNextState(GhostAiState.Patrol);
        }

        /// <summary>
        /// This fn is the default state when player is able to eat ghosts.
        /// </summary>
        void RunAway()
        {
            print("Run away!!!");
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
                    m_enqueueChangingState = true;
                    
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

        float GetRandomStateChangeDampingPoint()
        {
            return Random.Range(m_settings.ChangeStateDampingRange.x, m_settings.ChangeStateDampingRange.y);
        }

        float GetRandomIdleWaitingTime()
        {
            return Random.Range(m_settings.ChangeIdleWaitngRange.x, m_settings.ChangeIdleWaitngRange.y);
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