using UnityEngine;

///This file is used to hold all the enums and other data for ghost
namespace Game.Ghost
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
        Idle,
        Patrol,      //wobbling around
        Chasing,     //chasing player
        Returning,   //returning to home point
        RunAway,     //running away from player, frightened
        StandBy      //nothing
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
}