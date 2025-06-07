using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;


public class LosePanelController : MonoBehaviour
{
    [Header("Panel Yapısı")]
    public GameObject panelRoot; // LosePanel
    public RectTransform popup;  // LoseWindow

    [Header("Yazılar")]
    public TMP_Text loseText;
    public TMP_Text reasonText; // "Out of moves!" gibi bir açıklama

    [Header("Butonlar")]
    public Button retryButton;
    public Button shareButton;

    private void Start()
    {
        panelRoot.SetActive(false);
        popup.localScale = Vector3.zero;

        retryButton.onClick.AddListener(RestartLevel);
        shareButton.onClick.AddListener(ShareLevel);
    }

    public void ShowLose()
    {
        Debug.Log("💀 Lose Panel açıldı.");

        panelRoot.SetActive(true);
        popup.localScale = Vector3.zero;

        LeanTween.scale(popup, Vector3.one, 0.4f).setEaseOutBack();

        // (İsteğe bağlı) arkaplan karartması
        var bg = panelRoot.GetComponent<UnityEngine.UI.Image>();
        if (bg != null)
        {
            bg.color = new Color(0, 0, 0, 0);
            LeanTween.value(panelRoot, 0f, 0.5f, 0.4f).setOnUpdate((float val) =>
            {
                bg.color = new Color(0, 0, 0, val);
            });
        }
    }

    private void RestartLevel()
    {
        Debug.Log("Retry pressed");
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    private void ShareLevel()
    {
        Debug.Log("Share pressed");
        // Platform özel paylaşım kodu buraya
    }
}