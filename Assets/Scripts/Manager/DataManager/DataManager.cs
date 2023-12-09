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
    public MasterDataList MasterDataList => m_masterDataList;

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

    public Type GetManagerType()
    {
        return typeof(DataManager);
    }

    public void PreStateChange(GameInstanceStates currentState)
    {
        throw new NotImplementedException();
    }

    public void OnStateChange(GameInstanceStates prevState, GameInstanceStates currState, GameInstanceStates nextState)
    {
        throw new NotImplementedException();
    }

    public void PostStateChange(GameInstanceStates nextState)
    {
        throw new NotImplementedException();
    }
}
