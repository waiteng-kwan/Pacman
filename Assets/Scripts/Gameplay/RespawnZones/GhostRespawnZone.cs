using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Ghost
{
    public class GhostRespawnZone : MonoBehaviour
    {
        [ReadOnly, SerializeField]
        private Queue<GhostBehaviourBase> m_toRespawn = new Queue<GhostBehaviourBase>();

        private Image m_fill;

        private void OnValidate()
        {
            m_fill = GameObject.Find("WorldSpaceCanvas/ImgRespawnFill").GetComponent<Image>();
        }

        private void Awake()
        {
            m_fill = GameObject.Find("WorldSpaceCanvas/ImgRespawnFill").GetComponent<Image>();
        }

        private void Update()
        {
            if (m_toRespawn.Count <= 0)
                return;
        }

        public void EnqueueRespawnGhost(GhostBehaviourBase ghost)
        {
            m_toRespawn.Enqueue(ghost);
        }

        public void DequeueRespawnGhost(GhostBehaviourBase ghost)
        {
            if (m_toRespawn.Contains(ghost))
            {
                m_toRespawn.Dequeue();
            }
        }
    }
}