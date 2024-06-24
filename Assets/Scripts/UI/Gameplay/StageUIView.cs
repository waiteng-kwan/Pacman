using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI
{
    public class StageUIView : MonoBehaviour
    {
        [SerializeField]
        private StagePlayerGroupUI[] m_players;
        [SerializeField]
        private TextMeshProUGUI m_lifeCounter;
        [SerializeField]
        private GameObject m_cdGrp;

        public StagePlayerGroupUI[] Players;
        public StagePlayerGroupUI GetPlayer(int id)
        {
            return m_players[id];
        }
        public GameObject CountdownGroup => m_cdGrp;
    }
}