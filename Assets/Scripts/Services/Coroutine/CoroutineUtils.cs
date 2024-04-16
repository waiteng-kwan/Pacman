using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Utils
{
    /// <summary>
    /// Helper class for coroutine services
    /// </summary>
    public static class CoroutineUtils 
    { 
        public static void TimerDelayed(float time)
        {

        }

        /// <summary>
        /// Timer to call event using UNSCALED time (this means it's unaffected by time scale!)
        /// Use this in StartCoroutine()
        /// </summary>
        /// <param name="uEvent">Callback event</param>
        /// <param name="time">Amount of seconds to delay</param>
        /// <returns></returns>
        static public IEnumerator TimerEventUnscaled(UnityAction uEvent, float time)
        {
            yield return new WaitForSecondsRealtime(time);
            uEvent?.Invoke();
        }

        /// <summary>
        /// Timer to call event using SCALED time (this means it's affected by time scale!)
        /// Use this in StartCoroutine()
        /// </summary>
        /// <param name="uEvent">Callback event</param>
        /// <param name="time">Amount of seconds to delay</param>
        /// <returns></returns>
        static public IEnumerator TimerEventScaled(UnityAction uEvent, float time)
        {
            yield return new WaitForSeconds(time);
            uEvent?.Invoke();
        }
    } 
}