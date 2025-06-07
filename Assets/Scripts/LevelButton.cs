using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public class LevelButton : MonoBehaviour
{

    public int levelNumber;


    public Image iconImage;
    public TMP_Text levelText;

    public Sprite lockedSprite;
    public Sprite completedSprite;
    public Sprite currentSprite;
    public Sprite targetSprite;

    public Image[] starImages; // 3 y�ld�z� Inspector �zerinden ba�layaca��z

    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    public void SetState(string state)
    {
        levelText.text = levelNumber.ToString();

        switch (state)
        {
            case "locked":
                iconImage.sprite = lockedSprite;
                button.interactable = false;
                break;
            case "completed":
                iconImage.sprite = completedSprite;
                button.interactable = true;
                break;
            case "current":
                iconImage.sprite = currentSprite;
                button.interactable = true;
                break;
            case "target":
                iconImage.sprite = targetSprite;
                button.interactable = false;
                break;
        }
    }

    public void SetStars(int starCount)
    {
        for (int i = 0; i < starImages.Length; i++)
        {
            starImages[i].enabled = true; // T�m y�ld�zlar g�r�ns�n

            Color color = starImages[i].color;

            if (i < starCount)
                color.a = 1f; // Tam opak (aktif y�ld�z)
            else
                color.a = 80f / 255f; // Saydamla�t�r (bo� y�ld�z)

            starImages[i].color = color;
        }
    }

    void OnClick()
    {
        // Seçilen level numarasına göre JSON anahtarını oluştur
        string key = $"level{levelNumber}";

        // PlayerPrefs’e kaydet
        PlayerPrefs.SetString("SelectedLevelJson", key);
        PlayerPrefs.Save();

        // Oyun sahnesini (GamePlayScene) yükle
        SceneManager.LoadScene("GameplayScene");
    }
}
