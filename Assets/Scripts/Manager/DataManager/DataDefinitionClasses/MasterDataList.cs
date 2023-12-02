using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MasterDataList",
menuName = "Scriptable Objects/MasterDataList Data", order = 1)]
public class MasterDataList : ScriptableObject
{
    [Header("Characters")]
    public List<ScriptableObject> m_charDataList = new List<ScriptableObject>();

    [Header("Ghosts")]
    public List<ScriptableObject> m_ghostDataList = new List<ScriptableObject>();

    [Header("Dots/Pellets")]
    public List<ScriptableObject> m_dotDataList = new List<ScriptableObject>();
}