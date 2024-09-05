using NaughtyAttributes;
using UnityEngine;

namespace Core
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

        [Header("Settings")]
        [SerializeField/*, ReadOnly*/, Expandable]
        protected PawnDataBase m_settings;
        public virtual PawnDataBase Settings 
        { 
            get => m_settings; 
            protected set => m_settings = value;
        }

        [Header("Character")]
        [SerializeField, ReadOnly]
        protected GameObject m_visualModelRoot;
        public GameObject Model => m_visualModelRoot;
        [field: SerializeField]
        public int SkinIndex { get; protected set; } = -1;

        [Header("Physics")]
        [SerializeField, ReadOnly]
        protected Rigidbody Rigidbody;
        [SerializeField, ReadOnly]
        protected Collider Collider;
        protected Vector3 m_moveVecDir;

        [Header("Others")]
        public bool IsInvul = false;

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

            InitializePawn();
        }

        /// <summary>
        /// Called at the end of PawnBase.Awake()
        /// Do child class stuff here
        /// </summary>
        protected virtual void InitializePawn()
        {
        }

        public void SetOwner(Controller newOwner)
        {
            Owner = newOwner;
        }

        public void ChangeModel(GameObject newModel)
        {
            if (m_visualModelRoot)
            {
                //destroy current model and spawn new one
                Destroy(m_visualModelRoot);
            }

            m_visualModelRoot = Instantiate(newModel, transform);
        }

        public void SetSettingsData(PawnDataBase data)
        {
            m_settings = data;

            InternalSetSettingsData(data);
        }

        protected virtual void InternalSetSettingsData(PawnDataBase data)
        { }

        #region Movement
        public virtual void SetMoveDir(Vector2 moveDir)
        {
            //x = left, right, y = up, down (in 3d space its z)
            m_moveVecDir.x = moveDir.x;
            m_moveVecDir.z = moveDir.y;
        }

        public virtual void ResetMovement()
        {
            m_moveVecDir = Vector3.zero;
        }
        #endregion
    }
}