// DropManager.cs
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
                if (board.blockedPositions.Exists(p => p.x == x && p.y == y))
                    continue;

                var current = board.allBalloons[x, y];
                var balloon = current != null ? current.GetComponent<BalloonItem>() : null;

                // 🧱 Eğer cam varsa ve hâlâ sahnedeyse, bu item yerinde sabit kalmalı
                if (board.glassHealthDict.ContainsKey(new Vector2Int(x, y)))
                    continue;

                // ❄️ Eğer bu balon cam içindeyse ve isFrozen true ise, hareket ettirme
                if (balloon != null && balloon.isFrozen)
                    continue;

                if (board.allBalloons[x, y] == null)
                {
                    if (emptyY < 0) emptyY = y;
                }
                else if (emptyY >= 0)
                {
                    var obj = board.allBalloons[x, y];
                    board.allBalloons[x, emptyY] = obj;
                    board.allBalloons[x, y] = null;

                    var bi = obj.GetComponent<BalloonItem>();
                    Vector3 dest = new Vector3(x * board.spacing + board.offsetX,
                                               emptyY * board.spacing + board.offsetY, 0);

                    if (bi != null)
                    {
                        bi.x = x; bi.y = emptyY;
                        bi.MoveTo(dest);
                    }
                    else
                    {
                        obj.transform.position = dest;
                    }

                    emptyY++;
                    while (emptyY < board.height && board.blockedPositions.Exists(p => p.x == x && p.y == emptyY))
                        emptyY++;
                }
            }

            // 🧼 Spawn kısmı — cam varsa veya camın içindeki balon isFrozen ise, spawn etme
            for (int y = board.height - 1; y >= 0; y--)
            {
                var pos = new Vector2Int(x, y);
                var existing = board.allBalloons[x, y];
                bool isFrozenHere = existing != null && existing.GetComponent<BalloonItem>()?.isFrozen == true;

                if (existing == null &&
                    !board.blockedPositions.Exists(p => p.x == x && p.y == y) &&
                    !board.glassHealthDict.ContainsKey(pos) &&
                    !isFrozenHere)
                {
                    Vector3 spawnPos = new Vector3(x * board.spacing + board.offsetX,
                                                   (y + board.height) * board.spacing + board.offsetY, 0);
                    int r = Random.Range(0, board.balloonPrefabs.Length);
                    var nb = Instantiate(board.balloonPrefabs[r], spawnPos, Quaternion.identity, transform);
                    board.allBalloons[x, y] = nb;

                    var newBi = nb.GetComponent<BalloonItem>();
                    newBi.x = x; newBi.y = y;
                    newBi.MoveTo(new Vector3(x * board.spacing + board.offsetX,
                                              y * board.spacing + board.offsetY, 0));
                }
            }
        }

        StartCoroutine(ClearAfterFall());
    }

    private IEnumerator ClearAfterFall()
    {
        yield return new WaitForSeconds(0.4f);
        if (matchManager.CheckAndClearMatches())
            yield return new WaitForSeconds(0.2f);
        else
            GetComponent<BoardGenerator>().StartCheckBoardHasMoves();
    }
}