using AYellowpaper.SerializedCollections;
using UnityEngine;
using static PacmanBaseData;

namespace Utils
{
    [CreateAssetMenu(fileName = "StringToMaterialColorSO",
    menuName = "Scriptable Objects/PacmanColorToMaterialColor Data", order = 1)]
    public class PacmanColorToMaterialColorSO : ScriptableObject
    {
        [SerializeField]
        private Material m_baseMat;
        public Material Material => m_baseMat;

        [SerializedDictionary("Pacman Type, Color")]
        public SerializedDictionary<PacmanType, Color> m_colors = new();

        public Material GetColorInstance(PacmanType type)
        {
            Material m = new Material(m_baseMat);

            if (m_colors.TryGetValue(type, out var c))
            {
                m.SetColor("_BaseColor", c);
                m.SetColor("_EmissionColor", c);
            }

            return m;
        }
    }
}