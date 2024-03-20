using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManagerState
{
    public Queue<CustomSceneData> ActiveSceneQueue = new Queue<CustomSceneData>();
    public Queue<CustomSceneData> NextToLoadSceneQueue = new Queue<CustomSceneData>();
    public Queue<CustomSceneData> NextToUnloadSceneQueue = new Queue<CustomSceneData>();

    public MasterSceneList MasterSceneList { get; private set; }

    //this is special
    public CustomSceneData LoadingScene { get; private set; }

    public void Initialize()
    {
    }

    public void SetSceneListRef(MasterSceneList masterSceneList)
    {
        MasterSceneList = masterSceneList;

        LoadingScene = masterSceneList.Find("LoadingUI");
    }

    public void AddSceneToUnload(CustomSceneData scene)
    {
        NextToUnloadSceneQueue.Enqueue(scene);
    }

    public CustomSceneData PopSceneToUnload()
    {
        return NextToUnloadSceneQueue.Dequeue();
    }

    public void AddSceneToLoad(CustomSceneData scene)
    {
        NextToLoadSceneQueue.Enqueue(scene);
    }

    public CustomSceneData PopSceneToLoad()
    {
        return NextToLoadSceneQueue.Dequeue();
    }

    public void AddActiveScenes(CustomSceneData scene)
    {
        ActiveSceneQueue.Enqueue(scene);
    }

    public void PopActiveScenes()
    {
        ActiveSceneQueue.Dequeue();
    }
}
