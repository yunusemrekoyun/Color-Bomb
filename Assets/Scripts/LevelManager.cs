using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class LevelManager : MonoBehaviour
{
    [Header("Level JSON Dosyası (Resources/Levels/)")]
    [Tooltip("MapScene’den PlayerPrefs ile ayarlanır; fallback için default kullanılır")]
    public string defaultLevelJsonName = "level1";

    [Header("Prefab Mappings (Key → Prefab)")]
    public List<NamedPrefab> prefabMappings;

    // Bu tipler GameBoard.cs içinden geliyor
    GameBoard board;
    TaskManager taskManager;
    MovesManager movesManager;

    void Awake()
    {
        // Obsolete uyarılarını önlemek için:
        board = FindFirstObjectByType<GameBoard>();
        taskManager = FindFirstObjectByType<TaskManager>();
        movesManager = FindFirstObjectByType<MovesManager>();

        if (board == null || taskManager == null || movesManager == null)
            Debug.LogError("⚠️ LevelManager: Gerekli manager bulunamadı!");

        string jsonName = PlayerPrefs.GetString("SelectedLevelJson", defaultLevelJsonName);
        LoadAndApplyLevel(jsonName);
    }

    void LoadAndApplyLevel(string levelJsonName)
    {
        // 1) JSON’u al
        TextAsset txt = Resources.Load<TextAsset>($"Levels/{levelJsonName}");
        if (txt == null)
        {
            Debug.LogError($"❌ Level JSON bulunamadı: Levels/{levelJsonName}.json");
            return;
        }

        // 2) Parse et
        LevelData data;
        try { data = JsonUtility.FromJson<LevelData>(txt.text); }
        catch (Exception ex)
        {
            Debug.LogError($"❌ JSON parse hatası: {ex.Message}");
            return;
        }

        // 3) GameBoard’a uygula
        board.blockedPositions = data.blockedPositions;
        board.scoreMultiplier = data.scoreMultiplier;
        board.maxMoves = data.maxMoves;

        // 4) Balon prefab’ları
        board.balloonPrefabs = data.balloonPrefabs
            .Select(key => GetMappedPrefab(key))
            .Where(go => go != null)
            .ToArray();

        // 5) Kırılabilir blok pozisyonları
        board.glassTiles = data.glassTiles;
        board.boxTiles = data.boxTiles;

        // 6) Görevleri oluştur
        taskManager.destroyTasks.Clear();
        foreach (var dt in data.destroyTasks)
        {
            var go = GetMappedPrefab(dt.prefab);
            if (go == null)
            {
                Debug.LogWarning($"⚠️ Task prefab mapping bulunamadı: GetMappedPrefab(dt.prefab)");
                continue;
            }
            var t = new DestroyTask
            {
                prefab = go,
                count = dt.count,
                remaining = dt.count
            };
            taskManager.destroyTasks.Add(t);
        }
        // TaskManager kendi Start’ında InitTasks() diyecek

        // 7) Hamle sayısını ayarla
        movesManager.InitializeMoves(data.maxMoves);

        Debug.Log($"✅ Level '{levelJsonName}' başarıyla yüklendi.");
    }

    GameObject GetMappedPrefab(string key)
    {
        var mp = prefabMappings.FirstOrDefault(m => m.key == key);
        return mp.prefab;
    }

    [Serializable]
    public struct NamedPrefab
    {
        public string key;
        public GameObject prefab;
    }

    [Serializable]
    public class LevelData
    {
        // Burada GameBoard.cs içindeki tiplere bak:
        public List<BlockedPosition> blockedPositions;
        public List<string> balloonPrefabs;
        public int scoreMultiplier;
        public int maxMoves;
        public List<GlassPosition> glassTiles;
        public List<BoxPosition> boxTiles;
        public List<TaskEntry> destroyTasks;
    }

    [Serializable]
    public class TaskEntry
    {
        // JSON’da "prefab" olarak tanımladığın alanla birebir eşleşecek:
        public string prefab;
        public int count;
    }
}