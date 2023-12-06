using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GhostBehaviourBase : MonoBehaviour
{
    public enum GhostState
    {
        InitialSpawn,
        Active,
        Dead,
        Respawning
    }

    [Header("Data")]
    [SerializeField, NaughtyAttributes.Expandable]
    private GhostDataBase m_data;

    [Header("(Read Only) for Debug")]
    [NaughtyAttributes.ReadOnly, SerializeField]
    private GameObject m_model;
    [NaughtyAttributes.ReadOnly, SerializeField]
    private Rigidbody m_rb;
    [NaughtyAttributes.ReadOnly, SerializeField]
    private Collider m_col;

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
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(transform.position, transform.forward * 15f, Color.magenta);
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

    void Die()
    {
        gameObject.SetActive(false);


    }

    void StartRespawnTimer()
    {
        Invoke("Respawn", m_data.RespawnTime);
    }

    void Respawn()
    {
        gameObject.SetActive(true);
    }
}
