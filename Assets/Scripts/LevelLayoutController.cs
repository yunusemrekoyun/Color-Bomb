using UnityEngine;

public class LevelLayoutController : MonoBehaviour
{
    public int playerCurrentLevel = 2;

    public Sprite lockedSprite, completedSprite, currentSprite, targetSprite;

    void Start()
    {
        LevelButton[] buttons = GetComponentsInChildren<LevelButton>();
        int unlocked = PlayerPrefs.GetInt("unlockedLevel", 1); // default 1

        foreach (LevelButton btn in buttons)
        {
            int lvl = btn.levelNumber;

            // 1) Yıldızları kayıttan oku
            int savedStars = PlayerPrefs.GetInt($"level{lvl}_stars", 0);
            btn.SetStars(savedStars);

            // 2) State ayarla
            string state = lvl < unlocked ? "completed"
                         : lvl == unlocked ? "current"
                         : lvl == unlocked + 1 ? "target"
                         : "locked";

            btn.lockedSprite = lockedSprite;
            btn.completedSprite = completedSprite;
            btn.currentSprite = currentSprite;
            btn.targetSprite = targetSprite;

            btn.SetState(state);
        }
    }
}
