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
        public TextMeshProUGUI Label { get; private set; }

        [field: SerializeField]
        public string TextLabel { get; private set; } = "Label";

        public bool IsInteractable
        {
            get { return Button.interactable; }
            set { SetInteractable(value); }
        }

        Coroutine m_disableTimekeepCr = null;

        private void OnValidate()
        {
            Button = GetComponent<Button>();
            Label = GetComponentInChildren<TextMeshProUGUI>();
            Label.text = TextLabel;
        }

        private void Awake()
        {
            Label.text = TextLabel;
            Button.interactable = IsInteractable;

            Button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            Button.onClick.RemoveAllListeners();
        }

        private void OnClick()
        {
            if(m_disableTimekeepCr != null)
                StopCoroutine(m_disableTimekeepCr);

            m_disableTimekeepCr = StartCoroutine(DisableInteractionFor(1f));
        }

        private void SetInteractable(bool value)
        {
            Button.interactable = value;

            //do other stuff here
        }

        IEnumerator DisableInteractionFor(float seconds)
        {
            IsInteractable = false;
            yield return new WaitForSeconds(seconds);
            IsInteractable = true;
        }
    }
}