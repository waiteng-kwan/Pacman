using Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PawnDataBase",
menuName = "Scriptable Objects/Pawn/Basic Pawn Data", order = 1)]
public class PawnDataBase : ScriptableObject
{
    [Header("Prefab")]
    public PawnBase PawnPrefab;

    [Header("Visual")]
    public string DisplayName;
    public MeshRenderer CharacterModel;

    [Header("Movement")]
    public float Speed = 5f;
    public bool HasAccelDeccel = false;

    [Header("Audio - SFX")]
    public AudioClip SfxDieNormal;
    public AudioClip SfxSpawnNormal;
    public AudioClip SfxWalkNormal;
}