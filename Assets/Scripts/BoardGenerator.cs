// BoardGenerator.cs
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(GameBoard), typeof(MatchManager))]
public class BoardGenerator : MonoBehaviour
{
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

                Vector2 spawnPos = new Vector2(x * board.spacing + board.offsetX,
                                                y * board.spacing + board.offsetY);
                if (board.itemBackgroundPrefab != null)
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

                    //  E�er glass b�lgesindeyse kilitle
                    if (board.glassTiles.Exists(p => p.x == x && p.y == y))
                        bi.isFrozen = true;
                }

            }
        }
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
