using Game;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour, IManager
{
    [SerializeField, ReadOnly, Expandable]
    private MasterDataList m_masterDataList;

    public static DataManager CreateInstance(GameObject attachTo)
    {
        return attachTo.AddComponent<DataManager>();
    }

    public void RegisterManager(GameManager mgr)
    {
        m_masterDataList = Resources.Load("Data/MasterDataList", typeof(MasterDataList)) as MasterDataList;
    }

    public void ShutdownManager(GameManager mgr)
    {
        Resources.UnloadAsset(m_masterDataList);
    }

    public IManager RetrieveInterface()
    {
        return this;
    }

    Type GetManagerType()
    {
        return typeof(DataManager);
    }

    private void Awake()
    {
        
    }
    private void Start()
    {
        
    }
}