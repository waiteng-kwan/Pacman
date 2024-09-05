using Core.Ghost;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Core.Ghost
{
    [CreateAssetMenu(fileName = "GhostDataBase",
menuName = "Scriptable Objects/Pawn/Ghost Type Data", order = 0)]
    public class GhostDataBase : PawnDataBase
    {
        //chara
        [Header("Character")]
        public GhostType GhostCharType;

        [Header("AI")]
        public bool UseNavMesh = false;          //replace with AI Tree next time
                                                 //the time before it changes state, x = min, y = max
        [NaughtyAttributes.InfoBox("X = min, Y = max")]
        public Vector2 ChangeStateDampingRange = Vector2.one;
        [NaughtyAttributes.InfoBox("X = min, Y = max")]
        public Vector2 ChangeIdleWaitngRange = Vector2.one;
        public float DetectionRange = 8f;
        [Tooltip("x = x weight, y = y weight, etc...")]
        public Vector4 DirectionWeight = Vector4.zero;

        [Header("Gameplay")]
        public float RespawnTime = 5f;
        public float GhostIdleTime = 3.5f;
    }
}