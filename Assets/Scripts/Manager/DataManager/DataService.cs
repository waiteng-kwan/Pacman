using Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataService : IDataService
{
    private DataManager m_dataManager;

    public MasterDataList GetMasterDataList()
    {
        m_dataManager = GameManager.Instance.GetManager<DataManager>(ManagerType.Data);

        return m_dataManager.MasterDataList;
    }
}
