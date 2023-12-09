using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Services
{
    public enum ServiceType
    {
        Camera,
        UI,
        Audio
    }

    public static Services Instance;

    private Dictionary<ServiceType, IService> m_typeToServiceDict = new Dictionary<ServiceType, IService>();

    public void Initialize()
    {

    }

    public void Shutdown()
    {
        m_typeToServiceDict.Clear();
    }

    public void RegisterServiceType(ServiceType serviceType, IService svc)
    {
        m_typeToServiceDict.Add(serviceType, svc);
    }
}
