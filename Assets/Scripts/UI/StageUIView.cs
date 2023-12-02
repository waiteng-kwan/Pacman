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

    public TextMeshProUGUI ScoreCounter => m_scoreCounter;
    public TextMeshProUGUI LifeCounter => m_lifeCounter;
}
