using NaughtyAttributes;
using UnityEngine;

namespace Game
{
    public class CharacterController : MonoBehaviour
    {
        //serialize properties use field:
        [field: SerializeField]
        public int Index { get; protected set; } = -1;

        //character/pawn
        [field: SerializeField, ReadOnly]
        public PawnBase Pawn { get; protected set; }

        public void SetIndex(int index)
        {
            Index = index;
        }
    }
} 