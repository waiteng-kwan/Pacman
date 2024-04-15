using NaughtyAttributes;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelBuilder : EditorWindow
{
    public Vector2Int FloorSize = Vector2Int.one;
    public Vector3Int TileSize = Vector3Int.one;
    public GameObject CubePrefab = null;
    public GameObject DefaultGameBoard = null;
    Transform lvlParent;

    [MenuItem("Tools/Pellet Man/Level Builder")]
    public static void ShowWindow()
    {
        GetWindow<LevelBuilder>();
    }

    private void OnGUI()
    {
        GUILayout.Label("Level Settings", EditorStyles.boldLabel);

        FloorSize = EditorGUILayout.Vector2IntField("Floor/Stage Size", FloorSize);
        TileSize = EditorGUILayout.Vector3IntField("Tile Size", TileSize);

        //CubePrefab = AssetDatabase.FindAssets("")
        CubePrefab = EditorGUILayout.ObjectField("Basic Tile Prefab", CubePrefab, typeof(GameObject), false) as GameObject;

        if (GUILayout.Button("Create Basic Level"))
        {
            CreateLevel();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Create Basic Cube"))
        {
            CreateTile();
        }

        GUILayout.Space(30);

        if(GUILayout.Button("DestroyLevel"))
        {
            DestroyLevel();
        }
    }

    void CreateLevel()
    {
        ResolveLevelRoot();

        Transform floor = lvlParent.Find("Floor");

        if (!CubePrefab)
        {
            Debug.LogError("[LevelBuilder] No prefab to spawn");
            return;
        }

        if (!floor)
        {
            floor = CreateGameObject(CubePrefab, "Floor", lvlParent);
            floor.transform.position = Vector3.zero;
            floor.rotation = Quaternion.identity;
            floor.localScale = new Vector3(FloorSize.x, 0.2f, FloorSize.y);
        }

        Transform wallParent = lvlParent.Find("Walls");

        if (!wallParent)
            wallParent = CreateGameObject(null, "Walls", lvlParent);

        //4 walls
        int cubesInX = FloorSize.x / TileSize.x;
        int cubesInY = FloorSize.y / TileSize.z;
        Vector3 halfTileSize = new Vector3(TileSize.x * 0.5f, TileSize.y * 0.5f, TileSize.z * 0.5f);

        //spawn cubes
        float startX = floor.position.x - floor.localScale.x * 0.5f + halfTileSize.x;
        float startY = floor.position.z - floor.localScale.z * 0.5f + halfTileSize.z;

        //upper ^^^
        Transform wonderWall = wallParent.Find("TopWall");
        if (!wonderWall)
            wonderWall = CreateGameObject(null, "TopWall", wallParent);

        for (int j = 0; j < cubesInX; j++)
        {
            Vector2 pos = new Vector2(startX + j * TileSize.x,
                                      startY);
            Transform tmp = CreateGameObject(CubePrefab, "Basic Wall Tile", wonderWall);
            tmp.position = new Vector3(pos.x, halfTileSize.y, pos.y);
            tmp.localScale = TileSize;
        }

        //left <<<
        wonderWall = wallParent.Find("LeftWall");
        if (!wonderWall)
            wonderWall = CreateGameObject(null, "LeftWall", wallParent);

        //start from 1 bc top & bottom wall has cube alr
        for (int j = 1; j < cubesInY - 1; j++)
        {
            Vector2 pos = new Vector2(startX,
                                      startY + j * TileSize.z);
            Transform tmp = CreateGameObject(CubePrefab, "Basic Wall Tile", wonderWall);
            tmp.position = new Vector3(pos.x, halfTileSize.y, pos.y);
            tmp.localScale = TileSize;
        }

        //bottom vvv
        startY = floor.position.y + floor.localScale.z * 0.5f - halfTileSize.y;

        wonderWall = wallParent.Find("BottomWall");
        if (!wonderWall)
            wonderWall = CreateGameObject(null, "BottomWall", wallParent);

        for (int j = 0; j < cubesInX; j++)
        {
            Vector2 pos = new Vector2(startX + j * TileSize.x,
                                      startY);
            Transform tmp = CreateGameObject(CubePrefab, "Basic Wall Tile", wonderWall);
            tmp.position = new Vector3(pos.x, halfTileSize.y, pos.y);
            tmp.localScale = TileSize;
        }

        //right >>>
        startX = floor.position.x + floor.localScale.x * 0.5f - halfTileSize.x;
        startY = floor.position.y - floor.localScale.z * 0.5f + halfTileSize.y;

        wonderWall = wallParent.Find("RightWall");
        if (!wonderWall)
            wonderWall = CreateGameObject(null, "RightWall", wallParent);

        for (int j = 1; j < cubesInY - 1; j++)
        {
            Vector2 pos = new Vector2(startX,
                                      startY + j * TileSize.z);
            Transform tmp = CreateGameObject(CubePrefab, "Basic Wall Tile", wonderWall);
            tmp.position = new Vector3(pos.x, halfTileSize.y, pos.y);
            tmp.localScale = TileSize;
        }
    }

    void DestroyLevel()
    {
        List<GameObject> rootGo = SceneManager.GetActiveScene().GetRootGameObjects().ToList();

        GameObject lvl = rootGo.Find(x => x.name.ToUpper() == "LEVELDESIGN");
#if UNITY_EDITOR
        if (!Application.isPlaying)
            DestroyImmediate(lvl);
        else
#endif
        {
            Destroy(lvl);
        }
    }

    void CreateTile()
    {
        ResolveLevelRoot();

        if (!CubePrefab)
        {
            Debug.LogError("[LevelBuilder] No prefab to spawn");
            return;
        }

        Transform tile = CreateGameObject(CubePrefab, "Basic Tile", lvlParent);
        tile.localScale = TileSize;
    }

    Transform CreateGameObject(GameObject prefab = null, string name = "", Transform parent = null)
    {
        GameObject rt = null;

        if (prefab == null)
            rt = new GameObject();
        else
            rt = Instantiate(prefab, parent);

        rt.name = name;
        rt.transform.parent = parent;

        return rt.transform;
    }

    void ResolveLevelRoot()
    {
        List<GameObject> rootGo = SceneManager.GetActiveScene().GetRootGameObjects().ToList();

        GameObject lvl = rootGo.Find(x => x.name.ToUpper() == "LEVELDESIGN");
        if (!lvl)
        {
            lvl = new GameObject();
            lvl.name = "LevelDesign";
        }

        lvlParent = lvl.transform;
    }
}