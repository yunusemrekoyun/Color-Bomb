// SpecialComboManager.cs
// Special kombinasyonları yönetir
// - Horizontal4 + Horizontal4 → 3 satır sil
// - Vertical4 + Vertical4 → 2 sütun sil
// - Horizontal4 + Vertical4 → 2 satır + 2 sütun sil
// - Square4 + Vertical4 → 3x3 alan + sütun
// - Square4 + Horizontal4 → 3x3 alan + satır
// - Square4 + Square4 → 5x5 büyük alan temizliği

using UnityEngine;
using System.Collections.Generic;

public static class SpecialComboManager
{
    public static bool TryHandleCombo(SpecialItem s1, SpecialItem s2, int x1, int y1, int x2, int y2, GameBoard board)
    {
        var type1 = s1.state;
        var type2 = s2.state;

        // Horizontal + Horizontal → 3 satır sil
        if (type1 == SpecialItem.SpecialState.Horizontal4 && type2 == SpecialItem.SpecialState.Horizontal4)
        {
            ClearRows(board, new int[] { y1, y2 });
            return true;
        }

        // Vertical + Vertical → 2 sütun sil
        if (type1 == SpecialItem.SpecialState.Vertical4 && type2 == SpecialItem.SpecialState.Vertical4)
        {
            ClearColumns(board, new int[] { x1, x2 });
            return true;
        }

        // Horizontal + Vertical → 2 satır + 2 sütun sil
        if ((type1 == SpecialItem.SpecialState.Horizontal4 && type2 == SpecialItem.SpecialState.Vertical4) ||
            (type1 == SpecialItem.SpecialState.Vertical4 && type2 == SpecialItem.SpecialState.Horizontal4))
        {
            ClearRows(board, new int[] { y1, y2 });
            ClearColumns(board, new int[] { x1, x2 });
            return true;
        }

        // Square + Vertical → 3x3 alan + sütun
        if ((type1 == SpecialItem.SpecialState.Square4 && type2 == SpecialItem.SpecialState.Vertical4) ||
            (type2 == SpecialItem.SpecialState.Square4 && type1 == SpecialItem.SpecialState.Vertical4))
        {
            ClearArea(board, x1, y1, 1);
            ClearColumns(board, new int[] { x1, x2 });
            return true;
        }

        // Square + Horizontal → 3x3 alan + satır
        if ((type1 == SpecialItem.SpecialState.Square4 && type2 == SpecialItem.SpecialState.Horizontal4) ||
            (type2 == SpecialItem.SpecialState.Square4 && type1 == SpecialItem.SpecialState.Horizontal4))
        {
            ClearArea(board, x1, y1, 1);
            ClearRows(board, new int[] { y1, y2 });
            return true;
        }

        // Square + Square → 5x5 alan
        if (type1 == SpecialItem.SpecialState.Square4 && type2 == SpecialItem.SpecialState.Square4)
        {
            ClearArea(board, x1, y1, 2);
            ClearArea(board, x2, y2, 2);
            return true;
        }
        // ──────────────────────────────────────────────────────────────
        // ColorClear + Vertical4 → rastgele renk Vertical4 prefab ile değiştir ve 0.2s sonra tetikle
        if ((type1 == SpecialItem.SpecialState.Horizontal5 && type2 == SpecialItem.SpecialState.Vertical4) ||
            (type2 == SpecialItem.SpecialState.Horizontal5 && type1 == SpecialItem.SpecialState.Vertical4))
        {
            var tags = new List<string> { "Red", "Green", "Blue", "Yellow", "Purple" };
            string targetTag = tags[Random.Range(0, tags.Count)];

            var spawner = GameObject.FindFirstObjectByType<SpecialItemSpawner>();
            var swap = GameObject.FindFirstObjectByType<SwapManager>();
            var prefab = board.vertical4SpecialPrefab;

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

                        // ← burada değişiklik:
                        if (swap != null)
                            swap.StartCoroutine(
                                swap.TriggerSpecialDelayed(
                                    specialObj.GetComponent<SpecialItem>(),
                                    x, y,
                                    null,
                                    0.2f
                                )
                            );
                    }
                }
            return true;
        }

        // ColorClear + Horizontal4 → rastgele renk Horizontal4 prefab ile değiştir ve tetikle
        if ((type1 == SpecialItem.SpecialState.Horizontal5 && type2 == SpecialItem.SpecialState.Horizontal4) ||
            (type2 == SpecialItem.SpecialState.Horizontal5 && type1 == SpecialItem.SpecialState.Horizontal4))
        {
            var tags = new List<string> { "Red", "Green", "Blue", "Yellow", "Purple" };
            string targetTag = tags[Random.Range(0, tags.Count)];

            var spawner = GameObject.FindFirstObjectByType<SpecialItemSpawner>();
            var swap = GameObject.FindFirstObjectByType<SwapManager>();
            var prefab = board.horizontal4SpecialPrefab;

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
                                x, y,
                                null,
                                0.2f     // burada ne kadar bekleyeceğini ayarlayabilirsin
                              )
                            );
                    }
                }

            return true;
        }
        // ColorClear + Square4 → rastgele renk Square4 prefab ile değiştir ve tetikle
        if ((type1 == SpecialItem.SpecialState.Horizontal5 && type2 == SpecialItem.SpecialState.Square4) ||
            (type2 == SpecialItem.SpecialState.Horizontal5 && type1 == SpecialItem.SpecialState.Square4))
        {
            var tags = new List<string> { "Red", "Green", "Blue", "Yellow", "Purple" };
            string targetTag = tags[Random.Range(0, tags.Count)];

            var spawner = GameObject.FindFirstObjectByType<SpecialItemSpawner>();
            var swap = GameObject.FindFirstObjectByType<SwapManager>();
            var prefab = board.square4SpecialPrefab;

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
                                x, y,
                                null,
                                0.2f     // burada ne kadar bekleyeceğini ayarlayabilirsin
                              )
                            );
                    }
                }

            return true;
        }

        // ColorClear + ColorClear → tüm board temizle
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
        // ──────────────────────────────────────────────────────────────
        return false; // İşlenmedi
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
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                int tx = cx + dx, ty = cy + dy;
                TryDestroy(board, tx, ty);
            }
        }
    }

    private static void TryDestroy(GameBoard board, int x, int y)
    {
        if (x < 0 || x >= board.width || y < 0 || y >= board.height) return;

        var b = board.allBalloons[x, y];
        if (b != null)
        {
            Object.Destroy(b);
            board.allBalloons[x, y] = null;
        }
    }
}
