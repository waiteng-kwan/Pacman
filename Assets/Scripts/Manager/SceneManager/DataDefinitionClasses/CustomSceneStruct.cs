using Eflatun.SceneReference;
using Game;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct CustomSceneData
{
    public enum SceneType
    {
        UI,
        Game,
        Environment,
        Loading,
        Init
    }

    public SceneType TypeOfScene;
    public SceneReference SceneObj;
    public string SceneName;
    public LoadSceneMode LoadSceneMode;

    [Header("Optional")]
    public GameInstanceStates BoundToGameState;
    public bool IsActiveSceneWhenLoaded;
}