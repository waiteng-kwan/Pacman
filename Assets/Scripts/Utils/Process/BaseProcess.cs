using System;
using System.Collections;
using UnityEngine;

namespace Utils
{
    public class BaseProcess : MonoBehaviour
    {
        public Action OnDone = null;

        public BaseProcess(Action onDone = null)
        {
            OnDone = onDone;
        }

        protected void Execute()
        {
            StartCoroutine(Process());
        }

        protected virtual IEnumerator Process()
        {
            OnDone?.Invoke();
            yield return null;
        }
    }
}