using Service;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Service.CoroutineSvc
{
    public class CoroutineService : ICoroutineService, IService
    {
        private List<Coroutine> m_coroutines = new List<Coroutine>();

        public void Initialize()
        {
            throw new System.NotImplementedException();
        }

        public void RegisterServiceType(ServiceType serviceType, IService svc)
        {
            throw new System.NotImplementedException();
        }

        public void Shutdown()
        {
            throw new System.NotImplementedException();
        }

        IEnumerator Update()
        {

            yield break;
        }

        public void StartCoroutine()
        {
            throw new System.NotImplementedException();
        }

        public void StartCoroutine(IEnumerator cr)
        {
            throw new System.NotImplementedException();
        }

        public void StopCoroutine(ICoroutineHandle cr)
        {
            throw new System.NotImplementedException();
        }
    }
}