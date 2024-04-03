using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Client.UI
{
    /// <summary>
    /// From https://www.youtube.com/watch?v=ZHU3AcyDKik
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class UITextJitter : MonoBehaviour
    {
        public Vector2 Jitter;

        [Range(0f, 1f)]
        public float RandomRange = 0.5f;

        private TextMeshProUGUI m_textGmp;
        
        private void OnValidate()
        {
            m_textGmp = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            //update to get the character count
            m_textGmp.ForceMeshUpdate();

            //get list of text vertices
            Vector3[] textVertices = m_textGmp.mesh.vertices;

            //loop through characters
            for (int i = 0; i < m_textGmp.textInfo.characterCount; ++i)
            {
                TMP_CharacterInfo cInfo = m_textGmp.textInfo.characterInfo[i];

                //if character is invisible, skip
                if (!cInfo.isVisible)
                    continue;

                //get vertex index
                int cVertexIndex = cInfo.vertexIndex;

                Vector2 charMidLine = new Vector2(
                    (textVertices[cVertexIndex + 0].x + textVertices[cVertexIndex + 2].x) / 2,
                    (cInfo.topRight).y);
                
                //translate all 4 vertices of each quad to align with mid of character/baseline
                Vector3 offset = charMidLine;
                for (int j = 0; j < 4; ++j)
                    textVertices[cVertexIndex + j] -= offset;
                
                float randYPos = Random.Range(-50, 50);
                Matrix4x4 matrix = Matrix4x4.TRS(new Vector3(0f, randYPos, 0f), Quaternion.identity, Vector3.one);

                textVertices[cVertexIndex + 0] = matrix.MultiplyPoint3x4(textVertices[cVertexIndex + 0]);
                textVertices[cVertexIndex + 1] = matrix.MultiplyPoint3x4(textVertices[cVertexIndex + 1]);
                textVertices[cVertexIndex + 2] = matrix.MultiplyPoint3x4(textVertices[cVertexIndex + 2]);
                textVertices[cVertexIndex + 3] = matrix.MultiplyPoint3x4(textVertices[cVertexIndex + 3]);

                m_textGmp.mesh.vertices = textVertices;

                //reset
                //for (int j = 0; j < 4; ++j)
                //    textVertices[cVertexIndex + j] += offset;

            }

            m_textGmp.ForceMeshUpdate();
        }
    }
}
