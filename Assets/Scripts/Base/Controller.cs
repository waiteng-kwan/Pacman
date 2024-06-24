using NaughtyAttributes;
using UnityEngine;

namespace Game
{
    public class Controller : MonoBehaviour
    {
        //serialize properties use field:
        [field: SerializeField]
        public int Index { get; protected set; } = -1;

        //character/pawn
        [field: SerializeField, ReadOnly]
        public PawnBase Pawn { get; protected set; }

        //ai
        public bool IsAIMode { get; protected set; }
        public bool UpdateAI { get; protected set; }

        #region Unity Fn
        protected virtual void OnValidate()
        {
        }

        protected virtual void Awake()
        {
        }

        protected virtual void OnDestroy()
        {
        }
        #endregion

        public void SetIndex(int index)
        {
            Index = index;
        }

        public virtual void PossessPawn(PawnBase pawn)
        {
            Pawn = pawn;
            Pawn.SetOwner(this);
        }
        public virtual void UnposessPawn()
        {
            Pawn.SetOwner(null);
            Pawn = null;
        }

        public virtual void SetAIMode(bool isAi)
        {
            IsAIMode = isAi;
        }

        public virtual void ToggleActiveAI(bool isActive)
        {
            UpdateAI = isActive;
        }
    }
} 