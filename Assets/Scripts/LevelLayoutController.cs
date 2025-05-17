using UnityEngine;

public class LevelLayoutController : MonoBehaviour
{
    public int playerCurrentLevel = 2;

    public Sprite lockedSprite, completedSprite, currentSprite, targetSprite;

    void Start()
    {
        LevelButton[] buttons = GetComponentsInChildren<LevelButton>();

        foreach (LevelButton btn in buttons)
        {
            string state = "locked";

            if (btn.levelNumber < playerCurrentLevel)
                state = "completed";
            else if (btn.levelNumber == playerCurrentLevel)
                state = "current";
            else if (btn.levelNumber == playerCurrentLevel + 1)
                state = "target";

            btn.lockedSprite = lockedSprite;
            btn.completedSprite = completedSprite;
            btn.currentSprite = currentSprite;
            btn.targetSprite = targetSprite;

            btn.SetState(state);
        }
    }
}
