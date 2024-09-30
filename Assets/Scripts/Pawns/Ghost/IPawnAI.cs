using Core.Ghost;
using UnityEngine;

namespace Core
{
    public interface IPawnAI<T> where T : struct
    {
        void PossessPawn(PawnBase pawn);

        //state
        void SetNextState(T value);
        T GetCurrentState();
        void PrepChangeState(T nextState);
        void SwitchState();

        //Movement
        void SetDestination(Vector3 destination);
        void SetTarget(Transform target);
    }
}