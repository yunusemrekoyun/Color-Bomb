using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GridCleanupManager:
///  - Inspector’dan atayacağınız prefab referanslarına göre sahneyi tarar:
///    • glassPrefab  : Tek bir Cam (Glass) blok prefab’ı.
///    • boxPrefab    : Tek bir Kutu (Box) blok prefab’ı.
///    • balloonPrefabs[] : Birden fazla (örn. 8) normal Balloon prefab’ı.
///    • specialPrefabs[] : Birden fazla (örn. 5) Özel (Special) item prefab’ı.
///
///  - Her checkInterval saniyede bir:
///    1) Sahnedeki tüm Glass/Box örneklerini toplayıp “blok hücreleri” kümesine ekler.
///    2) Sahnedeki tüm BalloonItem örneklerini balloonPrefabs dizisindeki her prefab’a
///       göre künyeler (adı prefabRef.name ile başlayanları) toplayıp “hücre → list” eşler.
///    3) Sahnedeki tüm SpecialItem örneklerini specialPrefabs dizisindeki her prefab’a
///       göre künyeler (adı prefabRef.name ile başlayanları) toplayıp “hücre → list” eşler.
///    4) Her hücre için:
///       • Eğer hücre Glass/Box ise, o hücredeki listedeki tüm Balloon ve Special örnekler yok edilir.
///       • Değilse, listede 1’den fazla Balloon veya Special varsa, fazladakiler yok edilir (sadece ilki kalır).
///
///  - Performans: checkInterval süresini 0.1 – 0.2 saniye civarında tutarak yeterli sıklıkta tarama yapın.
///  - İleride devre dışı bırakmak isterseniz, bu script’i eklediğiniz GameObject’i disable ediniz.
/// </summary>
public class GridCleanupManager : MonoBehaviour
{
    [Header("▶ Prefab Referansları (Inspector’dan sürükleyin)")]
    [Tooltip("Cam (Glass) blok prefab’ı")]
    public GameObject glassPrefab;

    [Tooltip("Kutu (Box) blok prefab’ı")]
    public GameObject boxPrefab;

    [Tooltip("Normal Balloon prefab’ları (örn. 8 farklı tür)")]
    public GameObject[] balloonPrefabs;

    [Tooltip("Special Item prefab’ları (örn. 5 farklı tür)")]
    public GameObject[] specialPrefabs;

    [Header("▶ Cleanup Ayarları")]
    [Tooltip("Kaç saniyede bir grid’i tarayıp temizleyecek")]
    public float checkInterval = 0.1f;

    // -------------------------------------------------------
    // İçeride “blok hücreleri” ve “hücre → nesne listesi” eşlemeleri tutulacak:
    private GameBoard board;

    private void Awake()
    {
        // Eğer Inspector’dan atamadıysanız: sahnede tek bir GameBoard bileşeni bul
        if (board == null)
            board = FindObjectOfType<GameBoard>();

        if (board == null)
            Debug.LogError("GridCleanupManager: Scene içinde bir GameBoard bulunamadı!");
    }

