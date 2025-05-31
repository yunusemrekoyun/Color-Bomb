using UnityEngine;
using System.Collections;

public class VictoryPanelController : MonoBehaviour
{
    public GameObject panelRoot;        // VictoryPanel objesi
    public RectTransform popup;         // VictoryWindow (popup görselin)

    public GameObject[] starObjects;

    private void Awake()
    {
        panelRoot.SetActive(false);
        popup.localScale = Vector3.zero;
    }

     public void ShowVictory(int starCount)
    {
        panelRoot.SetActive(true);      // VictoryPanel’i görünür yap

        popup.localScale = Vector3.zero;

        LeanTween.scale(popup, Vector3.one, 0.4f).setEaseOutBack();

        var bg = panelRoot.GetComponent<UnityEngine.UI.Image>();
        if (bg != null)
        {
            bg.color = new Color(0, 0, 0, 0);
            LeanTween.value(panelRoot, 0f, 0.5f, 0.4f).setOnUpdate((float val) =>
            {
                bg.color = new Color(0, 0, 0, val);
            });
        }

        Debug.Log("🔥 Victory ekranı açıldı");

        StartCoroutine(AnimateStars(starCount));
    }
    private IEnumerator AnimateStars(int count)
    {
        for (int i = 0; i < starObjects.Length; i++)
        {
            starObjects[i].SetActive(i < count);
            if (i < count)
            {
                starObjects[i].transform.localScale = Vector3.zero;
                LeanTween.scale(starObjects[i], Vector3.one, 0.3f).setEaseOutBack();
                yield return new WaitForSeconds(0.2f);
            }
        }
          yield return null;
    }

    public void HideVictory()
    {
        LeanTween.scale(popup, Vector3.zero, 0.3f)
            .setEaseInBack()
            .setOnComplete(() => panelRoot.SetActive(false));
    }
}
