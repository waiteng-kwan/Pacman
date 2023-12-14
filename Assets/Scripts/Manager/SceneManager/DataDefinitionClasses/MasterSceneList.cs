using Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CustomSceneData;

[CreateAssetMenu(fileName = "MasterSceneList",
menuName = "Scriptable Objects/MasterSceneList Data", order = 1)]
public class MasterSceneList : ScriptableObject
{
    public List<CustomSceneData> SceneList = new List<CustomSceneData>();

    private void OnValidate()
    {
        foreach(CustomSceneData data in SceneList)
        {
            var item = data;
            item.SceneName = data.SceneObj.Name;
        }
    }

    public bool IsExists(string name)
    {
        //return SceneList.Exists(x => x.SceneName.Equals(name));
        return SceneList.Exists(x => x.SceneObj.Name.Equals(name));
    }

    public CustomSceneData Find(string name)
    {
        //return SceneList.Find(x => x.SceneName.Equals(name));
        return SceneList.Find(x => x.SceneObj.Name.Equals(name));
    }


    public List<CustomSceneData> Find(GameInstanceStates state)
    {
        List<CustomSceneData> rt = new List<CustomSceneData>();

        foreach(CustomSceneData data in SceneList)
        {
            if(data.BoundToGameState == state)
                rt.Add(data);
        }

        return rt;
    }
    public List<CustomSceneData> Find(SceneType type)
    {
        List<CustomSceneData> rt = new List<CustomSceneData>();

        foreach(CustomSceneData data in SceneList)
        {
            if(data.TypeOfScene == type)
                rt.Add(data);
        }

        return rt;
    }
}