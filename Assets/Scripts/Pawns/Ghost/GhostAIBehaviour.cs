using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Core.Ghost
{
    public class GhostAIBehaviour
    {
        //data
        public GhostAIBehaviourDataBase AiSettings { get; private set; }

        //states
        public GhostAiState PreviousState { get; private set; }
        public GhostAiState CurrentState { get; private set; }
        public GhostAiState NextState { get; private set; }
        public UnityEvent EOnAiStateChange { get; private set; } = new();

        private bool m_enqueueChangingState = false;
        private float m_currChangeStateTime = 0f;
        private float m_maxChangeStateTime = 0f;

        //functions
        Dictionary<GhostAiState, System.Action> m_stateToFuncDict;
        List<System.Action> m_updateQueue = new();

        //target
        private PacmanBehaviour m_playerTarget = null;
        private Transform m_targetTransform = null;
        private Vector3 m_destination;
        private Transform m_myTransform;

        //navigation crap
        private GhostAiNavigationData m_navData = new();
        private const float m_distanceCheckThreshold = 1f;

        //!temp !! 
        public NavMeshAgent Agent;

        #region Constructor/Destructor
        public GhostAIBehaviour(GhostAIBehaviourDataBase data, Transform myTransform)
        {
            m_myTransform = myTransform;

            AiSettings = data;

            m_stateToFuncDict = new()
            {
                //set up function dictionary
                { GhostAiState.Idle, Idle },
                { GhostAiState.Patrol, Patrol },
                { GhostAiState.Chasing, ChasePlayer },
                { GhostAiState.Returning, Returning },
                { GhostAiState.RunAway, RunAway },
                { GhostAiState.StandBy, StandBy }
            };

            m_updateQueue.Add(data.ChangeStateDampingRange != Vector2.zero ? UpdateRandomChangeState : UpdateNormalChangeState);

            m_updateQueue.Add(UpdateState);

            //set up navmesh agent
            Agent = myTransform.AddComponent<NavMeshAgent>();

            //this agent id is actualy an addressable asset, can get from game mode set up file maybe?
            Agent.agentTypeID = GameObject.FindGameObjectWithTag("Floor").GetComponent<NavMeshSurface>().agentTypeID;

            //temporary settings shit
            Agent.baseOffset = 0.5f;
            Agent.stoppingDistance = 0.5f;
            Agent.angularSpeed = 270f;
        }

        ~GhostAIBehaviour()
        {
            EOnAiStateChange.RemoveAllListeners();
            m_updateQueue.Clear();
            m_stateToFuncDict.Clear();
        }
        #endregion

        #region AI State Function Library
        public void Pause(bool isPauseAI = false)
        {
            if (isPauseAI)
                NextState = GhostAiState.StandBy;
            else
                NextState = PreviousState;
        }
        /// <summary>
        /// Spawned but game not ready
        /// </summary>
        protected virtual void StandBy()
        {
            Debug.Log("STAND BY MEEEEEEE");
        }

        /// <summary>
        /// This is the function that will be called before the ghost exits the respawn zone
        /// </summary>
        protected virtual void Idle()
        {
            Debug.Log("idle");

            var distance = (m_myTransform.position - m_destination).sqrMagnitude;

            if (distance <= m_distanceCheckThreshold)
            {
                m_destination = m_navData.NextPointOnIdlePath();
                SetTargetPosition(m_destination);

                SwitchState(GhostAiState.Chasing);
            }
        }

        /// <summary>
        /// This is the state where the player has not been detected and ghost isn't chasing it.
        /// By default this is the state that will happen most frequently outside of idle.
        /// Can get out of this state when detecting player
        /// </summary>
        protected virtual void Patrol()
        {
            Debug.Log("wee woo wee woo snow patrol");
        }

        float m_currCycleCounter = 0f, m_maxCycleCounter = 5f;
        /// <summary>
        /// This fn is called when state is chasing player.
        /// Stop condition: when player is no longer alive
        /// @todo: 
        /// - set different ways of chasing player, coordination etc
        /// - get new target (multiplayer)
        /// </summary>
        protected virtual void ChasePlayer()
        {
            //set object of interest to player
            m_destination = m_playerTarget.transform.position;
            SetTargetPosition(m_destination);
            Agent.SetDestination(m_destination);

            //the cycle is how traditional pacman is implemented where
            //there are cycles of patrol -> chase -> patrol -> chase
            /*if (m_currCycleCounter >= m_maxCycleCounter)
            {
                m_currCycleCounter = 0f;
                //stop movement
                Agent.isStopped = true;

                //add recalculate here next time

                NextState = (GhostAiState.Patrol);
            }

            if (m_playerTarget.Attributes.CurrentState != PacmanStates.Alive)
            {
                //stop movement
                Agent.isStopped = true;

                //add recalculate here next time

                NextState = (GhostAiState.Returning);
            }

            m_currCycleCounter += Time.deltaTime;*/
        }

        /// <summary>
        /// This fn is called when player has finished chasing.
        /// </summary>
        protected virtual void Returning()
        {
            //once finish returning to og point, then set to patrol
            NextState = (GhostAiState.Patrol);
        }

        /// <summary>
        /// This fn is the default state when player is able to eat ghosts.
        /// Aka scared, or wahtever you want to call it
        /// </summary>
        protected virtual void RunAway()
        {
            Debug.Log("run away!!!");
        }
        #endregion

        #region Update
        public void Update()
        {
            //exec
            for (int i = 0; i < m_updateQueue.Count; i++)
                m_updateQueue[i]();
        }
        private void UpdateNormalChangeState()
        {
            if (CurrentState != NextState)
            {
                SwitchState(NextState);
            }
        }
        private void UpdateRandomChangeState()
        {
            if (CurrentState != NextState)
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
                    SwitchState(NextState);

                    //reset flag
                    m_enqueueChangingState = false;
                }
            }
        }
        private void UpdateState()
        {
            //exec
            if (m_stateToFuncDict[CurrentState] != null)
                m_stateToFuncDict[CurrentState]();
        }
        #endregion

        #region State Change
        /// <summary>
        /// Outfacing
        /// </summary>
        /// <param name="state">New state to set</param>
        public void SetNextState(GhostAiState nextState)
        {
            NextState = nextState;
        }

        private void PreChangeState(GhostAiState nextState)
        {
            switch (nextState)
            {
                case GhostAiState.StandBy:
                    //have to turn this off if not it resets position on its own
                    Agent.enabled = false;

                    Agent.autoBraking = true;

                    //clear path data
                    Agent.velocity = Vector3.zero;
                    break;
                case GhostAiState.Idle:
                    Agent.enabled = true;
                    Agent.autoBraking = true;

                    //calculate path?
                    /*m_navData.ClearIdle();
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
                        SetDestination(m_destination);*/
                    break;
                case GhostAiState.Patrol:
                    Agent.autoBraking = false;

                    break;
                case GhostAiState.Chasing:
                    Agent.autoBraking = false;

                    break;
                case GhostAiState.Returning:
                    Agent.autoBraking = false;

                    break;
                case GhostAiState.RunAway:
                    Agent.autoBraking = false;

                    break;
                default:
                    break;
            }
        }

        public void SwitchState(GhostAiState nextState)
        {
            PreChangeState(nextState);

            NextState = nextState;
            PreviousState = CurrentState;
            CurrentState = NextState;

            PostChangeState();
        }

        private void PostChangeState()
        {

        }

        float GetRandomStateChangeDampingPoint()
        {
            return Random.Range(AiSettings.ChangeStateDampingRange.x, AiSettings.ChangeStateDampingRange.y);
        }
        #endregion

        #region Targeting
        public void SetTargetPosition(Vector3 pos)
        {

        }

        public void SetTargetReference(Transform pos)
        {
            m_targetTransform = pos;
        }

        public void FollowPlayerTargetReference(PacmanBehaviour pos)
        {
            m_playerTarget = pos;
            m_targetTransform = pos.transform;
        }
        #endregion

        #region Move
        public Vector3 GetRandomSpawnPointInZone(Bounds bounds, Vector3 allowance, float vertPos)
        {
            //get random point on extent
            float randX = Random.Range(bounds.min.x + allowance.x, bounds.max.x - allowance.x);
            float randZ = Random.Range(bounds.min.z + allowance.z, bounds.max.z - allowance.z);

            return new Vector3(randX, vertPos, randZ);
        }
        #endregion
    }
}