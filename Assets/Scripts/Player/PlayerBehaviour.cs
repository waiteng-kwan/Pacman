using Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerBehaviour : MonoBehaviour
{
    [Header("Data")]
    [SerializeField, NaughtyAttributes.Expandable]
    private PacmanBaseData m_data;

    [Header("Debug")]
    [NaughtyAttributes.ReadOnly, SerializeField]
    private GameObject m_model;
    [NaughtyAttributes.ReadOnly, SerializeField]
    private Rigidbody m_rb;
    [NaughtyAttributes.ReadOnly, SerializeField]
    private PlayerController m_belongsTo;
    public int BelongToPlayerIndex => m_belongsTo.Index;

    private void OnValidate()
    {
        m_model = GetComponentInChildren<MeshRenderer>().gameObject;
        m_rb = GetComponent<Rigidbody>();

        if (m_data == null)
            Debug.LogError("Pacman data is missing on " + gameObject.name);
    }

    private void Awake()
    {
        m_model = GetComponentInChildren<MeshRenderer>().gameObject;
        m_rb = GetComponent<Rigidbody>();

        ChangeModel();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Vector3 moveVec = Vector3.zero;
    private void FixedUpdate()
    {
        m_rb.velocity = moveVec * m_data.Speed;
    }

    public void SetMoveDir(Vector2 moveDir)
    {
        //x = left, right, y = up, down (in 3d space its z)
        moveVec.x = moveDir.x;
        moveVec.z = moveDir.y;
    }

    public void ResetMovement()
    {
        moveVec = Vector3.zero;
    }

    public void ChangeModel()
    {
        if(m_model)
        {
            //destroy current model and spawn new one
            Destroy(m_model);

            m_model = Instantiate(m_data.PacmanCharModel.gameObject, transform);
        }
    }

    public void SetOwner(PlayerController owner)
    {
        m_belongsTo = owner;
    }
}
