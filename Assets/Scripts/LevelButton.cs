using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelButton : MonoBehaviour
{

    public int levelNumber;
  

    public Image iconImage;
    public TMP_Text levelText;

    public Sprite lockedSprite;
    public Sprite completedSprite;
    public Sprite currentSprite;
    public Sprite targetSprite;

    public Image[] starImages; // 3 yýldýzý Inspector üzerinden baðlayacaðýz

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
            starImages[i].enabled = true; // Tüm yýldýzlar görünsün

            Color color = starImages[i].color;

            if (i < starCount)
                color.a = 1f; // Tam opak (aktif yýldýz)
            else
                color.a = 80f / 255f; // Saydamlaþtýr (boþ yýldýz)

            starImages[i].color = color;
        }
    }


    void OnClick()
    {
        int selectedWorld = PlayerPrefs.GetInt("SelectedWorld", 0);
        Debug.Log($"World {selectedWorld + 1} - Level {levelNumber} sahnesine gidiliyor (simülasyon)");
    }
}
