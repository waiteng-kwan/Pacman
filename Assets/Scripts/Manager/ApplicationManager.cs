using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Common
{
    /// <summary>
    /// This is the highest level manager, higher than even game manager.
    /// It controls application flow such as on Entry and Exit, Pause, etc.
    /// </summary>
    public class ApplicationManager : MonoBehaviour
    {
        private void Awake()
        {
            
        }

        private void OnApplicationFocus(bool focus)
        {
            
        }

        private void OnApplicationPause(bool pause)
        {
            
        }

        private void OnApplicationQuit()
        {
            
        }
    }
}