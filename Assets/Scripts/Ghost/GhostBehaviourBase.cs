using Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GhostBehaviourBase : PawnBase
{
    public enum GhostState
    {
        InitialSpawn,
        Standby,       //waiting to leave zone
        Active,        //on field
        Dying,         //extra state for animation
        Dead,          //finally dead
        Respawning     //loops back to standby
    }
    public bool IsDead { get; private set; } = false;

    [Header("Data")]
    [SerializeField, NaughtyAttributes.Expandable]
    private GhostDataBase m_data;
    public GhostDataBase Data => m_data;

    [Header("(Read Only) for Debug")]
    [NaughtyAttributes.ReadOnly, SerializeField]
    private GameObject m_model;
    [NaughtyAttributes.ReadOnly, SerializeField]
    private Rigidbody m_rb;
    [NaughtyAttributes.ReadOnly, SerializeField]
    private Collider m_col;
    public Collider GhostRespawnZone { get; private set; } = null;

    //states
    [NaughtyAttributes.ReadOnly, SerializeField]
    private GhostState m_currGState;

    //events
    //current state, new state
    public UnityEvent<GhostState, GhostState> EOnStateChange { get; private set; } = 
        new UnityEvent<GhostState, GhostState>();

    private void OnValidate()
    {
        m_rb = GetComponent<Rigidbody>();
        m_col = GetComponent<Collider>();

        if (m_data == null)
            Debug.LogError("Ghost data is missing on " + gameObject.name);
    }

    private void Awake()
    {
        m_rb = GetComponent<Rigidbody>();
        m_col = GetComponent<Collider>();

        if (m_data != null)
        {
            ChangeModel();
        }
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(transform.position, transform.forward * 15f, Color.magenta);
        Debug.DrawRay(transform.position, Vector3.left * 5f, Color.cyan);
    }

    private void OnDestroy()
    {
        EOnStateChange.RemoveAllListeners();
    }

    public void ChangeModel()
    {
        if (m_model)
        {
            //destroy current model and spawn new one
            Destroy(m_model);
        }

        m_model = Instantiate(m_data.GhostCharModel.gameObject, transform);
    }

    public void SetData(GhostDataBase data)
    {
        m_data = data;

        ChangeModel();
    }

    /// <summary>
    /// The game mode calls this
    /// </summary>
    public void Die()
    {
        SwitchState(GhostState.Dying);
        IsDead = true;
        m_col.enabled = false;

        StartCoroutine(Blink());
    }

    void StartRespawnTimer(float time)
    {
        SwitchState(GhostState.Respawning);
        //respawn after time
        Invoke("Respawn", time);
    }

    void Respawn()
    {
        //REWORK THIS, THE GHOST SHOULDN'T BE PICKING
        Vector3 pos = GhostRespawnZone.transform.position;
        pos.x = Random.Range(GhostRespawnZone.bounds.min.x, GhostRespawnZone.bounds.max.x);
        pos.z = Random.Range(GhostRespawnZone.bounds.min.z, GhostRespawnZone.bounds.max.z);
        transform.position = pos;

        IsDead = false;
        
        SwitchState(GhostState.Standby);

        m_model.gameObject.SetActive(true);
        m_col.enabled = true;
    }

    public void PickRespawnZone(Collider respawnZone)
    {
        GhostRespawnZone = respawnZone;
    }

    public void SetIsAI(bool isAI = true)
    {
        if(isAI)
        {
            GhostAiController ai = GetComponent<GhostAiController>();
            if (!ai)
                ai = gameObject.AddComponent<GhostAiController>();

            ai.PossessPawn(this);
        }
    }

    IEnumerator Blink()
    {
        float currTime = 0f;

        while (currTime <= 3f)
        {
            m_model.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.5f);

            m_model.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            currTime += 1f;
        }

        m_model.gameObject.SetActive(false);

        StartRespawnTimer(m_data.RespawnTime);

        yield return null;
    }

    void SwitchState(GhostState nextState)
    {
        EOnStateChange?.Invoke(m_currGState, nextState);
        m_currGState = nextState;

        switch (nextState)
        {
            case GhostState.InitialSpawn:
                break;
            case GhostState.Standby:
                break;
            case GhostState.Active:
                break;
            case GhostState.Dying:
                break;
            case GhostState.Dead:
                break;
            case GhostState.Respawning:
                break;
            default:
                break;
        }
    }
}
