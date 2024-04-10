using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace Client.UI
{
    public class UITransitionEvent
    {
        public UnityEvent OnRaised = new();
        public UnityEvent OnRaisedFinished = new();
        public UnityEvent OnClosed = new();
        public UnityEvent OnClosedFinished = new();
    }

    public class UITransition : MonoBehaviour
    {
        public UITransitionEvent TransitionEvent = new();

        [Header("Transition")]
        public bool IsTransitOut = false;
        public bool IsActiveAfterTransitOut = false;
        
        [Button]
        public void DoTransitionIn()
        {
            IsTransitOut = false;
            BeginTransitionIn();
        }

        public void DoTransitionOut()
        {
            IsTransitOut = true;
            BeginTransitionIn();
        }

        protected virtual void BeginTransitionIn()
        {
            gameObject.SetActive(true);
            TransitionEvent.OnRaised?.Invoke();
            DoTransition();
        }

        protected virtual void DoTransition()
        {

        }

        protected virtual void OnTransitionDone()
        {
            if (IsTransitOut)
            {
                TransitionEvent.OnClosedFinished?.Invoke();
                gameObject.SetActive(IsActiveAfterTransitOut);
            }
            else
                TransitionEvent.OnRaisedFinished?.Invoke();
        }
    }
}