    private void OnEnable()
    {
        // Başlangıçta çok kısa bekleyip cleanup döngüsünü başlat
        StartCoroutine(CleanupRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    /// <summary>
    /// Belirlenen aralıkla CleanupGrid() metodunu çağırır.
    /// </summary>
    private IEnumerator CleanupRoutine()
    {
        // Diğer sistemlerin init olmasına izin vermek için kısa bekleme
        yield return new WaitForSeconds(0.05f);

        while (true)
        {
            CleanupGrid();
            yield return new WaitForSeconds(checkInterval);
        }
    }

    /// <summary>
    /// Tüm grid hücrelerindeki çakışmaları temizler:
    ///  - Glass/Box hücrelerinin içindeki tüm Balloon/Special nesneleri siler.
    ///  - Diğer hücrelerde birden fazla nesne varsa sadece bir tanesini bırakır, fazlalıkları yok eder.
    /// </summary>
    private void CleanupGrid()
    {
        if (board == null) return;

        int width = board.width;
        int height = board.height;
        float spacing = board.spacing;
        float offsetX = board.offsetX;
        float offsetY = board.offsetY;

        // 1) Glass ve Box hücrelerini topla
        HashSet<Vector2Int> blockCells = new HashSet<Vector2Int>();
        CollectBlockCells(glassPrefab, blockCells);
        CollectBlockCells(boxPrefab, blockCells);

        // 2) BalloonItem örneklerini hücre haritasına ekle
        Dictionary<Vector2Int, List<GameObject>> cellMap = new Dictionary<Vector2Int, List<GameObject>>();
        CollectItemsIntoMap(balloonPrefabs, cellMap, spacing, offsetX, offsetY);

        // 3) SpecialItem örneklerini hücre haritasına ekle
        CollectItemsIntoMap(specialPrefabs, cellMap, spacing, offsetX, offsetY);

        // 4) Her hücre için çakışma/temizleme yap
        foreach (var kv in cellMap)
        {
            Vector2Int cell = kv.Key;
            List<GameObject> objs = kv.Value;

            // 4a) Hücre Glass/Box ise, listede ne varsa hepsini sil
            if (blockCells.Contains(cell))
            {
                for (int i = 0; i < objs.Count; i++)
                {
                    if (objs[i] != null)
                        DestroyImmediate(objs[i]);
                }
            }
            // 4b) Aksi halde, birden fazla nesne varsa (aynı hücrede ikiden fazla)
            else if (objs.Count > 1)
            {
                // Sadece listedeki ilk elemanı bırak, geri kalanları sil
                for (int i = 1; i < objs.Count; i++)
                {
                    if (objs[i] != null)
                        DestroyImmediate(objs[i]);
                }
            }
        }
    }

    /// <summary>
    /// Belirtilen tek bir prefabRef’ı baz alarak, sahnedeki tüm örneklerini tarar,
    /// pozisyonlarını grid hücresine dönüştürüp blockCells kümesine ekler.
    /// </summary>
    private void CollectBlockCells(GameObject prefabRef, HashSet<Vector2Int> blockCells)
    {
        if (prefabRef == null) return;

        // Sahnedeki tüm GameObject’leri al
        GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();
        for (int i = 0; i < allGameObjects.Length; i++)
        {
            GameObject go = allGameObjects[i];
            if (go == null) continue;

            // Prefab ile tam eşleşme: go.name, prefabRef.name ile başlıyorsa (örn. "Glass" vs "Glass(Clone)")
            if (!go.name.StartsWith(prefabRef.name)) continue;

            // Hücre koordinatını bul (x,y)
            Vector3 pos = go.transform.position;
            int cellX = Mathf.RoundToInt((pos.x - board.offsetX) / board.spacing);
            int cellY = Mathf.RoundToInt((pos.y - board.offsetY) / board.spacing);
            Vector2Int cell = new Vector2Int(cellX, cellY);

            // Sınırlar içinde mi?
            if (cellX < 0 || cellX >= board.width || cellY < 0 || cellY >= board.height)
                continue;

            blockCells.Add(cell);
        }
    }

    /// <summary>
    /// Birden fazla prefabRef dizisini baz alarak, her biri için sahnedeki örnekleri tarar,
    /// pozisyonlarını grid hücresine dönüştürüp cellMap içerisine ekler:
    ///   • balloonPrefabs[] veya specialPrefabs[]
    /// 
    /// cellMap: Hücre → O hücredeki GameObject listesi
    /// </summary>
    private void CollectItemsIntoMap(
        GameObject[] prefabRefs,
        Dictionary<Vector2Int, List<GameObject>> cellMap,
        float spacing,
        float offsetX,
        float offsetY)
    {
        if (prefabRefs == null || prefabRefs.Length == 0) return;

        // Sahnedeki tüm GameObject’leri al
        GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();
        for (int i = 0; i < allGameObjects.Length; i++)
        {
            GameObject go = allGameObjects[i];
            if (go == null) continue;

            // Her prefabRef ile karşılaştır:
            bool isMatch = false;
            for (int p = 0; p < prefabRefs.Length; p++)
            {
                GameObject prefabRef = prefabRefs[p];
                if (prefabRef == null) continue;

                // “go.name” prefabRef.name ile başlamalı (örn. “BalloonRed” vs “BalloonRed(Clone)”)
                if (go.name.StartsWith(prefabRef.name))
                {
                    isMatch = true;
                    break;
                }
            }

            if (!isMatch) continue;

            // Eşleşen GameObject’in pozisyonunu hücreye dönüştür
            Vector3 pos = go.transform.position;
            int cellX = Mathf.RoundToInt((pos.x - offsetX) / spacing);
            int cellY = Mathf.RoundToInt((pos.y - offsetY) / spacing);
            Vector2Int cell = new Vector2Int(cellX, cellY);

            // Sınırlar içinde mi?
            if (cellX < 0 || cellX >= board.width || cellY < 0 || cellY >= board.height)
                continue;

            // Hücre henüz map’te yoksa, yeni liste oluştur:
            if (!cellMap.ContainsKey(cell))
                cellMap[cell] = new List<GameObject>();

            cellMap[cell].Add(go);
        }
    }
}