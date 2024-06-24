using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI
{
    public class StagePlayerGroupUI : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField]
        private int m_pId;
        [SerializeField]
        private TextMeshProUGUI m_playerIdTxt;
        [SerializeField]
        private TextMeshProUGUI m_playerName;

        [Header("health")]
        [SerializeField]
        private Transform m_hpCounterParent;

        [Header("Score")]
        [SerializeField]
        private TextMeshProUGUI m_scoreCounter;

        public TextMeshProUGUI ScoreCounter => m_scoreCounter;

        public void Initialize(int maxHp = 3)
        {
            if (m_hpCounterParent.childCount < maxHp)
            {
                int spawn = maxHp - m_hpCounterParent.childCount;
                for(int i = 0; i < spawn; i++)
                    Instantiate(m_hpCounterParent.GetChild(0), m_hpCounterParent);
            }

            for (int i = 0; i < m_hpCounterParent.childCount; i++)
            {
                m_hpCounterParent.GetChild(i).gameObject.SetActive(i < maxHp);
            }
        }

        public void SetPlayerName(string playerName, int id = -1)
        {
            m_playerName.text = playerName;

            if(id < 0)
                gameObject.SetActive(false);
            else
            {
                gameObject.SetActive(true);
                m_playerIdTxt.text = "#" + id.ToString();
            }
        }

        public void SetScore(int score)
        {
            if (score < 0)
                score = 0;

            m_scoreCounter.text = score.ToString();
        }
    }
}