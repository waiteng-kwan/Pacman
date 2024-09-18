using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using AYellowpaper.SerializedCollections;

namespace Client.UI
{
    public class UIDropdown : MonoBehaviour
    {
        [SerializeField]
        private TMP_Dropdown m_dropdownCmp;

        [Header("Header")]
        [SerializeField]
        private TextMeshProUGUI m_dropdownTitleCmp;
        [SerializeField]
        private string m_titleStr;
        public string TitleLabel => m_titleStr;

        [Header("Options")]
        [SerializeField]
        private SerializedDictionary<string, object> m_optionDict = new();

        private void OnValidate()
        {
            m_dropdownTitleCmp ??= transform.Find("Txt_Header")?.GetComponent<TextMeshProUGUI>();

            m_dropdownCmp ??= transform.Find("Dropdown")?.GetComponent<TMP_Dropdown>();

            if (m_dropdownTitleCmp)
                m_dropdownTitleCmp.text = m_titleStr;
        }
    }
}