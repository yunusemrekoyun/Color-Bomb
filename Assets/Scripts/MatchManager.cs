// MatchManager.cs
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GameBoard), typeof(MergeManager), typeof(SpecialItemSpawner))]
public class MatchManager : MonoBehaviour
{
    public enum MatchState { None, Horizontal5, Vertical5, Square4, Horizontal4, Vertical4, Horizontal3, Vertical3, Unknown }

    private MatchState currentState = MatchState.None;
    private GameBoard board;
    private MergeManager mergeManager;

    private void Awake()
    {
        board = GetComponent<GameBoard>();
        mergeManager = GetComponent<MergeManager>();
    }
    public (Vector2Int, Vector2Int)? GetFirstValidSwap()
    {
        return FindFirstValidSwap();
    }

    public bool HasAnyValidMoves() => FindFirstValidSwap().HasValue;

    private (Vector2Int, Vector2Int)? FindFirstValidSwap()
    {
        for (int x = 0; x < board.width; x++)
            for (int y = 0; y < board.height; y++)
            {
                if (x < board.width - 1 && IsSwapMatch(x, y, x + 1, y))
                    return (new Vector2Int(x, y), new Vector2Int(x + 1, y));
                if (y < board.height - 1 && IsSwapMatch(x, y, x, y + 1))
                    return (new Vector2Int(x, y), new Vector2Int(x, y + 1));
            }
        return null;
    }

    private bool IsSwapMatch(int x1, int y1, int x2, int y2)
    {
        var b1 = board.allBalloons[x1, y1];
        var b2 = board.allBalloons[x2, y2];
        if (b1 == null || b2 == null) return false;

        board.allBalloons[x1, y1] = b2;
        board.allBalloons[x2, y2] = b1;

        bool result = CheckMatchExists(x1, y1) || CheckMatchExists(x2, y2);

        board.allBalloons[x1, y1] = b1;
        board.allBalloons[x2, y2] = b2;
        return result;
    }

    public bool CheckAndClearMatches()
    {
        // Horizontal 5
        for (int y = 0; y < board.height; y++)
            for (int x = 0; x <= board.width - 5; x++)
                if (TryMatchLine(x, y, 1, 0, 5, MatchState.Horizontal5, board.horizontal5SpecialPrefab))
                    return true;
        // Vertical 5
        for (int x = 0; x < board.width; x++)
            for (int y = 0; y <= board.height - 5; y++)
                if (TryMatchLine(x, y, 0, 1, 5, MatchState.Vertical5, board.vertical5SpecialPrefab))
                    return true;
        // Square 4
        for (int x = 0; x < board.width - 1; x++)
            for (int y = 0; y < board.height - 1; y++)
            {
                if (board.allBalloons[x, y] == null) continue;
                string tag = board.allBalloons[x, y].tag;
                if (board.allBalloons[x + 1, y]?.tag == tag &&
                    board.allBalloons[x, y + 1]?.tag == tag &&
                    board.allBalloons[x + 1, y + 1]?.tag == tag)
                {
                    currentState = MatchState.Square4;
                    var items = new List<GameObject> {
                        board.allBalloons[x,y],
                        board.allBalloons[x+1,y],
                        board.allBalloons[x,y+1],
                        board.allBalloons[x+1,y+1]
                    };
                    int spawnX = x + 1, spawnY = y + 1;
                    mergeManager.StartMerge(items, spawnX, spawnY, board.square4SpecialPrefab);
                    currentState = MatchState.None;
                    return true;
                }
            }
        // Horizontal 4
        for (int y = 0; y < board.height; y++)
            for (int x = 0; x <= board.width - 4; x++)
                if (TryMatchLine(x, y, 1, 0, 4, MatchState.Horizontal4, board.horizontal4SpecialPrefab))
                    return true;
        // Vertical 4
        for (int x = 0; x < board.width; x++)
            for (int y = 0; y <= board.height - 4; y++)
                if (TryMatchLine(x, y, 0, 1, 4, MatchState.Vertical4, board.vertical4SpecialPrefab))
                    return true;
        // Horizontal 3
        for (int y = 0; y < board.height; y++)
            for (int x = 0; x <= board.width - 3; x++)
                if (TryMatchLine(x, y, 1, 0, 3, MatchState.Horizontal3, null))
                    return true;
        // Vertical 3
        for (int x = 0; x < board.width; x++)
            for (int y = 0; y <= board.height - 3; y++)
                if (TryMatchLine(x, y, 0, 1, 3, MatchState.Vertical3, null))
                    return true;

        return false;
    }

    private bool TryMatchLine(int startX, int startY, int dx, int dy, int len,
                              MatchState state, GameObject specialPrefab)
    {
        var first = board.allBalloons[startX, startY];
        if (first == null) return false;
        string tag = first.tag;
        for (int i = 1; i < len; i++)
            if (board.allBalloons[startX + dx * i, startY + dy * i]?.tag != tag)
                return false;

        currentState = state;
        var items = new List<GameObject>();
        for (int i = 0; i < len; i++)
            items.Add(board.allBalloons[startX + dx * i, startY + dy * i]);

        int centerX = startX + dx * ((len - 1) / 2);
        int centerY = startY + dy * ((len - 1) / 2);
        mergeManager.StartMerge(items, centerX, centerY, specialPrefab);
        currentState = MatchState.None;
        return true;
    }

    private bool CheckMatchExists(int x, int y)
    {
        var center = board.allBalloons[x, y];
        if (center == null) return false;
        string tag = center.tag;

        int count = 1;
        for (int i = x - 1; i >= 0 && board.allBalloons[i, y]?.tag == tag; i--) count++;
        for (int i = x + 1; i < board.width && board.allBalloons[i, y]?.tag == tag; i++) count++;
        if (count >= 3) return true;

        count = 1;
        for (int i = y - 1; i >= 0 && board.allBalloons[x, i]?.tag == tag; i--) count++;
        for (int i = y + 1; i < board.height && board.allBalloons[x, i]?.tag == tag; i++) count++;
        return count >= 3;
    }
}
