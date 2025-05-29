using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreFillBar : MonoBehaviour
{
    public Image fillImage;
    public Image[] starImages;
    public Sprite greyStar;
    public Sprite yellowStar;
    public int maxScore = 100;
    public Material yellowStarMaterial;
    private GameBoard gameBoard;

    void Start()
    {
        gameBoard = FindFirstObjectByType<GameBoard>();
        fillImage.fillAmount = 0f;
    }

    void Update()
    {
        if (gameBoard == null) return;

        int currentScore = ScoreManager.Instance.currentScore;

        // Skor katsayısını kullanarak oranı hesapla
        float fillAmount = Mathf.Clamp01(
            (float)currentScore / (maxScore * gameBoard.scoreMultiplier)
        );

        fillImage.fillAmount = fillAmount;

        for (int i = 0; i < starImages.Length; i++)
        {
            float threshold = (i + 1) / (float)starImages.Length;

            if (fillAmount >= threshold)
            {
                if (starImages[i].sprite != yellowStar)
                {
                    starImages[i].sprite = yellowStar;
                    starImages[i].material = yellowStarMaterial;

                    // ✨ Zoom animasyonu
                    StartCoroutine(PlayStarPopAnimation(starImages[i]));
                }
            }
            else
            {
                starImages[i].sprite = greyStar;
                starImages[i].material = null;
            }
        }
    }

    private IEnumerator PlayStarPopAnimation(Image starImage)
    {
        Vector3 originalScale = starImage.transform.localScale;
        Vector3 zoomScale = originalScale * 1.3f;
        float duration = 0.15f;
        float elapsed = 0f;

        // Zoom in
        while (elapsed < duration)
        {
            starImage.transform.localScale = Vector3.Lerp(originalScale, zoomScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Zoom out
        elapsed = 0f;
        while (elapsed < duration)
        {
            starImage.transform.localScale = Vector3.Lerp(zoomScale, originalScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        starImage.transform.localScale = originalScale;
    }
}