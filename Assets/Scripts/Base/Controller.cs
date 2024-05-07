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
    }
} 