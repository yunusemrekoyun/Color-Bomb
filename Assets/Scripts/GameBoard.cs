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

            // Arkaplan da yerleştir
            if (itemBackgroundPrefab != null)
            {
                var bg = Instantiate(itemBackgroundPrefab, pos, Quaternion.identity, transform);
                bg.transform.position = new Vector3(pos.x, pos.y, 1f); // Z arkada kalsın
            }

            Instantiate(glassPrefab, pos, Quaternion.identity, transform);
        }

        // Box tile'ları yerleştir
        foreach (var b in boxTiles)
        {
            Vector3 pos = CellToWorld(b.x, b.y);

            // Arkaplan da yerleştir
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
