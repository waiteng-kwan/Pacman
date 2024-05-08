using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PacmanBaseData",
menuName = "Scriptable Objects/Pawn/Pacman Type Data", order = 1)]
public class PacmanBaseData : PawnDataBase
{
    public enum PacmanType
    {
        Yellow = 0,        //this is default!
        Pink,
        Purple,
        Blue,
        Red,
        Black
    }

    //chara
    [Header("Character")]
    public PacmanType PacmanCharType;

    [Header("SFX")]
    public AudioClip SfxEatNormal;
}