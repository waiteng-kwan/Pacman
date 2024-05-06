using NaughtyAttributes;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Client.UI
{
    public class UIDialog : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField]
        private TextMeshProUGUI m_header;
        [SerializeField]
        private TextMeshProUGUI m_body;

        [Header("Buttons")]
        [SerializeField]
        private UIButton m_cancelBtn;
        public UIButton CancelBtn => m_cancelBtn;
        [SerializeField]
        private UIButton m_okBtn;
        public UIButton OkBtn => m_okBtn;
        [SerializeField]
        private UIButton m_submitBtn;
        public UIButton SubmitBtn => m_submitBtn;

        [SerializeField, ReadOnly]
        private OpenDialog.EShowButtons eShowButtons;

        private UITransitionAnimator m_transition;

        private void OnValidate()
        {
            m_transition = GetComponent<UITransitionAnimator>();
        }
        private void Awake()
        {
            m_transition = GetComponent<UITransitionAnimator>();

            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }

        public void SetUpText(bool showHeader = true, string header = "", string body = "")
        {
            if (m_header)
                m_header.text = header;

            if (m_body)
                m_body.text = body;

            m_header.gameObject.SetActive(showHeader);
        }

        public void SetUpButtons(OpenDialog.EShowButtons btnToShow, string cancelTxt = "Cancel", string okTxt = "Ok", string submitTxt = "Submit")
        {
            if (m_cancelBtn)
                m_cancelBtn.SetLabelText(cancelTxt);

            if (m_okBtn)
                m_okBtn.SetLabelText(okTxt);

            if (m_submitBtn)
                m_submitBtn.SetLabelText(submitTxt);

            m_cancelBtn.gameObject.SetActive(btnToShow.HasFlag(OpenDialog.EShowButtons.Cancel));
            m_okBtn.gameObject.SetActive(btnToShow.HasFlag(OpenDialog.EShowButtons.Okay));
            m_submitBtn.gameObject.SetActive(btnToShow.HasFlag(OpenDialog.EShowButtons.Submit));
        }

        public void ShowDialog()
        {
            gameObject.SetActive(true);

            if (!m_transition)
                m_transition = GetComponent<UITransitionAnimator>();

            if (m_transition)
                m_transition.DoTransitionIn();
        }

        public void CloseDialog()
        {
            if (!m_transition)
                m_transition = GetComponent<UITransitionAnimator>();

            m_cancelBtn.IsInteractable = false;
            m_okBtn.IsInteractable = false;
            m_submitBtn.IsInteractable = false;

            if (m_transition)
                m_transition.DoTransitionOut();
        }
    }
}