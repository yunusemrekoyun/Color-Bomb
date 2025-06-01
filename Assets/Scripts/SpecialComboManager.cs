// Refactored version of SpecialComboManager.cs
// Split TryHandleCombo into modular, readable methods

using UnityEngine;
using System.Collections.Generic;

public static class SpecialComboManager
{
    public static bool TryHandleCombo(SpecialItem s1, SpecialItem s2, int x1, int y1, int x2, int y2, GameBoard board)
    {
        var type1 = s1.state;
        var type2 = s2.state;

        if (HandleBasicCombos(type1, type2, x1, y1, x2, y2, board)) return true;
        if (HandleColorClearCombos(type1, type2, board)) return true;

        return false;
    }

    private static bool HandleBasicCombos(SpecialItem.SpecialState type1, SpecialItem.SpecialState type2, int x1, int y1, int x2, int y2, GameBoard board)
    {
        if (type1 == SpecialItem.SpecialState.Horizontal4 && type2 == SpecialItem.SpecialState.Horizontal4)
        {
            ClearRows(board, new int[] { y1, y2 });
            return true;
        }
        if (type1 == SpecialItem.SpecialState.Vertical4 && type2 == SpecialItem.SpecialState.Vertical4)
        {
            ClearColumns(board, new int[] { x1, x2 });
            return true;
        }
        if ((type1 == SpecialItem.SpecialState.Horizontal4 && type2 == SpecialItem.SpecialState.Vertical4) ||
            (type1 == SpecialItem.SpecialState.Vertical4 && type2 == SpecialItem.SpecialState.Horizontal4))
        {
            ClearRows(board, new int[] { y1, y2 });
            ClearColumns(board, new int[] { x1, x2 });
            return true;
        }
        if ((type1 == SpecialItem.SpecialState.Square4 && type2 == SpecialItem.SpecialState.Vertical4) ||
            (type2 == SpecialItem.SpecialState.Square4 && type1 == SpecialItem.SpecialState.Vertical4))
        {
            ClearArea(board, x1, y1, 1);
            ClearColumns(board, new int[] { x1, x2 });
            return true;
        }
        if ((type1 == SpecialItem.SpecialState.Square4 && type2 == SpecialItem.SpecialState.Horizontal4) ||
            (type2 == SpecialItem.SpecialState.Square4 && type1 == SpecialItem.SpecialState.Horizontal4))
        {
            ClearArea(board, x1, y1, 1);
            ClearRows(board, new int[] { y1, y2 });
            return true;
        }
        if (type1 == SpecialItem.SpecialState.Square4 && type2 == SpecialItem.SpecialState.Square4)
        {
            ClearArea(board, x1, y1, 2);
            ClearArea(board, x2, y2, 2);
            return true;
        }
        return false;
    }

    private static bool HandleColorClearCombos(SpecialItem.SpecialState type1, SpecialItem.SpecialState type2, GameBoard board)
    {
        if ((type1 == SpecialItem.SpecialState.Horizontal5 && type2 == SpecialItem.SpecialState.Vertical4) ||
            (type2 == SpecialItem.SpecialState.Horizontal5 && type1 == SpecialItem.SpecialState.Vertical4))
        {
            return SpawnAndTriggerAll(board, board.vertical4SpecialPrefab);
        }
        if ((type1 == SpecialItem.SpecialState.Horizontal5 && type2 == SpecialItem.SpecialState.Horizontal4) ||
            (type2 == SpecialItem.SpecialState.Horizontal5 && type1 == SpecialItem.SpecialState.Horizontal4))
        {
            return SpawnAndTriggerAll(board, board.horizontal4SpecialPrefab);
        }
        if ((type1 == SpecialItem.SpecialState.Horizontal5 && type2 == SpecialItem.SpecialState.Square4) ||
            (type2 == SpecialItem.SpecialState.Horizontal5 && type1 == SpecialItem.SpecialState.Square4))
        {
            return SpawnAndTriggerAll(board, board.square4SpecialPrefab);
        }
        if (type1 == SpecialItem.SpecialState.Horizontal5 && type2 == SpecialItem.SpecialState.Horizontal5)
        {
            for (int x = 0; x < board.width; x++)
                for (int y = 0; y < board.height; y++)
                {
                    var b = board.allBalloons[x, y];
                    if (b != null) Object.Destroy(b);
                    board.allBalloons[x, y] = null;
                }
            return true;
        }
        return false;
    }

    private static bool SpawnAndTriggerAll(GameBoard board, GameObject prefab)
    {
        var tags = new List<string> { "Red", "Green", "Blue", "Yellow", "Purple" };
        string targetTag = tags[Random.Range(0, tags.Count)];

        var spawner = GameObject.FindFirstObjectByType<SpecialItemSpawner>();
        var swap = GameObject.FindFirstObjectByType<SwapManager>();

        for (int x = 0; x < board.width; x++)
            for (int y = 0; y < board.height; y++)
            {
                var b = board.allBalloons[x, y];
                if (b != null && b.tag == targetTag)
                {
                    Object.Destroy(b);
                    board.allBalloons[x, y] = null;

                    spawner.SpawnSpecial(prefab, x, y);
                    var specialObj = board.allBalloons[x, y];

                    if (swap != null)
                        swap.StartCoroutine(
                            swap.TriggerSpecialDelayed(
                                specialObj.GetComponent<SpecialItem>(),
                                x, y, null, 0.2f)
                        );
                }
            }
        return true;
    }

    private static void ClearRows(GameBoard board, int[] rows)
    {
        foreach (int y in rows)
        {
            for (int i = 0; i < board.width; i++)
            {
                TryDestroy(board, i, y);
                TryDestroy(board, i, y + 1);
                TryDestroy(board, i, y - 1);
            }
        }
    }

    private static void ClearColumns(GameBoard board, int[] cols)
    {
        foreach (int x in cols)
        {
            for (int j = 0; j < board.height; j++)
            {
                TryDestroy(board, x, j);
                TryDestroy(board, x + 1, j);
                TryDestroy(board, x - 1, j);
            }
        }
    }

    private static void ClearArea(GameBoard board, int cx, int cy, int radius)
    {
        for (int dx = -radius; dx <= radius; dx++)
            for (int dy = -radius; dy <= radius; dy++)
            {
                int tx = cx + dx, ty = cy + dy;
                TryDestroy(board, tx, ty);
            }
    }
    private static void TryDestroy(GameBoard board, int x, int y)
    {
        if (x < 0 || x >= board.width || y < 0 || y >= board.height) return;

        // Eğer kutu veya cam varsa sadece hasar ver, balona dokunma
        if (board.breakableManager != null && board.breakableManager.HasBlock(x, y))
        {
            board.breakableManager.TryDamageBlock(new Vector2Int(x, y));
            return;
        }

        var b = board.allBalloons[x, y];
        if (b != null)
        {
            Object.Destroy(b);
            board.allBalloons[x, y] = null;
        }
    }
}