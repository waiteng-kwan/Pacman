using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicDot : MonoBehaviour
{
    [Header("Data")]
    [SerializeField, NaughtyAttributes.Expandable]
    public DotData m_data;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (other.GetComponent<PlayerBehaviour>())
            {
                GameMode.gameMode.PlayerScored(other.GetComponent<PlayerBehaviour>().BelongToPlayerIndex, m_data.CalculateScoreToAdd());
                Destroy(gameObject);
            }
        }
    }
}
