using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
using UnityEngine.Events;

public class BasicDot : MonoBehaviour
{
    [Header("Data")]
    [SerializeField, NaughtyAttributes.Expandable]
    public DotData m_data;
    public DotData.DotType DotPelletType => m_data.EdibleDotType;

    //destroy event
    public UnityEvent<DotData.DotType> EOnDestroy { get; private set; }

    private void Awake()
    {
        EOnDestroy = new UnityEvent<DotData.DotType>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (other.GetComponent<PlayerBehaviour>())
            {
                ResolveCollideWithPlayer(other.GetComponent<PlayerBehaviour>());
                ResolveEatenBehaviour();
            }
        }
    }

    protected void OnDestroy()
    {
        EOnDestroy?.Invoke(DotPelletType);
        EOnDestroy.RemoveAllListeners();
    }

    protected virtual void ResolveCollideWithPlayer(PlayerBehaviour player)
    {
        GameModeBase.gameMode.OnPlayerScored(player.BelongToPlayerIndex, m_data.CalculateScoreToAdd());
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
