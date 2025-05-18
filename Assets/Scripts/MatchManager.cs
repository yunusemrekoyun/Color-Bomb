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

        board.allBalloons[x1, y1] = b2;
        board.allBalloons[x2, y2] = b1;

        matched = FindMatchesAt(x1, y1);
        matched.AddRange(FindMatchesAt(x2, y2));
        matched = new HashSet<GameObject>(matched).ToList();

        board.allBalloons[x1, y1] = b1;
        board.allBalloons[x2, y2] = b2;

        return matched.Count >= 3;
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
        if (horiz.Count >= 3) result.AddRange(horiz);

        List<GameObject> vert = new List<GameObject> { center };
        for (int i = y - 1; i >= 0 && board.allBalloons[x, i]?.tag == tag; i--)
            vert.Add(board.allBalloons[x, i]);
        for (int i = y + 1; i < board.height && board.allBalloons[x, i]?.tag == tag; i++)
            vert.Add(board.allBalloons[x, i]);
        if (vert.Count >= 3) result.AddRange(vert);

        return result;
    }

    public bool CheckAndClearMatches()
    {
        for (int y = 0; y < board.height; y++)
            for (int x = 0; x <= board.width - 5; x++)
                if (TryMatchLine(x, y, 1, 0, 5, MatchState.Horizontal5, board.horizontal5SpecialPrefab))
                    return true;

        for (int x = 0; x < board.width; x++)
            for (int y = 0; y <= board.height - 5; y++)
                if (TryMatchLine(x, y, 0, 1, 5, MatchState.Vertical5, board.vertical5SpecialPrefab))
                    return true;

        for (int x = 0; x < board.width - 1; x++)
        {
            for (int y = 0; y < board.height - 1; y++)
            {
                var a = board.allBalloons[x, y];
                var b = board.allBalloons[x + 1, y];
                var c = board.allBalloons[x, y + 1];
                var d = board.allBalloons[x + 1, y + 1];

                // hepsi var mý?
                if (a == null || b == null || c == null || d == null)
                    continue;

                // eðer herhangi biri special ise atla
                if (a.GetComponent<SpecialItem>() != null ||
                    b.GetComponent<SpecialItem>() != null ||
                    c.GetComponent<SpecialItem>() != null ||
                    d.GetComponent<SpecialItem>() != null)
                    continue;

                // tag’larý ayný mý?
                string tag = a.tag;
                if (b.tag != tag || c.tag != tag || d.tag != tag)
                    continue;

                // eþleþme bulundu
                currentState = MatchState.Square4;
                var items = new List<GameObject> { a, b, c, d };

                // spawn merkezi: ortadaki hücre
                int sx = x + 1, sy = y + 1;
                mergeManager.StartMerge(items, sx, sy, board.square4SpecialPrefab);
                currentState = MatchState.None;
                    return true;
                }
            }

        for (int y = 0; y < board.height; y++)
            for (int x = 0; x <= board.width - 4; x++)
                if (TryMatchLine(x, y, 1, 0, 4, MatchState.Horizontal4, board.horizontal4SpecialPrefab))
                    return true;

        for (int x = 0; x < board.width; x++)
            for (int y = 0; y <= board.height - 4; y++)
                if (TryMatchLine(x, y, 0, 1, 4, MatchState.Vertical4, board.vertical4SpecialPrefab))
                    return true;

        for (int y = 0; y < board.height; y++)
            for (int x = 0; x <= board.width - 3; x++)
                if (TryMatchLine(x, y, 1, 0, 3, MatchState.Horizontal3, null))
                    return true;

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
        // eðer ilk obje special ise atla
        if (first.GetComponent<SpecialItem>() != null)
            return false;

        string tag = first.tag;

        // diðer taþlar da dolu, ayný tag’te ve special deðil mi?
        for (int i = 1; i < len; i++)
        {
            var go = board.allBalloons[startX + dx * i, startY + dy * i];
            if (go == null) return false;
            if (go.GetComponent<SpecialItem>() != null) return false;
            if (go.tag != tag) return false;
        }

        // eþleþme bulundu
        currentState = state;
        var items = new List<GameObject>();
        for (int i = 0; i < len; i++)
            items.Add(board.allBalloons[startX + dx * i, startY + dy * i]);

        int cx = startX + dx * ((len - 1) / 2);
        int cy = startY + dy * ((len - 1) / 2);
        mergeManager.StartMerge(items, cx, cy, specialPrefab);

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
