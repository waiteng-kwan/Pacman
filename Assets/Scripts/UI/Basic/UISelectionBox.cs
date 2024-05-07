using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using NaughtyAttributes;
using UnityEngine.UI;

namespace Client.UI
{
    public class UISelectionBox : MonoBehaviour
    {
        [Header("Controls")]
        [SerializeField]
        private UIButton m_leftArrow;
        [SerializeField]
        private UIButton m_rightArrow;

        [Header("Content")]
        [SerializeField]
        private Image m_bg;
        [SerializeField]
        private TextMeshProUGUI m_labelObj;
        [field: SerializeField]
        public string Label { get; private set; } = "Label";

        [Header("Options")]
        [SerializeField]
        public List<string> Options = new List<string>();
        [HideInInspector]
        public UnityEvent<int> OnOptionChanged = new();
        [field: SerializeField, ReadOnly]
        public int OptionIndex { get; private set; } = 0;

        //interactable
        [Header("Interactivity")]
        [SerializeField]
        private bool m_isInteractable;
        public bool IsInteractable 
        { 
            get => m_isInteractable; 
            set => SetInteractable(value); 
        }

        private void OnValidate()
        {
            if (!m_leftArrow)
                m_leftArrow = transform.Find("Img_LeftArrow")?.GetComponent<UIButton>();

            if (!m_rightArrow)
                m_rightArrow = transform.Find("Img_RightArrow")?.GetComponent<UIButton>();

            if (!m_labelObj)
                m_labelObj = transform.Find("Img_Background")?.GetComponentInChildren<TextMeshProUGUI>();

            if (m_labelObj)
                m_labelObj.text = Label;
        }

        private void Awake()
        {
            SetInteractable(IsInteractable);
        }

        private void OnLeftArrowClicked()
        {
            if (--OptionIndex < 0)
                OptionIndex = Options.Count - 1;

            OnSelectionChanged();
        }

        private void OnRightArrowClicked()
        {
            if (++OptionIndex >= Options.Count)
                OptionIndex = 0;

            OnSelectionChanged();
        }

        private void OnSelectionChanged()
        {
            OnOptionChanged?.Invoke(OptionIndex);
        }

        private void SetInteractable(bool value)
        {
            m_isInteractable = value;

            m_leftArrow.IsInteractable = value;
            m_rightArrow.IsInteractable = value;

            if (m_isInteractable)
                m_bg.color = new Color(.17f, .17f, .17f, 1f);
            else
                m_bg.color = new Color(.15f, .15f, .15f, .5f);
        }
    }
}