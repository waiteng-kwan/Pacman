using UnityEngine;

namespace Core
{
    public class WarpZone : MonoBehaviour
    {
        private Collider m_warpZone;
        [SerializeField]
        private Transform m_warpTo;

        private void OnValidate()
        {
            m_warpZone ??= GetComponent<Collider>();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnTriggerEnter(Collider other)
        {
            IWarpable cmp = other.GetComponent<IWarpable>();
            if (cmp != null)
            {
                //dont want to ping pong
                if (cmp.IsWarping)
                {
                    if(cmp.TeleportedFrom != transform)
                        cmp.DoneTeleporting();
                }
                else
                    cmp.TeleportTo(transform, m_warpTo.transform.position, m_warpTo.transform.forward);
            }
        }
    }
}