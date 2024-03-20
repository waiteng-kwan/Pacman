using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Service.Timer
{
    public class TimerService : ITimerService, IService
    {
        private List<ITimerHandle> m_timers = new List<ITimerHandle>();
        private bool m_isInitialized = false;
        private Coroutine m_updateCr = null;

        #region IService
        public void Initialize()
        {


            m_isInitialized = true;
        }

        public void RegisterServiceType(ServiceType serviceType, IService svc)
        {
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }
        #endregion

        IEnumerator Update()
        {
            for(int i = m_timers.Count - 1; i >= 0; i--)
            {
                if (m_timers[i] == null || m_timers[i].IsFinished())
                {
                    m_timers.RemoveAt(i);
                }
            }

            yield break;
        }

        #region TimerService
        public ITimerHandle CreateTimer(double duration, Action callback = null, bool isLooping = false)
        {
            if(!m_isInitialized)
            {
                Initialize();
            }

            ITimerHandle rt = new TimerHandle(duration, callback, isLooping);

            m_timers.Add(rt);

            return rt;
        }

        public void DisposeTimer(ITimerHandle timer)
        {
            if (timer == null)
                return;

            timer.Stop();
        }

        #endregion
    }
}