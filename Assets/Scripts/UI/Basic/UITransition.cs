using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace Client.UI
{
    public class UITransitionEvent
    {
        public UnityEvent OnRaised = new();
        public UnityEvent OnRaisedFinished = new();
        //public UnityEvent OnClosed = delegate { };
        //public UnityEvent OnClosedFinished = delegate { };

        public void BeginTransitionIn()
        {
            OnRaised?.Invoke();
        }

        public void TransitionDone()
        {
            OnRaisedFinished?.Invoke();
        }
    }

    public class UITransition : MonoBehaviour
    {
        public UITransitionEvent TransitionEvent = new();
        public bool IsTransitOut;
        
        [Button]
        public void DoTransitionIn()
        {
            BeginTransitionIn();
        }

        public void DoTransitionOut()
        {

        }

        protected virtual void BeginTransitionIn()
        {
            TransitionEvent.BeginTransitionIn();
            DoTransition();
        }

        protected virtual void DoTransition()
        {

        }

        protected virtual void OnTransitionDone()
        {
            TransitionEvent.TransitionDone();
        }
    }
}
