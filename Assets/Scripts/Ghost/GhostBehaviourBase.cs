using Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GhostBehaviourBase : MonoBehaviour
{
    public enum GhostState
    {
        InitialSpawn,
        Standby,       //waiting to leave zone
        Active,        //on field
        Dying,         //extra state for animation
        Dead,
        Respawning     //loops back to standby
    }

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
    private Collider m_ghostRespawnZone;
    public Collider GhostRespawnZone => m_ghostRespawnZone;

    //states
    [NaughtyAttributes.ReadOnly, SerializeField]
    private GhostState m_currGState;

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

    // Start is called before the first frame update
    void Start()
    {
        //testing!!!!
        SetAIState();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(transform.position, transform.forward * 15f, Color.magenta);
        Debug.DrawRay(transform.position, Vector3.left * 5f, Color.cyan);
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
        GetComponent<GhostAiController>().enabled = false;

        m_currGState = GhostState.Dead;
        StartCoroutine(Blink());
    }

    void StartRespawnTimer(float time)
    {
        Invoke("Respawn", m_data.RespawnTime);
    }

    void Respawn()
    {
        //pick respawn point
        Vector3 pos = m_ghostRespawnZone.transform.position;
        pos.x = Random.Range(m_ghostRespawnZone.bounds.min.x, m_ghostRespawnZone.bounds.max.x);
        pos.z = Random.Range(m_ghostRespawnZone.bounds.min.z, m_ghostRespawnZone.bounds.max.z);
        transform.position = pos;

        GetComponent<GhostAiController>().enabled = true;

        m_model.gameObject.SetActive(true);
    }

    public void PickRespawnZone(Collider respawnZone)
    {
        m_ghostRespawnZone = respawnZone;
    }

    void SetAIState(bool isAI = true)
    {
        if(isAI)
        {
            gameObject.AddComponent<GhostAiController>();
            GetComponent<GhostAiController>().SetPawn(this);
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

        StartRespawnTimer(5f);

        yield return null;
    }
}
