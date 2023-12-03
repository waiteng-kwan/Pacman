using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class SceneManager : MonoBehaviour, IManager
    {
        public static SceneManager CreateInstance()
        {
            return new SceneManager();
        }

        public Type GetManagerType()
        {
            return typeof(SceneManager);
        }

        public void RegisterManager(GameManager mgr)
        {
            
        }

        public IManager RetrieveInterface()
        {
            return this;
        }

        public void ShutdownManager(GameManager mgr)
        {
            
        }

        private void FirstSceneSwitch()
        {

        }
    }
}