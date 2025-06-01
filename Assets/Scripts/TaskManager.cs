using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; // ✅ coroutine'ler için gerekli
[Serializable]
public class DestroyTask
{
    [Tooltip("Bu prefabın tag’ini takip edeceğiz")]
    public GameObject prefab;
    [Tooltip("Kaç kez patlaması gerekiyor")]
    public int count;
    [HideInInspector] public int remaining;
}

public class TaskManager : MonoBehaviour
{
    public VictoryPanelController victoryPanelController;

    [Header("◆ Balloon Destroy Tasks ◆")]
    [Tooltip("Aynı prefab‑ı birden fazla eklemeyin")]
    public List<DestroyTask> destroyTasks = new List<DestroyTask>(3);

    [Header("◆ UI References ◆")]
    public Image[] taskImages;  // Task1Image, Task2Image, Task3Image
    public TMP_Text[] taskTexts;   // Task1Text,  Task2Text,  Task3Text

    void OnValidate()
    {
        var seen = new HashSet<GameObject>();
        for (int i = 0; i < destroyTasks.Count; i++)
        {
            var p = destroyTasks[i].prefab;
            if (p == null) continue;
            if (seen.Contains(p))
                destroyTasks[i].prefab = null;
            else
                seen.Add(p);
        }
    }

    void Start()
    {
        InitTasks();
    }

    public void InitTasks()
    {
        for (int i = 0; i < destroyTasks.Count; i++)
        {
            var t = destroyTasks[i];
            t.remaining = t.count;

            if (i < taskImages.Length && t.prefab != null)
            {
                var sr = t.prefab.GetComponent<SpriteRenderer>();
                if (sr != null) taskImages[i].sprite = sr.sprite;
            }

            if (i < taskTexts.Length)
                taskTexts[i].text = t.remaining.ToString();
        }
    }

    public void OnItemDestroyed(GameObject itemGO)
    {
        for (int i = 0; i < destroyTasks.Count; i++)
        {
            var t = destroyTasks[i];
            if (t.prefab != null && t.remaining > 0 && itemGO.CompareTag(t.prefab.tag))
            {
                t.remaining--;
                if (i < taskTexts.Length)
                    taskTexts[i].text = t.remaining.ToString();
                CheckAllTasksComplete();
                break;
            }
        }
    }
    void CheckAllTasksComplete()
    {
        if (destroyTasks.All(t => t.remaining <= 0))
        {
            Debug.Log("Level tamamlandı");

            var movesManager = FindFirstObjectByType<MovesManager>();
            if (movesManager != null)
            {
                int remaining = movesManager.GetRemainingMoves();
                int bonus = remaining * 50;
                ScoreManager.Instance.AddScore(bonus);
                Debug.Log($"🎁 Bonus Skor Eklendi: {remaining} x 50 = {bonus}");
            }

            // Artık fillbar dolduktan sonra victory panel gösterilecek
            StartCoroutine(ShowVictoryDelayed());
        }
    }

    private IEnumerator ShowVictoryDelayed()
    {
        // ✅ fillAmount'ın güncellenmesini bekle
        yield return new WaitForSeconds(0.5f);

        int earnedStars = CalculateStarCount();
        Debug.Log("🎖️ Yıldız sayısı hesaplandı: " + earnedStars);
        victoryPanelController.ShowVictory(earnedStars);
    }
    private int CalculateStarCount()
    {
        var bar = FindFirstObjectByType<ScoreFillBar>();
        if (bar == null) return 0;

        float fill = bar.GetCurrentFill(); // Aşağıda göstereceğim

        if (fill >= 1f) return 3;
        else if (fill >= 0.66f) return 2;
        else if (fill >= 0.33f) return 1;
        else return 0;
    }
}
