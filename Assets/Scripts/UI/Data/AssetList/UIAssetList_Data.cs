using UnityEngine;

namespace Client.UI
{
    [CreateAssetMenu(fileName = "UIAssetList_Data",
    menuName = "Scriptable Objects/UI/AssetList Data", order = 1)]
    public class UIAssetList_Data : ScriptableObject
    {
        [Header("General Dialog Box")]
        public UIDialog m_smallDialogBox;
        public UIDialog m_mediumDialogBox;
        public UIDialog m_largeDialogBox;

        [Header("General Selection Box")]
        public UISelectionBox m_selectionBox;

        [Header("General Button")]
        public UIButton m_normalBtn;
        public UIButton m_GreenBtn;
        public UIButton m_redBtn;

        [Header("General Dropdown")]
        public UIDropdown m_dropdown1;
    }
}