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

    /// <summary>
    /// Tüm kaydı sıfırlar (hem JSON hem de PlayerPrefs için WebGL fallback).
    /// </summary>
    public void ResetProgress()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL: PlayerPrefs içindeki saveData anahtarını sil
        PlayerPrefs.DeleteKey("saveData");
        PlayerPrefs.Save();
        Debug.Log("[SaveManager] WebGL: PlayerPrefs 'saveData' silindi.");
#else
        // Standart platform: diskten dosyayı sil
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("[SaveManager] Disk: saveData.json silindi.");
        }
#endif
        // Bellekteki veriyi default ile yeniden yükle
        InitializeSave();
        Debug.Log("[SaveManager] RAM verisi yeniden yüklendi.");
    }

    /// <summary>
    /// Kaydetme altyapısını başlatır: dosya yoksa kopyalar, sonra Data'yı okur.
    /// </summary>
    void InitializeSave()
    {
        string json;
#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL: PlayerPrefs fallback
        json = PlayerPrefs.GetString("saveData", "");
        if (string.IsNullOrEmpty(json))
        {
            var txt = Resources.Load<TextAsset>("Save/saveData");
            json = txt != null
                ? txt.text
                : "{\"worldCount\":3,\"levelsPerWorld\":8,\"unlockedLevels\":[1,1,1],\"stars\":[]}";
            PlayerPrefs.SetString("saveData", json);
            PlayerPrefs.Save();
        }
        Debug.Log("[SaveManager] WebGL: JSON yüklendi from PlayerPrefs.");
#else
        // Standart platform: disk okuma
        Debug.Log($"[SaveManager] persistentDataPath = {Application.persistentDataPath}");
        Debug.Log($"[SaveManager] persistent file exists? {File.Exists(savePath)} at {savePath}");

        if (!File.Exists(savePath))
        {
            var txt = Resources.Load<TextAsset>("Save/saveData");
            Debug.Log($"[SaveManager] Resources.Load → txt is {(txt == null ? "null" : "len=" + txt.text.Length)}");
            if (txt != null)
                File.WriteAllText(savePath, txt.text);
        }

        if (File.Exists(savePath))
        {
            json = File.ReadAllText(savePath);
            Debug.Log($"[SaveManager] Disk: Loaded JSON:\n{json}");
        }
        else
        {
            Debug.LogError("[SaveManager] Disk: save file still missing!");
            json = "{\"worldCount\":3,\"levelsPerWorld\":8,\"unlockedLevels\":[1,1,1],\"stars\":[]}";
        }
#endif
        // Ortak: Data'yı parse et
        Data = JsonUtility.FromJson<SaveData>(json);
    }

    /// <summary>
    /// Mevcut Data'yı tekrar kaydeder.
    /// </summary>
    public void Save()
    {
        string json = JsonUtility.ToJson(Data, true);

#if UNITY_WEBGL && !UNITY_EDITOR
        PlayerPrefs.SetString("saveData", json);
        PlayerPrefs.Save();
        Debug.Log("[SaveManager] WebGL: JSON kaydedildi to PlayerPrefs.");
#else
        File.WriteAllText(savePath, json);
        Debug.Log("[SaveManager] Disk: JSON kaydedildi to saveData.json.");
#endif
    }

    // --- API Metodları ---

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