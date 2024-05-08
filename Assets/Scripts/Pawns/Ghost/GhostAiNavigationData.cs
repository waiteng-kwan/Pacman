using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Game
{
    public class GhostAiNavigationNode
    {
        public Vector3 Destination { get; private set; }
        public float Weight { get; private set; }

        public GhostAiNavigationNode(Vector3 destination, float weight)
        {
            Destination = destination;
            Weight = weight;
        }
    }

    public class GhostAiNavigationData
    {
        public PriorityQueue<Vector3, float> m_nodePriority = new PriorityQueue<Vector3, float>();
        public List<GameObject> m_patrolPath = new List<GameObject>();
        private List<Vector3> m_idlePath = new List<Vector3>();
        public List<Vector3> IdlePath => m_idlePath;
        int m_idlePathTraverseIndex = 0;

        public void AddPointToIdlePath(Vector3 pt)
        {
            m_idlePath.Add(pt);
        }

        public void RemovePointFromIdlePath(Vector3 pt)
        {
            m_idlePath.Add(pt);
        }

        public void ClearIdle()
        {
            m_idlePath.Clear();
        }

        public Vector3 CurrentPointOnIdlePath()
        {
            if (m_idlePath.Count == 0)
                return Vector3.zero;

            return m_idlePath[m_idlePathTraverseIndex];
        }

        public Vector3 NextPointOnIdlePath()
        {
            if(m_idlePath.Count == 0)
                return Vector3.zero;

            if (++m_idlePathTraverseIndex >= m_idlePath.Count)
                m_idlePathTraverseIndex = 0;

            return m_idlePath[m_idlePathTraverseIndex];
        }
    }
}