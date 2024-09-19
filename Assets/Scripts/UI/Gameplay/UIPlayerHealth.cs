using UnityEngine;
using UnityAtoms.BaseAtoms;

namespace UI
{
    public class UIPlayerHealth : MonoBehaviour
    {
        [Header("UnityAtoms")]
        [SerializeField]
        private IntVariable m_currentHealth;
        [SerializeField] 
        private IntConstant m_startingHealth;
        [SerializeField] 
        private IntConstant m_absoluteMaxHealth;

        private void Start()
        {
            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.SetActive(false);
        }

        public void OnHealthChanged()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (m_currentHealth.Value > i)
                    transform.GetChild(i).gameObject.SetActive(true);
            }
        }
    }
}