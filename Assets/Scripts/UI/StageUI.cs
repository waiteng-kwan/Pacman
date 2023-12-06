using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
public class StageUI : MonoBehaviour
{
    StageUIView m_view;

    private void Awake()
    {
        m_view = GetComponent<StageUIView>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //GameModeBase.gameMode.EPlayerScored.AddListener(OnPlayerScored);
        //GameModeBase.gameMode.EPlayerLifeChanged.AddListener(OnPlayerLifeChanged);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnPlayerScored(int index, int score)
    {
        m_view.ScoreCounter.text = score.ToString();
    }

    void OnPlayerLifeChanged(int index, int newLife)
    {
        m_view.LifeCounter.text = newLife.ToString();
    }
}
