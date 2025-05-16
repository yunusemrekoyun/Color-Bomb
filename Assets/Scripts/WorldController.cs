using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorldController : MonoBehaviour
{
    [Header("World Data")]
    public List<WorldData> worlds;
    public int currentWorldIndex = 0;

    [Header("UI References")]
    public Image backgroundImage;                    // Arka plan görseli (Background Image objesi)
    public WorldPreview worldPreview;                // WorldPreview scripti (Prefab içindeki script atanacak)

    public void Start()
    {
        UpdateUI();
    }

    public void ShowNextWorld()
    {
        if (currentWorldIndex < worlds.Count - 1)
        {
            currentWorldIndex++;
            UpdateUI();
        }
    }

    public void ShowPreviousWorld()
    {
        if (currentWorldIndex > 0)
        {
            currentWorldIndex--;
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        if (worlds == null || worlds.Count == 0 || currentWorldIndex >= worlds.Count)
            return;

        WorldData currentWorld = worlds[currentWorldIndex];

        // WorldPreview scriptini güncelle
        if (worldPreview != null)
        {
            worldPreview.SetData(currentWorld, currentWorldIndex);
        }

        // Arka planý deðiþtir
        if (backgroundImage != null)
        {
            backgroundImage.sprite = currentWorld.backgroundSprite;
        }
    }
}
