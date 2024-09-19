using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityAtoms.BaseAtoms;
using TMPro;

namespace UI
{
    public class UIPlayerScore : MonoBehaviour
    {
        [Header("UnityAtoms")]
        [SerializeField]
        private IntVariable m_currentScore;
        [SerializeField]
        private IntConstant m_startingScore;

        [Header("UI")]
        [SerializeField]
        private TextMeshProUGUI m_scoreTxtCmp;

        private void OnValidate()
        {
            m_scoreTxtCmp ??= GetComponentInChildren<TextMeshProUGUI>();
        }

        private void Start()
        {
            m_scoreTxtCmp ??= GetComponentInChildren<TextMeshProUGUI>();
        }

        public void OnScoreChanged()
        {
            m_scoreTxtCmp ??= GetComponentInChildren<TextMeshProUGUI>();

            m_scoreTxtCmp.text = m_currentScore.Value.ToString();
        }
    }
}