using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-200)]
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    private string savePath;
    public SaveData Data;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, "saveData.json");
        InitializeSave();
    }

    void InitializeSave()
    {
        Debug.Log($"[SaveManager] persistentDataPath = {Application.persistentDataPath}");
        Debug.Log($"[SaveManager] persistent file exists? {File.Exists(savePath)} at {savePath}");

        if (!File.Exists(savePath))
        {
            var txt = Resources.Load<TextAsset>("Save/saveData");
            Debug.Log($"[SaveManager] Resources.Load → txt is {(txt == null ? "null" : ("len=" + txt.text.Length))}");
            if (txt != null)
                File.WriteAllText(savePath, txt.text);
        }

        // Son olarak dosya içeriğini bir kere loglayalım
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            Debug.Log($"[SaveManager] Loaded JSON:\n{json}");
            Data = JsonUtility.FromJson<SaveData>(json);
        }
        else
        {
            Debug.LogError("[SaveManager] save file still missing!");
        }
    }

    public void Save()
    {
        string json = JsonUtility.ToJson(Data, true);
        File.WriteAllText(savePath, json);
    }
    public int GetUnlockedLevel(int world) => Data.unlockedLevels[world];

    public void UnlockNextLevel(int world)
    {
        int val = Data.unlockedLevels[world];
        if (val < Data.levelsPerWorld)
            Data.unlockedLevels[world] = val + 1;
    }

    public int GetStars(int world, int level)
    {
        var entry = Data.stars.Find(x => x.worldIndex == world && x.levelIndex == level);
        return entry != null ? entry.stars : 0;
    }

    public void SetStars(int world, int level, int stars)
    {
        var entry = Data.stars.Find(x => x.worldIndex == world && x.levelIndex == level);
        if (entry != null)
            entry.stars = Math.Max(entry.stars, stars);
        else
            Data.stars.Add(new LevelStarEntry { worldIndex = world, levelIndex = level, stars = stars });
    }
    // API metodları buraya gelecek...
}

[Serializable]
public class SaveData
{
    public int worldCount;
    public int levelsPerWorld;
    public List<int> unlockedLevels;
    public List<LevelStarEntry> stars;
}

[Serializable]
public class LevelStarEntry
{
    public int worldIndex;
    public int levelIndex;
    public int stars;
}