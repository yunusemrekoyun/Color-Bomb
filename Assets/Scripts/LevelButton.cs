using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelButton : MonoBehaviour
{
    public int levelNumber; // Bu sahnede elle verilecek

    public Image iconImage;
    public TMP_Text levelText;

    public Sprite lockedSprite;
    public Sprite completedSprite;
    public Sprite currentSprite;
    public Sprite targetSprite;

    public void SetState(string state)
    {
        levelText.text = levelNumber.ToString();

        switch (state)
        {
            case "locked":
                iconImage.sprite = lockedSprite;
                break;
            case "completed":
                iconImage.sprite = completedSprite;
                break;
            case "current":
                iconImage.sprite = currentSprite;
                break;
            case "target":
                iconImage.sprite = targetSprite;
                break;
        }
    }
}
