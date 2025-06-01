using System.Collections;
using UnityEngine;

[RequireComponent(typeof(GameBoard), typeof(MatchManager))]
public class DropManager : MonoBehaviour
{
    private GameBoard board;
    private MatchManager matchManager;

    private void Awake()
    {
        board = GetComponent<GameBoard>();
        matchManager = GetComponent<MatchManager>();
    }

    public void DropBalloons()
    {
        for (int x = 0; x < board.width; x++)
        {
            int emptyY = -1;

            for (int y = 0; y < board.height; y++)
            {
                if (IsBlockedOrBreakable(x, y)) continue;

                var current = board.allBalloons[x, y];
                var balloon = current?.GetComponent<BalloonItem>();

                if (balloon != null && balloon.isFrozen)
                    continue;

                if (current == null)
                {
                    if (emptyY < 0) emptyY = y;
                }
                else if (emptyY >= 0)
                {
                    board.allBalloons[x, emptyY] = current;
                    board.allBalloons[x, y] = null;

                    var dest = CellToWorld(x, emptyY);

                    if (balloon != null)
                    {
                        balloon.x = x;
                        balloon.y = emptyY;
                        balloon.MoveTo(dest);
                    }
                    else
                    {
                        current.transform.position = dest;
                    }

                    emptyY++;
                    while (emptyY < board.height && board.blockedPositions.Exists(p => p.x == x && p.y == emptyY))
                        emptyY++;
                }
            }

            // Yeni balon spawn kısmı
            for (int y = board.height - 1; y >= 0; y--)
            {
                var pos = new Vector2Int(x, y);
                var existing = board.allBalloons[x, y];
                bool isFrozenHere = existing != null && existing.GetComponent<BalloonItem>()?.isFrozen == true;

                if (existing == null &&
                    !board.blockedPositions.Exists(p => p.x == x && p.y == y) &&
                    !board.breakableManager.glassHealthDict.ContainsKey(pos) &&
                    !board.breakableManager.boxHealthDict.ContainsKey(pos) &&
                    !isFrozenHere)
                {
                    Vector3 spawnPos = new Vector3(x * board.spacing + board.offsetX,
                                                   (y + board.height) * board.spacing + board.offsetY, 0);
                    int r = Random.Range(0, board.balloonPrefabs.Length);
                    var nb = Instantiate(board.balloonPrefabs[r], spawnPos, Quaternion.identity, transform);
                    board.allBalloons[x, y] = nb;

                    var newBi = nb.GetComponent<BalloonItem>();
                    if (newBi != null)
                    {
                        newBi.x = x;
                        newBi.y = y;
                        newBi.MoveTo(CellToWorld(x, y));
                    }
                }
            }
        }

        StartCoroutine(ClearAfterFallWithDelay());
    }

    private Vector3 CellToWorld(int x, int y)
    {
        return new Vector3(x * board.spacing + board.offsetX, y * board.spacing + board.offsetY, 0);
    }

    private bool IsBlockedOrBreakable(int x, int y)
    {
        var pos = new Vector2Int(x, y);
        return board.blockedPositions.Exists(p => p.x == x && p.y == y) ||
               board.breakableManager.glassHealthDict.ContainsKey(pos) ||
               board.breakableManager.boxHealthDict.ContainsKey(pos);
    }

    private IEnumerator ClearAfterFall()
    {
        yield return new WaitForSeconds(0.4f);

        if (matchManager.CheckAndClearMatches())
            yield return new WaitForSeconds(0.2f);
        else
            GetComponent<BoardGenerator>().StartCheckBoardHasMoves();
    }

    private IEnumerator ClearAfterFallWithDelay()
    {
        yield return new WaitForSeconds(0.6f);

        while (AreBalloonsMoving())
            yield return null;

        if (matchManager.CheckAndClearMatches())
            yield return new WaitForSeconds(0.2f);
        else
            GetComponent<BoardGenerator>().StartCheckBoardHasMoves();
    }

    private bool AreBalloonsMoving()
    {
        foreach (var b in board.allBalloons)
        {
            if (b == null) continue;

            var rb = b.GetComponent<Rigidbody2D>();
            if (rb != null && rb.linearVelocity.magnitude > 0.01f)
                return true;

            var bi = b.GetComponent<BalloonItem>();
            if (bi != null && bi.transform.hasChanged)
            {
                bi.transform.hasChanged = false;
                return true;
            }
        }
        return false;
    }
}