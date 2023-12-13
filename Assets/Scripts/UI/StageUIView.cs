using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StageUIView : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI m_scoreCounter;
    [SerializeField]
    private TextMeshProUGUI m_lifeCounter;
    [SerializeField]
    private GameObject m_cdGrp;

    public TextMeshProUGUI ScoreCounter => m_scoreCounter;
    public TextMeshProUGUI LifeCounter => m_lifeCounter;
    public GameObject CountdownGroup => m_cdGrp;
}
