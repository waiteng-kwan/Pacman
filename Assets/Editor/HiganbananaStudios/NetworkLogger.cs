using UnityEditor;
using UnityEngine;

//see https://learn.unity.com/tutorial/editor-scripting for more
public class NetworkLogger : EditorWindow
{
    [MenuItem("Higanbanana/NetworkLogger")]
    public static void ShowWindow()
    {
        GetWindow<NetworkLogger>();
    }

    private void OnGUI()
    {
    }
}