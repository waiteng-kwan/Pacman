using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utils
{
    public static class GameObjectUtils 
    {
        public static T[] GetChildren<T>(Transform parent, bool recursive = false) where T : Component
        {
            List<T> rt = new List<T>();

            //this wont work if they are of component type, but technically it shouldnt happen anyway
            if (!typeof(T).IsSubclassOf(typeof(Component)))
            {
                Debug.Log("Type " + typeof(T) + " is not of UnityEngine.Component");
                return default;
            }

            //TO DO do recurisve next time

            for (int i = 0; i < parent.childCount; i++)
            {
                rt.Add(parent.GetChild(i).GetComponent<Component>() as T);
            }

            return rt.ToArray();
        }
    } 
}