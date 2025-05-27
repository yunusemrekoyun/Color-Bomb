using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class DestroyTask
{
    [Tooltip("Bu prefab’ın tag’ini takip edeceğiz")]
    public GameObject prefab;
    [Tooltip("Kaç kez patlaması gerekiyor")]
    public int count;
    [HideInInspector] public int remaining;
}

public class TaskManager : MonoBehaviour
{
    [Header("◆ Balloon Destroy Tasks ◆")]
    [Tooltip("Aynı prefab’ı birden fazla eklemeyin")]
    public List<DestroyTask> destroyTasks = new List<DestroyTask>(3);

    [Header("◆ UI References ◆")]
    public Image[] taskImages;  // Task1Image, Task2Image, Task3Image
    public Text[] taskTexts;   // Task1Text,  Task2Text,  Task3Text

    void OnValidate()
    {
        // Inspector’da duplicate seçimi engelle
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
            // UI görselini ayarla
            if (i < taskImages.Length && t.prefab != null)
            {
                var sr = t.prefab.GetComponent<SpriteRenderer>();
                if (sr != null) taskImages[i].sprite = sr.sprite;
            }
            // UI metnini ayarla
            if (i < taskTexts.Length)
                taskTexts[i].text = t.remaining.ToString();
        }
    }

    /// <summary>
    /// Patlayan her item için çağırılacak.
    /// </summary>
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
            Debug.Log("Level tamamlandı");
    }
}