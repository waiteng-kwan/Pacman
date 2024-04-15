using Game.Ghost;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Idea is that the GhostAiController will read this class.
/// So one way access only
/// </summary>
[CreateAssetMenu(fileName = "GhostBehaviourDataBase",
menuName = "Scriptable Objects/Ghost/GhostBehaviour Data", order = 1)]
public class GhostBehaviourDataBase : ScriptableObject
{
    [Header("Target Type")]
    [SerializeField, InfoBox("This influences the type of movement")]
    private GhostTargetType TargetType;

    [Header("Movement Weight")]
    [Tooltip("In traditional pacman, ghosts cannot move backwards except in special circumstances")]
    public bool BlockBackwardMovementNormally = true;
    [Tooltip("In traditional pacman, ghosts can turn around to chase")]
    public bool AllowChaseBackwardMovement = true;
    [Tooltip("In traditional pacman, they take the tile with closes linear distance to target")]
    public bool UseClosestLinearDistance = true;
    [InfoBox("The tendency to move in Up, Down, Left, Right. Closest Distance overrides this!")]
    [SerializeField, Range(0f, 1f), HideIf("UseClosestLinearDistance")]
    private float m_upWeight;
    [SerializeField, Range(0f, 1f), HideIf("UseClosestLinearDistance")] 
    private float m_downWeight;
    [SerializeField, Range(0f, 1f), HideIf("UseClosestLinearDistance")] 
    private float m_leftWeight;
    [SerializeField, Range(0f, 1f), HideIf("UseClosestLinearDistance")] 
    private float m_rightWeight;
    private MovementWeight m_movementWeight;

    [Header("State")]
    [field: SerializeField, ReadOnly]
    public GhostAiState PreviousAiState { get; private set; }
    [field: SerializeField, ReadOnly]
    public GhostAiState CurrentAiState { get; private set; }
    [field: SerializeField, ReadOnly]
    public GhostAiState NextAiState { get; private set; }
    private bool m_enqueueChangingState = false;
    private float m_currChangeStateTime = 0f;
    private float m_maxChangeStateTime = 0f;

    [Header("Cycle")]
    [InfoBox("This time is set by the game mode")]
    [ReadOnly, SerializeField]
    private float m_cycleTime;                  //the time to wait
    private float m_internalCycleTimeCounter;

    //functions
    Dictionary<GhostAiState, System.Action> m_stateToFuncDict;

    [Header("Destination")]
    [field: SerializeField, ReadOnly]
    public Vector3 Destination { get; private set; } = Vector3.zero;

    public virtual void Initialize()
    {
        m_movementWeight = new(m_upWeight, m_downWeight,
                             m_leftWeight, m_rightWeight);

        m_stateToFuncDict = new()
        {
            //set up function dictionary
            { GhostAiState.Idle, Idle },
            { GhostAiState.Patrol, Patrol },
            { GhostAiState.Chasing, ChasePlayer },
            { GhostAiState.Returning, Returning },
            { GhostAiState.RunAway, RunAway },
            { GhostAiState.StandBy, null }
        };
    }

    public virtual void CleanUp()
    {
        m_stateToFuncDict?.Clear();
    }

    /// <summary>
    /// Engine power go vroom vroom
    /// </summary>
    public void StartAI()
    {

    }

    #region States
    protected virtual void Idle()
    {

    }

    protected virtual void Patrol()
    {

    }

    protected virtual void ChasePlayer()
    {

    }

    /// <summary>
    /// Returning to their ghost zone
    /// </summary>
    protected virtual void Returning()
    {

    }

    protected virtual void RunAway()
    {

    }

    protected virtual void StandBy()
    {

    }

    public void StartSwitchState()
    {

    }

    protected virtual void PrepareToSwitchState()
    {

    }

    protected virtual void SwitchState()
    {

    }

    protected virtual void PostSwitchState()
    {

    }
    #endregion

    #region Destination
    public void SetDestination(Vector3 destination)
    {
        Destination = destination;
    }
    #endregion

    #region Movement
    public Vector2 GetMovementDir(Vector3 currentPos)
    {
        //calc tile with closest linear distance
        if (UseClosestLinearDistance)
        {
            //get up, down, left, right tile coordinate

            //check if any of them are solid/illegal

            //ignore illegal

            //calculate leftover linear distance to target

            //take closest
        }
        //otherwise do some logic with teh weights
        else
        {

        }

        return Vector2.zero;
    }
    #endregion
}