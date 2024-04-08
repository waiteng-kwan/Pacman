using Client.UI;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Client.UI
{
    public class UITransitionAnimator : UITransition
    {
        public enum AnimationType
        {
            Fade,
            Ease,
            Blink,
            Custom
        }

        private Animator m_animator;
        public AnimationType TransitionAnimationType;

        private void OnValidate()
        {
            if (!m_animator)
                m_animator = GetComponent<Animator>();
        }

        protected override void DoTransition()
        {
            base.DoTransition();

            string append = IsTransitOut ? "Out" : "In";

            m_animator.ResetTrigger(TransitionAnimationType.ToString() + append);
            m_animator.SetTrigger(TransitionAnimationType.ToString() + append);
        }

        bool AnimatorIsPlaying()
        {
            return m_animator.GetCurrentAnimatorStateInfo(0).length >
               m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        }

        bool AnimatorIsPlaying(string stateName)
        {
            return AnimatorIsPlaying() && m_animator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
        }

        IEnumerator WaitForAnimation()
        {
            while (AnimatorIsPlaying())
            {
                yield return new WaitForSeconds(.2f);
            }
        }
    }
}

/*
using MonoFramework;
using System.Collections;
using UnityEngine;

namespace Client
{
    [RequireComponent(typeof(Animator))]
    public class UITransitionAnimatorBehaviour : MonoBehaviour, IUITransitionBehaviour
    {
        [SerializeField] bool _uIServiceTriggersThis = true;
        [SerializeField] bool _forTransitIn = true;
        [SerializeField] bool _forTransitOut = true;
        [SerializeField] Animator _animator = null;
        [SerializeField] string _transitInStateName = "FadeIn";
        [SerializeField] string _transitOutStateName = "FadeOut";

        public bool UIServiceWillTrigger => _uIServiceTriggersThis;
        public bool IsTransitIn => _forTransitIn;
        public bool IsTransitOut => _forTransitOut;

        public event UITransitionEvent OnRaised = delegate { };
        public event UITransitionEvent OnRaisedFinished = delegate { };
        public event UITransitionEvent OnClosed = delegate { };
        public event UITransitionEvent OnClosedFinished = delegate { };

        private Coroutine _coroutine = null;

        public void DoTransition(bool isIn)
        {
            if (_coroutine != null)
                StopCoroutine(_coroutine);

            _coroutine = StartCoroutine(DoTransitionCr(isIn));
        }

        public void Init(bool visible, bool reInitOnly = false)
        {
            InitAnimatorRef();
        }

        private void OnValidate()
        {
            InitAnimatorRef();
        }
        private void OnEnable()
        {
            InitAnimatorRef();
        }
        private void InitAnimatorRef()
        {
            if (_animator == null)
                _animator = GetComponent<Animator>();
        }

        private IEnumerator DoTransitionCr(bool isIn)
        {
            string animName;

            if (isIn)
            {
                animName = _transitInStateName;
                OnRaised?.Invoke();
            }
            else
            {
                animName = _transitOutStateName;
                OnClosed?.Invoke();
            }

            yield return DoTransitionCrInner(animName);

            if (isIn)
                OnRaisedFinished?.Invoke();
            else
                OnClosedFinished?.Invoke();
        }

        private IEnumerator DoTransitionCrInner(string animName)
        {
            var stateId = Animator.StringToHash(animName);
            var hasState = _animator.HasState(0, stateId);
            if (!hasState)
            {
                Debug.LogError($"no such state name {animName}");
                yield break;
            }
            _animator.Play(stateId, 0, 0);

            yield return null;
            while (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.99f)
            {
                yield return null;
            }
        }
    }

}
*/