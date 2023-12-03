using Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Client
{
    public class AudioManager : IManager
    {
        public static AudioManager CreateInstance()
        {
            return new AudioManager();
        }

        public Type GetManagerType()
        {
            return typeof(AudioManager);
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
    }
}