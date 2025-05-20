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
    [Header("Breakable Tile Prefabs")]
    public GameObject glassPrefab;
    public GameObject boxPrefab;

    [Header("Normal Balloon Prefabs")]
    public GameObject[] balloonPrefabs;

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

    [HideInInspector] public GameObject[,] allBalloons;
    [HideInInspector] public float offsetX, offsetY;

    // ➕ Glass canı için dictionary
    public Dictionary<Vector2Int, int> glassHealthDict = new Dictionary<Vector2Int, int>();

    private void Awake()
    {
        allBalloons = new GameObject[width, height];
        offsetX = -(width - 1) * spacing / 2f;
        offsetY = -(height - 1) * spacing / 2f;

        foreach (var box in boxTiles)
        {
            blockedPositions.Add(new BlockedPosition { x = box.x, y = box.y });
        }
    }

    private void Start()
    {
        // Glass tile'ları yerleştir
        foreach (var g in glassTiles)
        {
            Vector3 pos = CellToWorld(g.x, g.y);
            Vector2Int gridPos = new Vector2Int(g.x, g.y);

            // Arka plan
            if (itemBackgroundPrefab != null)
            {
                var bg = Instantiate(itemBackgroundPrefab, pos, Quaternion.identity, transform);
                bg.transform.position = new Vector3(pos.x, pos.y, 1f); // Z arkada
            }

            // Glass yerleştir
            var glass = Instantiate(glassPrefab, pos, Quaternion.identity, transform);
            glass.name = $"Glass_{g.x}_{g.y}";

            // Canı 2 olarak ata
            glassHealthDict[gridPos] = 2;

            // Sadece bu pozisyon boşsa içine balon koy
            if (allBalloons[g.x, g.y] == null)
            {
                int randIndex = Random.Range(0, balloonPrefabs.Length);
                var balloon = Instantiate(balloonPrefabs[randIndex], pos, Quaternion.identity, transform);
                var balloonScript = balloon.GetComponent<BalloonItem>();
                if (balloonScript != null)
                {
                    balloonScript.x = g.x;
                    balloonScript.y = g.y;
                    balloonScript.isFrozen = true;

                    Debug.Log($"✅ isFrozen şimdi ayarlandı! ({g.x}, {g.y}) → {balloonScript.isFrozen}");
                }
                allBalloons[g.x, g.y] = balloon;
            }
            else
            {
                Debug.LogWarning($"⚠️ Dikkat: ({g.x},{g.y}) pozisyonunda zaten balon var, ikinci kez spawn edilmedi.");
            }
        }

        // Box tile'ları yerleştir
        foreach (var b in boxTiles)
        {
            Vector3 pos = CellToWorld(b.x, b.y);

            if (itemBackgroundPrefab != null)
            {
                var bg = Instantiate(itemBackgroundPrefab, pos, Quaternion.identity, transform);
                bg.transform.position = new Vector3(pos.x, pos.y, 1f);
            }

            Instantiate(boxPrefab, pos, Quaternion.identity, transform);
        }
    }

    /// <summary>
    /// Grid hücresi (x,y)’u dünya-koordinata çevirir.
    /// </summary>
    public Vector3 CellToWorld(int x, int y)
    {
        return new Vector3(
            offsetX + x * spacing,
            offsetY + y * spacing,
            0f
        );
    }
}