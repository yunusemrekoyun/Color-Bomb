using System.Collections;
using UnityEngine;
using TMPro;

[ExecuteAlways]
[RequireComponent(typeof(TMP_Text))]
public class TextArcEffect : MonoBehaviour
{
    public float arcHeight = 10f; // Ne kadar yukarý kavis yapsýn
    private TMP_Text textMesh;

    void Start()
    {
        textMesh = GetComponent<TMP_Text>();
        StartCoroutine(ApplyArcEffect());
    }

    IEnumerator ApplyArcEffect()
    {
        while (true)
        {
            textMesh.ForceMeshUpdate();
            TMP_TextInfo textInfo = textMesh.textInfo;

            if (textInfo.characterCount == 0)
            {
                yield return null;
                continue;
            }

            float centerX = textMesh.preferredWidth / 2f;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible)
                    continue;

                int matIndex = textInfo.characterInfo[i].materialReferenceIndex;
                int vertIndex = textInfo.characterInfo[i].vertexIndex;

                Vector3[] verts = textInfo.meshInfo[matIndex].vertices;

                Vector3 offset = (verts[vertIndex] + verts[vertIndex + 2]) / 2;
                float distanceFromCenter = offset.x - centerX;
                float yOffset = -Mathf.Pow(distanceFromCenter / centerX, 2) * arcHeight + arcHeight;

                for (int j = 0; j < 4; j++)
                {
                    verts[vertIndex + j].y += yOffset;
                }
            }

            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textMesh.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }

            yield return null;
        }
    }
}
