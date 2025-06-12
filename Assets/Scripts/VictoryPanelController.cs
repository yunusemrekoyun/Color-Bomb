using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class VictoryPanelController : MonoBehaviour
{
  [Header("Panel Yapısı")]
  public GameObject panelRoot;     // VictoryPanel
  public RectTransform popup;      // VictoryWindow

  [Header("Yazılar")]
  public TMP_Text victoryText;     // İsteğe bağlı: "You Win!"
  public TMP_Text levelCompleteText; // İsteğe bağlı: "Level X Complete!"

  [Header("Yıldızlar")]
  public GameObject[] fullStars;   // FullStar1, FullStar2, FullStar3

  [Header("Butonlar")]
  public Button restartButton;
  public Button nextButton;
  public Button shareButton;

  int _worldIndex;
  int _levelIndex;

  void Start()
  {
    // Başlangıçta gizle
    panelRoot.SetActive(false);
    popup.localScale = Vector3.zero;
    foreach (var star in fullStars)
      if (star != null) star.SetActive(false);

    // Tıkları bağla
    restartButton.onClick.AddListener(RestartLevel);
    nextButton.onClick.AddListener(NextLevel);
    shareButton.onClick.AddListener(ShareLevel);

    // Hangi dünya & level?
    _worldIndex = PlayerPrefs.GetInt("SelectedWorld", 0);
    string key = PlayerPrefs.GetString("SelectedLevelJson", "level1");
    _levelIndex = int.Parse(key.Replace("level", ""));
  }

  public void ShowVictory(int starCount)
  {
    // 1) Kaydet
    SaveManager.Instance.SetStars(_worldIndex, _levelIndex, starCount);
    SaveManager.Instance.UnlockNextLevel(_worldIndex);
    SaveManager.Instance.Save();

    // 2) Metinleri güncelle (isteğe bağlı)
    if (victoryText != null)
      victoryText.text = "You Win!";
    if (levelCompleteText != null)
      levelCompleteText.text = $"Level {_levelIndex} Complete!";

    // 3) Paneli görünür kıl
    foreach (var star in fullStars)
    {
      if (star != null)
      {
        star.SetActive(false);
        star.transform.localScale = Vector3.zero;
      }
    }
    panelRoot.SetActive(true);

    // 4) Animasyon
    popup.localScale = Vector3.zero;
    LeanTween.scale(popup, Vector3.one, 0.4f).setEaseOutBack();

    var bg = panelRoot.GetComponent<Image>();
    if (bg != null)
    {
      bg.color = new Color(0, 0, 0, 0);
      LeanTween.value(panelRoot, 0f, 0.5f, 0.4f)
               .setOnUpdate((float v) => bg.color = new Color(0, 0, 0, v));
    }

    // 5) Yıldızları sırayla aç
    StartCoroutine(AnimateStars(starCount));
  }

  IEnumerator AnimateStars(int count)
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

  void RestartLevel()
  {
    SceneManager.LoadScene("GamePlayScene");
  }

  void NextLevel()
  {
    // Aynı sahneyi reload et; SaveManager zaten açılan leveli güncelledi
    SceneManager.LoadScene("GamePlayScene");
  }

  void ShareLevel()
  {
    SceneManager.LoadScene("LevelSelectScene");
  }
}