using UnityEditor;
using UnityEngine;

public class GlobalLevelSettings : EditorWindow
{
    public Vector2 FloorSize = Vector2.one;
    public Vector2 TileSize = Vector2.one;

    [MenuItem("Higanbanana/Pellet Man/Global Level Settings")]
    public static void ShowWindow()
    {
        GetWindow<GlobalLevelSettings>();
    }

    private void OnGUI()
    {
        GUILayout.Label("Level Settings", EditorStyles.boldLabel);

        FloorSize = EditorGUILayout.Vector2Field("Floor/Stage Size", FloorSize);
        TileSize = EditorGUILayout.Vector2Field("Tile Size", TileSize);
    }
}