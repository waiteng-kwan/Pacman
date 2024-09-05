using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Service
{
    public enum ServiceType
    {
        Camera,
        UI,
        Audio,
        Timer,
        Log,
        FX,
        Coroutine,
        Input,
        Networking,
    }

    public class Services : IService
    {
        private static Services m_instance;
        public static Services Instance => m_instance;

        private Dictionary<ServiceType, IService> m_typeToServiceDict = new Dictionary<ServiceType, IService>();

        public void Initialize()
        {
            if (m_instance != null)
                return;

            m_instance = this;
        }

        public void Shutdown()
        {
            foreach(var service in m_typeToServiceDict.Values)
            {
                service.Shutdown();
            }

            m_typeToServiceDict.Clear();
        }

        public void RegisterServiceType(ServiceType serviceType, IService svc)
        {
            if(!m_typeToServiceDict.ContainsKey(serviceType))
                m_typeToServiceDict.Add(serviceType, svc);
            else
                Debug.LogError($"{serviceType.ToString()} already exists!");
        }

        public static IService GetService(ServiceType serviceType)
        {
            if (m_instance.m_typeToServiceDict.TryGetValue(serviceType, out IService svc))
            {
                return svc;
            }

            Debug.LogError("Requested service of type " + serviceType + " not found!");
            return default;
        }
    }
}