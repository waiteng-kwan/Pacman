using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GhostBehaviourBase : MonoBehaviour
{
    public enum GhostState
    {
        Normal,
        Edible
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

    private NavMeshAgent m_navMeshAgentCmp;
    [SerializeField] 
    private Transform m_destination;

    private void OnValidate()
    {
        m_model = GetComponentInChildren<MeshRenderer>().gameObject;
        m_rb = GetComponent<Rigidbody>();
        m_col = GetComponent<Collider>();

        //nav mesh stuff
        m_navMeshAgentCmp = GetComponent<NavMeshAgent>();

        if (m_data == null)
            Debug.LogError("Ghost data is missing on " + gameObject.name);
    }

    private void Awake()
    {
        m_model = GetComponentInChildren<MeshRenderer>().gameObject;
        m_rb = GetComponent<Rigidbody>();
        m_col = GetComponent<Collider>();

        //nav mesh stuff
        m_navMeshAgentCmp = GetComponent<NavMeshAgent>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Invoke("UpdateAgentDestination", 1f);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(transform.position, transform.forward * 15f, Color.magenta);
    }
    //testing only
    void UpdateAgentDestination()
    {
        UpdateAgentDestination(m_destination);
    }

    void UpdateAgentDestination(Transform transform)
    {
        m_navMeshAgentCmp.SetDestination(transform.position);
    }
}
