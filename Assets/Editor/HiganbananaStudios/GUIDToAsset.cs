using UnityEditor;
using UnityEngine;

//see https://learn.unity.com/tutorial/editor-scripting for more
public class GUIDToAsset : EditorWindow
{
    string guid;
    Object objToFind = null;

    [MenuItem("Higanbanana/GUIDToAsset")]
    public static void ShowWindow()
    {
        GetWindow<GUIDToAsset>();
    }

    private void OnGUI()
    {
        GUILayout.Label("Asset to GUID", EditorStyles.boldLabel);

        GUILayout.Space(5);
        GUILayout.Label("GUID & asset path, look at the console", EditorStyles.label);

        guid = EditorGUILayout.TextField("GUID", guid);
        objToFind = EditorGUILayout.ObjectField("Asset", objToFind, typeof(Object), false);

        GUILayout.BeginHorizontal();
        // find asset path by guid
        if (GUILayout.Button("Find Asset By GUID"))
        {
            if (!string.IsNullOrEmpty(guid))
            {
                //check for guid field
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Debug.Log($"Asset Path: {path}");
            }
        }

        if (GUILayout.Button("Get GUID From Object Or Selection"))
        {
            if (!objToFind && !Selection.activeObject)
            {
                Debug.LogError("Asset field is blank or no object selected in asset explorer");
                return;
            }

            objToFind ??= Selection.activeObject;

            string path = AssetDatabase.GetAssetPath(objToFind);
            string tmpGuid = AssetDatabase.AssetPathToGUID(path);
            Debug.Log($"Find: {objToFind.name} - Asset Path: {path}  |  GUID: {tmpGuid}");
        }
        GUILayout.EndHorizontal();
    }

    [MenuItem("Higanbanana/")]
    private static void NewMenuOption()
    {
    }

    private void FindGUID()
    {

    }
}
