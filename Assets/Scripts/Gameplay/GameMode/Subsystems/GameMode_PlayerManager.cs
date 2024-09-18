using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityAtoms;

namespace Core
{
    public class GameMode_PlayerManager : MonoBehaviour
    {
        [Header("Player Variables")]
        [SerializeField]
        private List<AtomBaseVariable> m_playerHealth;

        public void ChangePlayerHealth(int index)
        {
            if (index >= m_playerHealth.Count)
                return;

            //m_playerHealth[index].
        }
    }
}