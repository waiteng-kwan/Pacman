using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameBoardInstance : MonoBehaviour, IGameBoard
{
    [Header("Level Stuff")]
    [SerializeField] 
    private Collider[] m_ghostRespawnZones;
    public Transform[] m_playerSpawnPoints;

    private Transform m_levelParent;
    private Transform m_pelletParent;

    [Header("Set Up")]
    [SerializeField] private Vector3 m_offset;
    [SerializeField] private Collider m_volumeToSpawnPelletsIn;
    [SerializeField] private Collider[] m_volumesToNotSpawnPelletsIn;
    private Transform m_setupParentObj;

    //temp af
    public BasicDot SpawnPelletPrefab;
    public bool GenerateInXZAxisOnly = true;

    [Space, Header("Gameplay")]
    [ReadOnly, SerializeField]
    private Dictionary<DotData.DotType, int> m_typeToNumberDict =
        new Dictionary<DotData.DotType, int>();

    public int RemainingTotalPellets => GetRemainingAllPellets();

    private void OnValidate()
    {
        m_setupParentObj = transform.Find("Setup");
        m_levelParent = transform.Find("GameBoardLevel");
        m_pelletParent = transform.Find("GameBoardLevel/DotPellets");

        m_volumeToSpawnPelletsIn = transform.Find("Setup/SpawnVolumes").GetComponentInChildren<Collider>();
        m_volumesToNotSpawnPelletsIn = transform.Find("Setup/ExcludeSpawnVolumes").GetComponentsInChildren<Collider>();
        m_ghostRespawnZones = m_levelParent.Find("GhostRespawnZones").GetComponentsInChildren<Collider>();

        m_playerSpawnPoints = Utils.GameObjectUtils.GetChildren<Transform>(m_levelParent.Find("PlayerSpawnPoints"));
    }

    private void Awake()
    {
        if (m_setupParentObj != null)
        {
            Destroy(m_setupParentObj.gameObject);

            //remove the colliders that were there for easy to see
            foreach(Transform t in m_playerSpawnPoints)
            {
                Destroy(t.GetComponent<Collider>());
            }
        }
    }

    private void Start()
    {
        foreach (var p in m_pelletParent.GetComponentsInChildren<BasicDot>())
        {
            //if exists already
            if (m_typeToNumberDict.TryGetValue(p.DotPelletType, out int val))
            {
                m_typeToNumberDict[p.DotPelletType]++;
            }
            else
                m_typeToNumberDict.Add(p.DotPelletType, 0);

            p.EOnDestroy.AddListener(OnDotPelletCollected);
        }
    }

    #region Game Mode Helpers
    /// <summary>
    /// Currently the only condition for game ending via game board
    /// is when the normal pellets are all eaten.
    /// </summary>
    /// <returns></returns>
    public bool CanGameEnd()
    {
        return m_typeToNumberDict[DotData.DotType.Normal] <= 0;
    }
    #endregion

    #region Dot/Pellets
    void OnDotPelletCollected(DotData.DotType type)
    {
        m_typeToNumberDict[type]--;
    }
    #endregion

    #region Re/Spawn
    #region Ghost
    /// <summary>
    /// Get ghost respawn zone from the list by specified index or random.
    /// Returns first instance if not specified.
    /// </summary>
    /// <param name="ind">Index to get</param>
    /// <param name="rand">Should the respawn zone be random?</param>
    /// <returns></returns>
    public Collider GetGhostSpawnZone(int ind = 0, bool rand = false)
    {
        Collider rt = null;
        if (rand)
        {
            //the int version of random range is max exclusive
            rt = m_ghostRespawnZones[Random.Range(0, m_ghostRespawnZones.Length)];
        }
        else
        {
            if (ind >= 0 && ind < m_ghostRespawnZones.Length)
            {
                rt = m_ghostRespawnZones[ind];
            }
        }

        return rt;
    }

    /// <summary>
    /// Gets a random point in a ghost respawn zone from the list by specified index. If rand = true, the ghost respawn zone will be randomly selected from the available list.
    /// Returns first instance if not specified.
    /// </summary>
    /// <param name="ind">index of zone in list to get</param>
    /// <param name="rand">Is respawn zone randomly selected?</param>
    /// <param name="includeYAxis">Maintain same Y axis or not</param>
    /// <returns></returns>
    public Vector3 GetRandomPointInGhostSpawnZone(int ind = 0, bool rand = false, bool includeYAxis = false)
    {
        Vector3 rt = Vector3.zero;
        Collider col = GetGhostSpawnZone(ind, rand);

        //get position
        rt.x = Random.Range(col.bounds.min.x, col.bounds.max.x);
        rt.z = Random.Range(col.bounds.min.z, col.bounds.max.z);
        rt.y = includeYAxis ? Random.Range(col.bounds.min.y, col.bounds.max.y) :
            col.transform.position.y;

        return rt;
    }

    public (Vector3 pt, Collider zone) GetRandomPointAndGhostSpawnZone(int ind = 0, bool rand = false, bool includeYAxis = false)
    {
        Vector3 rt = Vector3.zero;
        Collider col = GetGhostSpawnZone(ind, rand);

        //get position
        rt.x = Random.Range(col.bounds.min.x, col.bounds.max.x);
        rt.z = Random.Range(col.bounds.min.z, col.bounds.max.z);
        rt.y = includeYAxis ? Random.Range(col.bounds.min.y, col.bounds.max.y) :
            col.transform.position.y;

        return (rt, col);
    }
    #endregion

    #region Player
    public Vector3 GetPlayerSpawnPoint(bool rand = false)
    {
        return m_playerSpawnPoints[rand ? 0 : Random.Range(0, m_playerSpawnPoints.Length)].position;
    }
    #endregion
    #endregion

    #region Getter
    private int GetRemainingAllPellets()
    {
        int total = 0;
        foreach (var elem in m_typeToNumberDict.Values)
        {
            total += elem;
        }

        return total;
    }

    public int GetRemainingPellets(DotData.DotType type)
    {
        return m_typeToNumberDict[type];
    }
    public int GetMaxScorePellets()
    {
        return m_typeToNumberDict[DotData.DotType.Normal];
    }
    #endregion

    #region Editor Stuff
    [Button]
    private void GenerateBoard()
    {
        DeleteBoard();

        Vector3 min = m_volumeToSpawnPelletsIn.bounds.min;
        Vector3 max = m_volumeToSpawnPelletsIn.bounds.max;
        Vector3 pos = Vector3.zero;

        for (int i = 0, j = 0; ;)
        {
            //horizontal axis
            pos.x = min.x + m_offset.x * i;
            i++;

            //vertical axis
            pos.z = min.z + m_offset.z * j;

            if (pos.x > max.x)
            {
                j++;
                i = 0;  //reset horizontal axis
                continue;
            }

            //the up down 3rd axis
            pos.y = GenerateInXZAxisOnly ?
                m_volumeToSpawnPelletsIn.bounds.center.y :
                min.y + m_offset.y * i;

            //if beyond the boundaries
            //only z for first check cos x alr handled by above when resetting axis
            if (GenerateInXZAxisOnly)
            {
                if (pos.z > max.z)
                    break;
            }
            else
                if (pos.z > max.z && pos.y > max.y) //this is for 3d
                    break;

            //check if in exclude zone
            bool flag = false;
            for (int k = 0; k < m_volumesToNotSpawnPelletsIn.Length; k++)
            {
                //sort by distance first, no point if greater than extents
                if ((pos - m_volumesToNotSpawnPelletsIn[k].transform.position).sqrMagnitude <= m_volumesToNotSpawnPelletsIn[k].bounds.extents.sqrMagnitude)
                {
                    //additional check to see if in bounds
                    var volToSkipBounds = m_volumesToNotSpawnPelletsIn[k].bounds;
                    Vector3 skipMin = volToSkipBounds.min;
                    Vector3 skipMax = volToSkipBounds.max;

                    //if greater than min x and smaller than max x, aka inside
                    if (pos.x >= skipMin.x  &&
                        pos.x <= skipMax.x)
                    {
                        //then check if in z value
                        if (pos.z >= skipMin.z &&
                        pos.z <= skipMax.z)
                        {
                            if(!GenerateInXZAxisOnly)
                            {
                                //then check if not in y value
                                if (pos.y < skipMin.y &&
                                pos.y > skipMax.y)
                                    continue;
                            }

                            flag = true;
                            break;
                        }
                    }

                    
                }
            }
            //if in exclude zone then do not spawn
            if (flag)
                continue;

            //isntantiate at position
            Instantiate(SpawnPelletPrefab, pos, Quaternion.identity, m_pelletParent);
        }
    }

    [Button]
    private void DeleteBoard()
    {
        //destroy all pellets first
        for (int i = m_pelletParent.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                Destroy(m_pelletParent.GetChild(i).gameObject);
            }
            else
            {
                DestroyImmediate(m_pelletParent.GetChild(i).gameObject);
            }
#else
            Destroy(m_pelletParent.GetChild(i).gameObject);
#endif
        }
    }
    #endregion
}
