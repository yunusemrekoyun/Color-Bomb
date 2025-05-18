// GameBoard.cs
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BlockedPosition { public int x, y; }

public class GameBoard : MonoBehaviour
{
    [Header("Normal Balloon Prefabs")]
    public GameObject[] balloonPrefabs;

    [Header("Blocked Positions")]
    public List<BlockedPosition> blockedPositions = new List<BlockedPosition>();

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