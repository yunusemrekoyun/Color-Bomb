using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class WorldManager : MonoBehaviour
{
    public Image worldPreviewImage;       // WorldPreview objesindeki Image
    public TMP_Text worldLabelText;       // WorldLabel içindeki TMP_Text
    public Transform starsContainer;      // StarsContainer objesi
    public Sprite fullStarSprite;         // Dolu yýldýz sprite
    public Sprite emptyStarSprite;        // Boþ yýldýz sprite

    public List<WorldData> worlds;
    private int currentIndex = 0;

    private void Start()
    {
        UpdateWorldUI();
    }

    public void ShowNextWorld()
    {
        currentIndex = (currentIndex + 1) % worlds.Count;
        UpdateWorldUI();
    }

    public void ShowPreviousWorld()
    {
        currentIndex = (currentIndex - 1 + worlds.Count) % worlds.Count;
        UpdateWorldUI();
    }

    private void UpdateWorldUI()
    {
        WorldData data = worlds[currentIndex];
        worldPreviewImage.sprite = data.worldPreview;
        worldLabelText.text = data.worldName;

        for (int i = 0; i < starsContainer.childCount; i++)
        {
            Image starImage = starsContainer.GetChild(i).GetComponent<Image>();
            if (i < data.earnedStars)
                starImage.sprite = fullStarSprite;
            else
                starImage.sprite = emptyStarSprite;
        }
    }
}
