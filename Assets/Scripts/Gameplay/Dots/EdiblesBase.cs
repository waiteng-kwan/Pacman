using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
using UnityEngine.Events;

public class EdiblesBase : MonoBehaviour
{
    [Header("Data")]
    [SerializeField, NaughtyAttributes.Expandable]
    public EdibleData m_data;
    public EdibleData.EdibleType DotPelletType => m_data.EdibleDotType;

    //destroy event
    public UnityEvent<EdibleData.EdibleType> EOnDestroy { get; private set; }

    private void Awake()
    {
        EOnDestroy = new UnityEvent<EdibleData.EdibleType>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (other.GetComponent<PacmanBehaviour>())
            {
                ResolveCollideWithPlayer(other.GetComponent<PacmanBehaviour>());
                ResolveEatenBehaviour();
            }
        }
    }

    protected void OnDestroy()
    {
        EOnDestroy?.Invoke(DotPelletType);
        EOnDestroy.RemoveAllListeners();
    }

    protected virtual void ResolveCollideWithPlayer(PacmanBehaviour player)
    {
        //! TODO: remove direct reference
        GameModeBase.Instance.PlayerScored(player.OwnerIndex, m_data.CalculateScoreToAdd());
    }

    protected void ResolveEatenBehaviour()
    {
        if(m_data.WillRespawn)
        {
            gameObject.SetActive(false);
            Invoke("Respawn", m_data.TimeToRespawn);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    protected void Respawn()
    {
        gameObject.SetActive(true);
    }
}
