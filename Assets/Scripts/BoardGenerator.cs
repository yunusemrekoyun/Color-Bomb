// ▶ 1) En üstteki using bölümü:
using System.Collections;              // (Zaten vardı)
using UnityEngine;
using System.Collections.Generic;       // ── Ekle: HashSet için

[RequireComponent(typeof(GameBoard), typeof(MatchManager))]
public class BoardGenerator : MonoBehaviour
{
    // ▶ 2) Class başında, herhangi bir metoda girmeden önce:
    //     Bu statik koleksiyon, başlangıçta kilitlenecek balonları saklayacak.
    public static HashSet<GameObject> lockedItems = new HashSet<GameObject>();

    private GameBoard board;
    private MatchManager matchManager;

    private void Awake()
    {
        board = GetComponent<GameBoard>();
        matchManager = GetComponent<MatchManager>();
    }

    public void StartCheckBoardHasMoves()
    {
        StartCoroutine(CheckBoardHasMoves());
    }

    private void Start()
    {
        GenerateBoard();
        StartCoroutine(InitialClear());
        ValidateBlockedPositions();
    }

    public void GenerateBoard()
    {
        StartCoroutine(CheckBoardHasMoves());

        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                if (board.blockedPositions.Exists(p => p.x == x && p.y == y)) continue;

                Vector2 spawnPos = new Vector2(
                    x * board.spacing + board.offsetX,
                    y * board.spacing + board.offsetY
                );

                if (board.itemBackgroundPrefab != null &&
                    !board.glassTiles.Exists(p => p.x == x && p.y == y) &&
                    !board.boxTiles.Exists(p => p.x == x && p.y == y))
                {
                    var bg = Instantiate(board.itemBackgroundPrefab, spawnPos, Quaternion.identity, transform);
                    bg.transform.position = new Vector3(spawnPos.x, spawnPos.y, 1f);
                }

                int r = Random.Range(0, board.balloonPrefabs.Length);
                var b = Instantiate(board.balloonPrefabs[r], spawnPos, Quaternion.identity, transform);
                board.allBalloons[x, y] = b;
                var bi = b.GetComponent<BalloonItem>();
                if (bi != null)
                {
                    bi.x = x;
                    bi.y = y;

                    // Eğer burada bir glass bölgesi varsa, o balonu başlangıçta donuk (kilitli) yap:
                    if (board.glassTiles.Exists(p => p.x == x && p.y == y))
                        bi.isFrozen = true;
                }
            }
        }

        // ▶ 3) Burası GenerateBoard() metodunun en sonunda eklenen kısım:
        //     Oyun başladığında, glass/box içine doğrudan düşen (kilitli) balonları "lockedItems" setine ekle.
        foreach (var pos in board.blockedPositions)
        {
            GameObject inner = board.allBalloons[pos.x, pos.y];
            if (inner != null)
            {
                lockedItems.Add(inner);
                Debug.Log($"🔒 Başlangıçta kilitlendi: ({pos.x},{pos.y}) → {inner.name}");
            }
        }
        // ──────────────────────────────────────────────────────────────────────────────
    }

    private IEnumerator CheckBoardHasMoves()
    {
        yield return new WaitForSeconds(0.1f);
        while (!matchManager.HasAnyValidMoves())
        {
            Debug.Log("No valid moves. Shuffling board...");
            for (int x = 0; x < board.width; x++)
                for (int y = 0; y < board.height; y++)
                    if (board.allBalloons[x, y] != null)
                        Destroy(board.allBalloons[x, y]);

            yield return new WaitForSeconds(0.1f);
            GenerateBoard();
            yield break;
        }
    }

    private IEnumerator InitialClear()
    {
        yield return new WaitForSeconds(0.2f);
        while (matchManager.CheckAndClearMatches())
            yield return new WaitForSeconds(0.5f);
    }

    private void ValidateBlockedPositions()
    {
        foreach (var pos in board.blockedPositions)
        {
            if (pos.x < 0 || pos.x >= board.width || pos.y < 0 || pos.y >= board.height)
                Debug.LogWarning($"Blocked position ({pos.x},{pos.y}) is outside bounds.");
        }
    }
}