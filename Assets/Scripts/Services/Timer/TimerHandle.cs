using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Service.Timer
{
    public class TimerHandle : ITimerHandle
    {
        public TimerHandle(double duration, 
            Action callback = null, bool isLooping = false)
        {

        }

        #region ITimerHandle
        public bool Enabled => throw new NotImplementedException();

        public bool IsPlaying()
        {
            throw new NotImplementedException();
        }
        public bool IsFinished()
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Play(double duration, Action callback = null, bool isLooping = false)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}