using System.Collections;
using System.Collections.Generic;  // ── HashSet<T> için
using UnityEngine;

[RequireComponent(typeof(GameBoard), typeof(MatchManager))]
public class DropManager : MonoBehaviour
{
    // “lockedItems” → BoardGenerator’da başlangıçta kaydettiğimiz, blok içindeki balonları tutuyoruz.
    private HashSet<GameObject> lockedItems => BoardGenerator.lockedItems;

    // “fallingItems” → Şu anda düşme animasyonuyla hareket eden balonları tutacak.
    private HashSet<GameObject> fallingItems = new HashSet<GameObject>();

    private void OnEnable()
    {
        BreakableBlockManager.OnBlockReleased += HandleBlockReleased;
    }

    private void OnDisable()
    {
        BreakableBlockManager.OnBlockReleased -= HandleBlockReleased;
    }

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
                Vector2Int pos = new Vector2Int(x, y);
                bool hasGlass = board.breakableManager.glassHealthDict.ContainsKey(pos);
                bool hasBox = board.breakableManager.boxHealthDict.ContainsKey(pos);

                Debug.Log($"[DropManager] Pos = ({x},{y}), hasGlass={hasGlass}, hasBox={hasBox}");

                if (board.blockedPositions.Exists(p => p.x == x && p.y == y))
                    continue;

                if (hasGlass || hasBox)
                    continue;

                var current = board.allBalloons[x, y];
                var balloon = current != null ? current.GetComponent<BalloonItem>() : null;

                // ❄️ Eğer bu balon donuksa veya kilitliyse, hareket ettirme
                if (balloon != null && (balloon.isFrozen || lockedItems.Contains(current)))
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
                    Vector3 dest = new Vector3(
                        x * board.spacing + board.offsetX,
                        emptyY * board.spacing + board.offsetY,
                        0
                    );

                    if (bi != null)
                    {
                        bi.x = x;
                        bi.y = emptyY;
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

            // 🧼 Spawn kısmı — cam/box var mı, frozen mı veya locked mı diye kontrol et
            for (int y = board.height - 1; y >= 0; y--)
            {
                var pos = new Vector2Int(x, y);
                var existing = board.allBalloons[x, y];
                bool isFrozenHere = existing != null && existing.GetComponent<BalloonItem>()?.isFrozen == true;
                bool isLockedHere = existing != null && lockedItems.Contains(existing);

                if (existing == null &&
                    !board.blockedPositions.Exists(p => p.x == x && p.y == y) &&
                    !board.breakableManager.glassHealthDict.ContainsKey(pos) &&
                    !board.breakableManager.boxHealthDict.ContainsKey(pos) &&
                    !isFrozenHere &&
                    !isLockedHere)  // ── lockedItems kontrolü eklendi
                {
                    Vector3 spawnPos = new Vector3(
                        x * board.spacing + board.offsetX,
                        (y + board.height) * board.spacing + board.offsetY,
                        0
                    );
                    int r = Random.Range(0, board.balloonPrefabs.Length);
                    var nb = Instantiate(board.balloonPrefabs[r], spawnPos, Quaternion.identity, transform);
                    board.allBalloons[x, y] = nb;

                    var newBi = nb.GetComponent<BalloonItem>();
                    newBi.x = x;
                    newBi.y = y;
                    newBi.MoveTo(new Vector3(
                        x * board.spacing + board.offsetX,
                        y * board.spacing + board.offsetY,
                        0
                    ));
                }
            }
        }

        StartCoroutine(ClearAfterFallWithDelay());
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
        yield return new WaitForSeconds(0.6f); // 🕓 Animasyonların inmesini bekle

        while (AreBalloonsMoving())
            yield return null;

        if (GetComponent<MatchManager>().CheckAndClearMatches())
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

    private void HandleBlockReleased(Vector2Int pos)
    {
        // Blok içindeki balon artık isFrozen = false oldu; 
        // DropBalloons() tetiklenerek hareket ettirilmesini sağlıyoruz.
        DropBalloons();
    }

    // ───────────────────────────────────────────────────────────────────
    // Aşağıdaki iki metot, BalloonItem’den çağrılan Register/Unregister işlemleri:
    public void RegisterFalling(GameObject balloon)
    {
        if (balloon == null) return;
        if (!fallingItems.Contains(balloon))
        {
            fallingItems.Add(balloon);
            Debug.Log($"⤵️ Falling started: {balloon.name}");
        }
    }

    public void UnregisterFalling(GameObject balloon)
    {
        if (balloon == null) return;
        if (fallingItems.Contains(balloon))
        {
            fallingItems.Remove(balloon);
            Debug.Log($"✅ Falling ended: {balloon.name}");
        }
    }
    // ───────────────────────────────────────────────────────────────────
}