using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BlockedPosition { public int x, y; }

[System.Serializable]
public struct GlassPosition { public int x, y; }

[System.Serializable]
public struct BoxPosition { public int x, y; }

public class GameBoard : MonoBehaviour
{
    [Header("▶ Hamle Ayarları")]
    [Tooltip("Oyuncuya bu sahnede verilecek toplam hamle sayısı")]
    public int maxMoves = 20;

    [Header("Breakable Tile Prefabs")]
    public GameObject glassPrefab;
    public GameObject boxPrefab;

    [Header("Normal Balloon Prefabs")]
    public GameObject[] balloonPrefabs;

    [Header("▶ Skor Ayarları")]
    public int scoreMultiplier = 10;

    [Header("Empty Positions")]
    public List<BlockedPosition> blockedPositions = new List<BlockedPosition>();

    [Header("Breakable Tile Positions")]
    public List<GlassPosition> glassTiles = new List<GlassPosition>();
    public List<BoxPosition> boxTiles = new List<BoxPosition>();

    [Header("Special Item Prefabs")]
    public GameObject horizontal5SpecialPrefab;
    public GameObject vertical5SpecialPrefab;
    public GameObject square4SpecialPrefab;
    public GameObject horizontal4SpecialPrefab;
    public GameObject vertical4SpecialPrefab;

    [Header("Board Dimensions")]
    public int width = 8;
    public int height = 9;
    public float spacing = 3f;
    public GameObject itemBackgroundPrefab;

    [Header("Hint Settings")]
    public float hintDelay = 5f;

    [Header("References")]
    public TaskManager taskManager;

    [HideInInspector] public GameObject[,] allBalloons;
    [HideInInspector] public float offsetX, offsetY;
    [HideInInspector] public BreakableBlockManager breakableManager;

    private void Awake()
    {
        allBalloons = new GameObject[width, height];
        offsetX = -(width - 1) * spacing / 2f;
        offsetY = -(height - 1) * spacing / 2f;

        breakableManager = GetComponent<BreakableBlockManager>();
        if (breakableManager == null)
            Debug.LogError("BreakableBlockManager bulunamadı!");

        if (taskManager == null)
            taskManager = GetComponent<TaskManager>();
    }

    private void Start()
    {
        foreach (var g in glassTiles)
        {
            Vector3 pos = CellToWorld(g.x, g.y);
            Vector2Int gridPos = new Vector2Int(g.x, g.y);

            if (itemBackgroundPrefab != null)
            {
                var bg = Instantiate(itemBackgroundPrefab, pos, Quaternion.identity, transform);
                bg.transform.position = new Vector3(pos.x, pos.y, 1f);
            }

            var glass = Instantiate(glassPrefab, pos, Quaternion.identity, transform);
            glass.name = $"Glass_{g.x}_{g.y}";
            breakableManager.glassHealthDict[gridPos] = 2;

            if (IsInsideBounds(g.x, g.y) && allBalloons[g.x, g.y] == null)
            {
                int randIndex = Random.Range(0, balloonPrefabs.Length);
                var balloon = Instantiate(balloonPrefabs[randIndex], pos, Quaternion.identity, transform);
                var balloonScript = balloon.GetComponent<BalloonItem>();
                if (balloonScript != null)
                {
                    balloonScript.x = g.x;
                    balloonScript.y = g.y;
                    balloonScript.isFrozen = true;
                    Debug.Log($"✅ isFrozen (Glass) → ({g.x}, {g.y})");
                }
                allBalloons[g.x, g.y] = balloon;
            }
        }

        foreach (var b in boxTiles)
        {
            Vector3 pos = CellToWorld(b.x, b.y);
            Vector2Int gridPos = new Vector2Int(b.x, b.y);

            if (itemBackgroundPrefab != null)
            {
                var bg = Instantiate(itemBackgroundPrefab, pos, Quaternion.identity, transform);
                bg.transform.position = new Vector3(pos.x, pos.y, 1f);
            }

            var box = Instantiate(boxPrefab, pos, Quaternion.identity, transform);
            box.name = $"Box_{b.x}_{b.y}";
            breakableManager.boxHealthDict[gridPos] = 2;

            if (IsInsideBounds(b.x, b.y) && allBalloons[b.x, b.y] != null)
            {
                var balloon = allBalloons[b.x, b.y].GetComponent<BalloonItem>();
                if (balloon != null)
                {
                    balloon.isFrozen = true;
                    Debug.Log($"📦 isFrozen (Box) → ({b.x}, {b.y})");
                }
            }
        }
    }

    public Vector3 CellToWorld(int x, int y)
    {
        return new Vector3(
            offsetX + x * spacing,
            offsetY + y * spacing,
            0f
        );
    }

    public void DestroyMatchedItems(List<GameObject> matchedItems)
    {
        foreach (var item in matchedItems)
        {
            if (item == null) continue;
            taskManager.OnItemDestroyed(item);
            Destroy(item);
        }
    }

    private bool IsInsideBounds(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }
}
