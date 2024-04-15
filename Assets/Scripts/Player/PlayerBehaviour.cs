using Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GhostBehaviourBase;
using Game;


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
    public PlayerController Owner => m_belongsTo;

    //attributes
    private PlayerBehaviourAttributes m_attributes;
    public PlayerBehaviourAttributes Attributes => m_attributes;

    #region Unity Fn
    private void OnValidate()
    {
        m_rb = GetComponent<Rigidbody>();

        if (m_data == null)
            Debug.LogError("Pacman data is missing on " + gameObject.name);

        m_attributes = GetComponent<PlayerBehaviourAttributes>();
    }

    private void Awake()
    {
        m_rb = GetComponent<Rigidbody>();
        m_attributes = GetComponent<PlayerBehaviourAttributes>();

        if (m_data != null)
        {
            ChangeModel();
        }
    }

    private Vector3 moveVec = Vector3.zero;
    private void FixedUpdate()
    {
        m_rb.velocity = moveVec * m_data.Speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Ghost" && !Attributes.IsPlayerInvul())
        {
            var ghostBehaviour = other.GetComponent<GhostBehaviourBase>();
            if (ghostBehaviour == null)
                return;

            GameModeBase.Instance.PlayerCollidedWithGhost(ghostBehaviour, this);
        }
    }
    #endregion

    #region Movement
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
    #endregion

    public void ChangeModel()
    {
        if(m_model)
        {
            //destroy current model and spawn new one
            Destroy(m_model);
        }

        m_model = Instantiate(m_data.PacmanCharModel.gameObject, transform);
    }

    public void SetData(PacmanBaseData data)
    {
        m_data = data;

        ChangeModel();
    }

    public void SetOwner(PlayerController owner)
    {
        m_belongsTo = owner;
    }


    public void SetPlayerEatGhostState(bool canEat)
    {
        m_attributes.SetCanEatGhostState(canEat);
    }

    public void SetPlayerState(PlayerCharacterStates state)
    {
        m_attributes.SetState(state);

        //handle state change here
        switch (state)
        {
            case PlayerCharacterStates.Alive:
                break;
            case PlayerCharacterStates.Dead:
                break;
            case PlayerCharacterStates.Respawning:
                break;
            default:
                break;
        }
    }
}
