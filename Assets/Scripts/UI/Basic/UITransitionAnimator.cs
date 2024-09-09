using Core;
using NaughtyAttributes;
using System.Collections;
using UnityEngine;

namespace Client.UI
{
    public class UITransitionAnimator : UITransition
    {
        private const string m_animationEntryName = "Entry";
        private const string m_animationExitName = "Exit";

        [SerializeField, ReadOnly]
        private string m_animationName;

        private Animator m_animator;

        private Coroutine m_transitionCr;

        private void OnValidate()
        {
            m_animator ??= GetComponent<Animator>();
        }

        private void Awake()
        {
            m_animator ??= GetComponent<Animator>();
        }

        protected override void DoTransition()
        {
            base.DoTransition();

            m_animator ??= GetComponent<Animator>();
            //m_animator.runtimeAnimatorController = GameManager.GetManager<DataManager>().MasterDataList;

            m_animationName = IsTransitOut ? m_animationExitName : m_animationEntryName;

            //fail safe
            if (!m_animator)
                m_animator = GetComponent<Animator>();

            m_animator.ResetTrigger(m_animationName);

            if (m_transitionCr != null)
                StopCoroutine(m_transitionCr);

            m_transitionCr = StartCoroutine(DoTransitionCr(IsTransitOut));
        }

        bool AnimatorIsPlaying()
        {
            return m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.99f;
        }

        IEnumerator DoTransitionCr(bool isIn)
        {
            if (isIn)
            {
                TransitionEvent.OnRaised?.Invoke();
            }
            else
            {
                TransitionEvent.OnClosed?.Invoke();
            }

            yield return DoTransitionCrInner(m_animationName);

            OnTransitionDone();

            yield break;
        }

        private IEnumerator DoTransitionCrInner(string animName)
        {
            var stateId = Animator.StringToHash(animName);
            var hasState = m_animator.HasState(0, stateId);
            if (!hasState)
            {
                Debug.LogError($"no such state name {animName}");
                yield break;
            }
            m_animator.Play(stateId, 0, 0);

            //wait 1 frame to start
            yield return null;

            //wait for animation to finish
            while (AnimatorIsPlaying())
                yield return null;
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