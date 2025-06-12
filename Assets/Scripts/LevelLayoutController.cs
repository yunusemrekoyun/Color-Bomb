using UnityEngine;

public class LevelLayoutController : MonoBehaviour
{
    [Header("Button Sprites")]
    public Sprite lockedSprite;
    public Sprite completedSprite;
    public Sprite currentSprite;

    void Start()
    {
        // Hangi dünya seçili?
        int world = PlayerPrefs.GetInt("SelectedWorld", 0);
        // Bu dünyada en yüksek açılan level
        int unlocked = SaveManager.Instance.GetUnlockedLevel(world);

        // Tüm LevelButton’ları sırala
        foreach (var btn in GetComponentsInChildren<LevelButton>())
        {
            int lvl = btn.levelNumber;

            // 1) Yıldız sayısını SaveManager’dan çek
            int stars = SaveManager.Instance.GetStars(world, lvl);
            btn.SetStars(stars);

            // 2) Durumu belirle
            string state;
            if (lvl < unlocked) state = "completed";
            else if (lvl == unlocked) state = "current";
            else state = "locked";

            // 3) Sprite’ları set et
            btn.lockedSprite = lockedSprite;
            btn.completedSprite = completedSprite;
            btn.currentSprite = currentSprite;

            // 4) Uygula
            btn.SetState(state);
        }
    } 
}   