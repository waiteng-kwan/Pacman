using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// put this on an object you want to be able to warp?
    /// </summary>
    public class Warpable : MonoBehaviour, IWarpable
    {
        private bool m_isWarping;
        public bool IsWarping
        {
            get => m_isWarping;
            private set => m_isWarping = value;
        }

        private Transform m_tpFrom = null;
        public Transform TeleportedFrom
        {
            get => m_tpFrom;
            private set => m_tpFrom = value;
        }

        public void DoneTeleporting()
        {
            IsWarping = false;
            m_tpFrom = null;
        }

        public void TeleportTo(Transform tpFrom, Vector3 tpTo, Vector3 forward)
        {
            IsWarping = true;
            m_tpFrom = tpFrom;

            tpTo.y = transform.position.y;
            transform.position = tpTo;
            transform.rotation = Quaternion.LookRotation(forward);
        }
    }
}