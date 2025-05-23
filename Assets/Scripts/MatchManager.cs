using System.Collections.Generic;
using System.Linq;
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
        {
            for (int y = 0; y < board.height; y++)
            {
                if (x < board.width - 1 && IsSwapMatch(x, y, x + 1, y))
                    return (new Vector2Int(x, y), new Vector2Int(x + 1, y));
                if (y < board.height - 1 && IsSwapMatch(x, y, x, y + 1))
                    return (new Vector2Int(x, y), new Vector2Int(x, y + 1));
            }
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

    public (Vector2Int from, Vector2Int to, List<GameObject> matchedItems)? GetBestValidSwapWithMatches()
    {
        (Vector2Int from, Vector2Int to, List<GameObject> matchedItems)? bestMatch = null;
        int bestMatchCount = 0;

        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                // Yatay dene
                if (x < board.width - 1 && TrySimulateSwap(x, y, x + 1, y, out List<GameObject> matched1))
                {
                    if (matched1.Count > bestMatchCount)
                    {
                        bestMatch = (new Vector2Int(x, y), new Vector2Int(x + 1, y), matched1);
                        bestMatchCount = matched1.Count;
                    }
                }

                // Dikey dene
                if (y < board.height - 1 && TrySimulateSwap(x, y, x, y + 1, out List<GameObject> matched2))
                {
                    if (matched2.Count > bestMatchCount)
                    {
                        bestMatch = (new Vector2Int(x, y), new Vector2Int(x, y + 1), matched2);
                        bestMatchCount = matched2.Count;
                    }
                }
            }
        }

        return bestMatch;
    }


    private bool TrySimulateSwap(int x1, int y1, int x2, int y2, out List<GameObject> matched)
    {
        matched = new List<GameObject>();

        var b1 = board.allBalloons[x1, y1];
        var b2 = board.allBalloons[x2, y2];
        if (b1 == null || b2 == null) return false;

        // 👉 eğer takas edeceğimiz balonlardan herhangi biri frozen ise hamle geçersiz
        var bi1 = b1.GetComponent<BalloonItem>();
        var bi2 = b2.GetComponent<BalloonItem>();
        if ((bi1 != null && bi1.isFrozen) || (bi2 != null && bi2.isFrozen))
            return false;

        // SpecialItem içeriyorsa zaten elimizde kontrol vardı:
        if (b1.GetComponent<SpecialItem>() != null || b2.GetComponent<SpecialItem>() != null)
            return false;

        // … ardından önceki simülasyon kodu değişmeden devam eder …
        board.allBalloons[x1, y1] = b2;
        board.allBalloons[x2, y2] = b1;

        matched = FindMatchesAt(x1, y1);
        matched.AddRange(FindMatchesAt(x2, y2));
        matched = new HashSet<GameObject>(matched).ToList();

        board.allBalloons[x1, y1] = b1;
        board.allBalloons[x2, y2] = b2;

        if (matched.Count < 3 || !AllSameTag(matched))
        {
            matched.Clear();
            return false;
        }

        return true;
    }

    private bool AllSameTag(List<GameObject> items)
    {
        if (items.Count == 0) return false;
        string tag = items[0].tag;
        foreach (var item in items)
        {
            if (item == null || item.tag != tag)
                return false;
        }
        return true;
    }

    private List<GameObject> FindMatchesAt(int x, int y)
    {
        var result = new List<GameObject>();
        var center = board.allBalloons[x, y];
        if (center == null) return result;

        string tag = center.tag;
        List<GameObject> horiz = new List<GameObject> { center };
        for (int i = x - 1; i >= 0 && board.allBalloons[i, y]?.tag == tag; i--)
            horiz.Add(board.allBalloons[i, y]);
        for (int i = x + 1; i < board.width && board.allBalloons[i, y]?.tag == tag; i++)
            horiz.Add(board.allBalloons[i, y]);
        if (horiz.Count >= 3)
            result.AddRange(horiz);

        List<GameObject> vert = new List<GameObject> { center };
        for (int i = y - 1; i >= 0 && board.allBalloons[x, i]?.tag == tag; i--)
            vert.Add(board.allBalloons[x, i]);
        for (int i = y + 1; i < board.height && board.allBalloons[x, i]?.tag == tag; i++)
            vert.Add(board.allBalloons[x, i]);
        if (vert.Count >= 3)
            result.AddRange(vert);

        // Eğer yatay ve dikey eşleşme yoksa boş liste dön
        if (result.Count < 3)
            result.Clear();

        return result;
    }

    public bool CheckAndClearMatches()
    {
        HashSet<GameObject> matchedSet = new HashSet<GameObject>();
        var allMatches = new List<(List<GameObject> items, int spawnX, int spawnY, GameObject specialPrefab)>();

        void AddMatch(List<GameObject> match, int spawnX, int spawnY, GameObject specialPrefab)
        {
            // ❄️ Eğer match içindeki herhangi bir balon frozen ise atla
            if (match.Any(obj => obj.GetComponent<BalloonItem>()?.isFrozen == true))
                return;

            // Eğer match’te special item varsa → bu eşleşmeyi iptal et (yok edilmesin)
            if (match.Any(obj => obj.GetComponent<SpecialItem>() != null))
                return;

            // Aynı objeyi tekrar eklememek için kontrol
            foreach (var obj in match)
                if (matchedSet.Contains(obj))
                    return;

            // ❌ Aynı pozisyonda daha önce special yerleştirilecekse, atla
            foreach (var existing in allMatches)
            {
                if (existing.spawnX == spawnX && existing.spawnY == spawnY)
                    return;
            }

            // Yeni match’i kaydet
            foreach (var obj in match)
                matchedSet.Add(obj);
            allMatches.Add((match, spawnX, spawnY, specialPrefab));
        }

        // Yatay ve dikey 5'li
        for (int y = 0; y < board.height; y++)
            for (int x = 0; x <= board.width - 5; x++)
                TryMatchLine(x, y, 1, 0, 5, board.horizontal5SpecialPrefab, AddMatch);
        for (int x = 0; x < board.width; x++)
            for (int y = 0; y <= board.height - 5; y++)
                TryMatchLine(x, y, 0, 1, 5, board.vertical5SpecialPrefab, AddMatch);

        // 2x2 Kare
        for (int x = 0; x < board.width - 1; x++)
            for (int y = 0; y < board.height - 1; y++)
            {
                var a = board.allBalloons[x, y];
                var b = board.allBalloons[x + 1, y];
                var c = board.allBalloons[x, y + 1];
                var d = board.allBalloons[x + 1, y + 1];
                if (a && b && c && d && a.tag == b.tag && a.tag == c.tag && a.tag == d.tag)
                {
                    var list = new List<GameObject> { a, b, c, d };
                    AddMatch(list, x + 1, y + 1, board.square4SpecialPrefab);
                }
            }

        // 4'lü eşleşmeler
        for (int y = 0; y < board.height; y++)
            for (int x = 0; x <= board.width - 4; x++)
                TryMatchLine(x, y, 1, 0, 4, board.horizontal4SpecialPrefab, AddMatch);
        for (int x = 0; x < board.width; x++)
            for (int y = 0; y <= board.height - 4; y++)
                TryMatchLine(x, y, 0, 1, 4, board.vertical4SpecialPrefab, AddMatch);

        // 3'lü eşleşmeler
        for (int y = 0; y < board.height; y++)
            for (int x = 0; x <= board.width - 3; x++)
                TryMatchLine(x, y, 1, 0, 3, null, AddMatch);
        for (int x = 0; x < board.width; x++)
            for (int y = 0; y <= board.height - 3; y++)
                TryMatchLine(x, y, 0, 1, 3, null, AddMatch);

        // Eşleşme varsa: merge ve skor işle
        if (allMatches.Count > 0)
        {
            foreach (var match in allMatches)
            {
                int scoreToAdd = 0;
                var prefab = match.specialPrefab;

                if (prefab == board.horizontal5SpecialPrefab || prefab == board.vertical5SpecialPrefab)
                    scoreToAdd = 30;
                else if (prefab == board.square4SpecialPrefab)
                    scoreToAdd = 25;
                else if (prefab == board.horizontal4SpecialPrefab || prefab == board.vertical4SpecialPrefab)
                    scoreToAdd = 20;
                else if (prefab == null && match.items.Count == 3)
                    scoreToAdd = 10;

                ScoreManager.Instance.AddScore(scoreToAdd);
                mergeManager.StartMerge(match.items, match.spawnX, match.spawnY, prefab);
            }
            return true;
        }

        return false;
    }
    private void TryMatchLine(int startX, int startY, int dx, int dy, int len,
                          GameObject specialPrefab,
                          System.Action<List<GameObject>, int, int, GameObject> onMatch)
    {
        var first = board.allBalloons[startX, startY];
        if (first == null) return;
        string tag = first.tag;

        for (int i = 1; i < len; i++)
        {
            var other = board.allBalloons[startX + dx * i, startY + dy * i];
            if (other == null || other.tag != tag)
                return;
        }

        var match = new List<GameObject>();
        for (int i = 0; i < len; i++)
            match.Add(board.allBalloons[startX + dx * i, startY + dy * i]);

        int spawnX = startX + dx * ((len - 1) / 2);
        int spawnY = startY + dy * ((len - 1) / 2);
        onMatch?.Invoke(match, spawnX, spawnY, specialPrefab);
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
