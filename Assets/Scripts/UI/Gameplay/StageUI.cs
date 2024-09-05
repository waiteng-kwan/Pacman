using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core;

namespace UI
{
    public class StageUI : MonoBehaviour
    {
        StageUIView m_view;

        private void Awake()
        {
            m_view = GetComponent<StageUIView>();

            Initialize();
        }

        // Start is called before the first frame update
        void Start()
        {
            GameModeBase.Instance.EPlayerScored.AddListener(OnPlayerScored);
            GameModeBase.Instance.EPlayerLifeChanged.AddListener(OnPlayerLifeChanged);
        }

        void Initialize()
        {
            //m_view.ScoreCounter.text = "0";
            //m_view.LifeCounter.text = GameModeBase.Instance.Settings.StartingHealth.ToString();
        }

        void OnPlayerScored(int index, int score)
        {
            //m_view.ScoreCounter.text = score.ToString();
        }

        void OnPlayerLifeChanged(int index, int newLife)
        {
            //m_view.LifeCounter.text = newLife.ToString();
        }
    }
}