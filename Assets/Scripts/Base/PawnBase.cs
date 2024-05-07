using NaughtyAttributes;
using UnityEngine;

namespace Game
{
    public class PawnBase : MonoBehaviour
    {
        [Header("Global Settings")]
        //serialize properties use field:
        [field: SerializeField]
        public int PawnIndex { get; protected set; } = -1;
        [field: SerializeField]
        public Controller Owner { get; protected set; }
        [property: SerializeField]
        public int OwnerIndex { get => Owner.Index; }

        [Header("Character")]
        [field: SerializeField]
        public int SkinIndex { get; protected set; } = -1;
        [SerializeField, ReadOnly]
        protected GameObject m_model;
        public GameObject Model => m_model;

        [Header("Physics")]
        [SerializeField, ReadOnly]
        protected Rigidbody Rigidbody;
        [SerializeField, ReadOnly]
        protected Collider Collider;

        protected virtual void OnValidate()
        {
            Rigidbody = GetComponent<Rigidbody>();
            Collider = GetComponent<Collider>();
        }

        protected virtual void Awake()
        {
            if (!Rigidbody)
                Rigidbody = GetComponent<Rigidbody>();
            if (!Collider)
                Collider = GetComponent<Collider>();
        }

        public void SetOwner(Controller newOwner)
        {
            Owner = newOwner;
        }

        public virtual void ChangeModel()
        {
            if (m_model)
            {
                //destroy current model and spawn new one
                Destroy(m_model);
            }
        }
    }
}