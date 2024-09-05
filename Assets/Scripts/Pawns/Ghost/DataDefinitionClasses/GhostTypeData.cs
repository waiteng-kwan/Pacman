using UnityEngine;

///This file is used to hold all the enums and other data for ghost
namespace Core.Ghost
{ 
    public enum GhostType
    {
        White = 0,        //this is default!
        Red,
        Cyan,
        Blue,
        Orange,
        Pink
    }

    public enum GhostAiState
    {
        StandBy = 0, //nothing
        Idle,        //at home point idling
        Patrol,      //wobbling around
        Chasing,     //chasing player
        Returning,   //returning to home point
        RunAway,     //running away from player, frightened
    }

    public enum AiTargetType
    {
        OnPlayer = 0,
        AroundPlayer,
        OtherAiDependant,
        DetectionBased
    }

    public class MovementWeight
    {
        Vector4 Weight = Vector4.zero;
        public float Up => Weight.w;
        public float Down => Weight.x;
        public float Left => Weight.y;
        public float Right => Weight.z;
        float Total => Weight.w + Weight.x + Weight.y + Weight.z;

        public MovementWeight(float up, float down, float left, float right)
        {
            Weight = new Vector4 (up, down, left, right);
        }
    }

    public class GhostMovementData
    {
        Vector2 MoveVec = Vector2.zero;
        Vector2 NormalizedMoveVec => MoveVec.normalized;
        public float HorizontalMoveSpeed => MoveVec.x;
        public float UpDownMoveSpeed => MoveVec.y;
        public float Speed { get; private set; }

        public GhostMovementData(Vector2 moveVec, float speed)
        {
            MoveVec = moveVec;
            Speed = speed;
        }
    }
}