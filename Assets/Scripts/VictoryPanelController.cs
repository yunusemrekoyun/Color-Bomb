using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using System;
public class VictoryPanelController : MonoBehaviour
{
  [Header("Panel Yapısı")]
  public GameObject panelRoot; // VictoryPanel
  public RectTransform popup;  // VictoryWindow

  [Header("Yazılar")]
  public TMP_Text victoryText;
  public TMP_Text levelCompleteText;

  [Header("Yıldızlar")]
  public GameObject[] fullStars; // FullStar1,2,3

  [Header("Butonlar")]
  public Button restartButton;
  public Button nextButton;
  public Button shareButton;

  private void Start()
  {
    // Başlangıçta görünmesin
    panelRoot.SetActive(false);
    popup.localScale = Vector3.zero;

    foreach (var star in fullStars)
    {
      if (star != null) star.SetActive(false);
    }

    // Butonlara fonksiyon bağla
    restartButton.onClick.AddListener(RestartLevel);
    nextButton.onClick.AddListener(NextLevel);
    shareButton.onClick.AddListener(ShareLevel);
  }

  public void ShowVictory(int starCount)
  {
    Debug.Log("Victory Panel açıldı. Kazanılan yıldız: " + starCount);

    // Şu anki level anahtarını al
    string currentKey = PlayerPrefs.GetString("SelectedLevelJson", "level1");
    // Yıldızları sakla (önceki maksimum ile karşılaştır)
    string starKey = currentKey + "_stars";
    int prev = PlayerPrefs.GetInt(starKey, 0);
    Debug.Log("StarCount = " + starCount);
    if (starCount > prev)
      PlayerPrefs.SetInt(starKey, starCount);
    // Sonraki leveli unlock et
    int lvl = int.Parse(currentKey.Replace("level", ""));
    int nextLvl = lvl + 1;
    int unlocked = PlayerPrefs.GetInt("unlockedLevel", 1);
    PlayerPrefs.SetInt("unlockedLevel", Math.Max(unlocked, nextLvl));
    PlayerPrefs.Save();
    foreach (var star in fullStars)
    {
      if (star != null)
      {
        star.SetActive(false);
        star.transform.localScale = Vector3.zero; // animasyon reset
      }
    }
    panelRoot.SetActive(true);

    // VictoryWindow'u sıfır scale ile başlat, sonra animasyonla büyüt
    popup.localScale = Vector3.zero;
    LeanTween.scale(popup, Vector3.one, 0.4f).setEaseOutBack();

    // Arkaplanı karart (yarı opak siyah)
    var bg = panelRoot.GetComponent<UnityEngine.UI.Image>();
    if (bg != null)
    {
      bg.color = new Color(0, 0, 0, 0);
      LeanTween.value(panelRoot, 0f, 0.5f, 0.4f).setOnUpdate((float val) =>
      {
        bg.color = new Color(0, 0, 0, val);
      });
    }

    // ⭐ Yıldızları sırayla aç
    StartCoroutine(AnimateStars(starCount));
  }

  private IEnumerator AnimateStars(int count)
  {
    yield return new WaitForSeconds(0.4f);

    for (int i = 0; i < count && i < fullStars.Length; i++)
    {
      var star = fullStars[i];
      if (star != null)
      {
        star.SetActive(true);
        star.transform.localScale = Vector3.zero;
        LeanTween.scale(star, Vector3.one, 0.3f).setEaseOutBack();
        yield return new WaitForSeconds(0.2f);
      }
    }
  }

  private void RestartLevel()
  {
    Debug.Log("Restart pressed");
    // SceneManager.LoadScene vs.
  }

  private void NextLevel()
  {
    // Şu anki level anahtarını al
    string currentKey = PlayerPrefs.GetString("SelectedLevelJson", "level1");
    int lvl = int.Parse(currentKey.Replace("level", ""));
    string nextKey = "level" + (lvl + 1);

    // Eğer sonraki JSON dosyası varsa ilerle, yoksa log bas
    if (Resources.Load<TextAsset>($"Levels/{nextKey}") != null)
    {
      PlayerPrefs.SetString("SelectedLevelJson", nextKey);
      PlayerPrefs.Save();
      SceneManager.LoadScene("GamePlayScene");
    }
    else
    {
      Debug.Log("🚫 Son seviye tamamlandı, yeni level yok: " + nextKey);
      nextButton.interactable = false;
    }
  }

  private void ShareLevel()
  {
    Debug.Log("Returning to Level Select...");
    SceneManager.LoadScene("LevelSelectScene");
  }
}