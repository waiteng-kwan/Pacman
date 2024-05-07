using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Client.UI
{
    public class UIButton : MonoBehaviour
    {
        public Button Button { get; private set; }
        public TextMeshProUGUI TMProLabelObj { get; private set; }

        [field: SerializeField]
        public string TextLabel { get; private set; } = "Label";
        [field: SerializeField]
        public bool IsLocked { get; private set; } = false;

        [Header("Icon if any")]
        [SerializeField]
        private Image m_iconImg;

        [Header("Event Type")]
        public UICommand Events;

        public bool IsInteractable
        {
            get { return Button.interactable; }
            set { SetInteractable(value); }
        }

        Coroutine m_disableTimekeepCr = null;

        private void OnValidate()
        {
            Button = GetComponent<Button>();
            TMProLabelObj = GetComponentInChildren<TextMeshProUGUI>();

            if(TMProLabelObj)
                TMProLabelObj.text = TextLabel;

            if (m_iconImg)
                m_iconImg.gameObject.SetActive(IsLocked);
        }

        private void Awake()
        {
            Button = GetComponent<Button>();
            TMProLabelObj = GetComponentInChildren<TextMeshProUGUI>();

            if (TMProLabelObj)
                TMProLabelObj.text = TextLabel;

            Animator a = Button.GetComponent<Animator>();

            if (a != null)
                a.SetBool("Locked", IsLocked);

            if (m_iconImg)
                m_iconImg.gameObject.SetActive(IsLocked);

            if (!IsLocked)
            {
                Button.interactable = IsInteractable;

                Button.onClick.AddListener(OnClick);
            }
            else
            {
                Button.interactable = false;

                if (a)
                    a.Play("Locked");
            }
        }

        private void OnDestroy()
        {
            Button.onClick.RemoveAllListeners();
        }

        private void OnClick()
        {
            if(m_disableTimekeepCr != null)
                StopCoroutine(m_disableTimekeepCr);

            m_disableTimekeepCr = StartCoroutine(DisableInteractionFor(UIConsts.GlobalButtonCooldownTime));

            Events.Execute();
        }

        private void SetInteractable(bool value)
        {
            Button.interactable = value;

            //do other stuff here
        }

        IEnumerator DisableInteractionFor(float seconds = .15f)
        {
            IsInteractable = false;
            yield return new WaitForSeconds(seconds);
            IsInteractable = true;
        }

        public void SetLabelText(string text)
        {
            TextLabel = text;
            TMProLabelObj.text = text;
        }
    }
}