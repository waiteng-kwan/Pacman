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
        public PriorityQueue<Vector3, float> m_nodePriority;
    }
}