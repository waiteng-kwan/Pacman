using UnityEditor;
using UnityEngine;

public class ClearPlayerPrefs : EditorWindow
{
    [MenuItem("Higanbanana/Clear PlayerPrefs")]
    private static void NewMenuOption()
    {
        PlayerPrefs.DeleteAll();
    }
}