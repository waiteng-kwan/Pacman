using UnityEngine;
using UnityAtoms.BaseAtoms;
using TMPro;

namespace UI
{
    public class UIPlayerName : MonoBehaviour
    {
        [Header("UnityAtoms")]
        [SerializeField]
        private StringVariable m_playerDisplayName;
        [SerializeField]
        private IntVariable m_playerIndex;


        [SerializeField]
        private TextMeshProUGUI m_idTxt;
        [SerializeField]
        private TextMeshProUGUI m_nameTxt;

        private void OnValidate()
        {
            m_idTxt ??= transform.Find("PlayerId")?.GetComponent<TextMeshProUGUI>();
            m_nameTxt ??= transform.Find("PlayerName")?.GetComponent<TextMeshProUGUI>();
        }
        // Start is called before the first frame update
        void Start()
        {
            m_idTxt ??= transform.Find("PlayerId")?.GetComponent<TextMeshProUGUI>();
            m_nameTxt ??= transform.Find("PlayerName")?.GetComponent<TextMeshProUGUI>();
        }

        public void OnPlayerNameChanged()
        {
            m_nameTxt.text = m_playerDisplayName.Value;
        }

        public void OnPlayerIdChanged()
        {
            m_idTxt.text = "#" + m_playerIndex.Value.ToString();
        }
    }
}