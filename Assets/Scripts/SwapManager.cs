using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(GameBoard), typeof(HintManager))]
public class SwapManager : MonoBehaviour
{
    private GameBoard board;
    private HintManager hintManager;
    private MovesManager movesManager;
    private void Awake()
    {
        board = GetComponent<GameBoard>();
        hintManager = GetComponent<HintManager>();
        movesManager = FindFirstObjectByType<MovesManager>();
    }
    public GameObject focusEffectPrefab;
    public GameObject explosionEffectPrefab;
    public void SwapBalloons(int x1, int y1, int x2, int y2)
    {
        hintManager.ResetIdleTimer();

        var b1 = board.allBalloons[x1, y1];
        var b2 = board.allBalloons[x2, y2];
        if (b1 == null || b2 == null) return;

        // ❄️ Freeze check: eğer biri donuksa, swap yapma
        var bi1 = b1.GetComponent<BalloonItem>();
        var bi2 = b2.GetComponent<BalloonItem>();
        if ((bi1 != null && bi1.isFrozen) || (bi2 != null && bi2.isFrozen))
        {
            Debug.Log($"🚫 Swap iptal: Balonlardan biri frozen → ({x1},{y1}) or ({x2},{y2})");
            return;
        }

        // Swap işlemi
        board.allBalloons[x1, y1] = b2;
        board.allBalloons[x2, y2] = b1;

        bi1.x = x2; bi1.y = y2;
        bi2.x = x1; bi2.y = y1;

        bi1.MoveTo(new Vector3(x2 * board.spacing + board.offsetX,
                               y2 * board.spacing + board.offsetY, 0));
        bi2.MoveTo(new Vector3(x1 * board.spacing + board.offsetX,
                               y1 * board.spacing + board.offsetY, 0));

        StartCoroutine(HandlePostSwap(b1, b2, x1, y1, x2, y2));
    }

    private IEnumerator HandlePostSwap(GameObject b1, GameObject b2, int x1, int y1, int x2, int y2)
    {
        yield return new WaitForSeconds(0.25f); // Swap animasyonu süresi

        // ✅ Destroy() edilmiş objeye erişmeye çalışma
        if (b1 == null || b2 == null || !b1 || !b2)
            yield break;

        SpecialItem s1 = b1.GetComponent<SpecialItem>();
        SpecialItem s2 = b2.GetComponent<SpecialItem>();

        bool specialTriggered = false;

        if (s1 != null)
        {
            board.allBalloons[x2, y2] = null;
            StartCoroutine(TriggerSpecial(s1, x2, y2, b2 != null ? b2.tag : null)); // tag çekilemezse null olur
            Destroy(b1);
            specialTriggered = true;
            if (movesManager != null)
                movesManager.UseMove();
        }

        if (s2 != null)
        {
            board.allBalloons[x1, y1] = null;
            StartCoroutine(TriggerSpecial(s2, x1, y1, b1 != null ? b1.tag : null));
            Destroy(b2);
            specialTriggered = true;
            if (movesManager != null)
                movesManager.UseMove();
        }

        if (!specialTriggered)
        {
            StartCoroutine(CheckMatchAfterSwap(x1, y1, x2, y2));
        }
    }



    private IEnumerator TriggerSpecial(SpecialItem item, int x, int y, string targetTag = null)
    {
        var toDestroy = new List<GameObject>();

        // Yardımcı: pozisyona göre camı kırma/düşürme kontrolü
        bool TryHandleGlassAt(int i, int j)
        {
            var balloon = board.allBalloons[i, j];
            if (balloon == null) return false;

            var bi = balloon.GetComponent<BalloonItem>();
            if (bi != null && bi.isFrozen)
            {
                var gridPos = new Vector2Int(i, j);
                if (board.breakableManager.glassHealthDict.TryGetValue(gridPos, out int health))
                {
                    health -= 1;
                    board.breakableManager.glassHealthDict[gridPos] = health;
                    Debug.Log($"🔨 Cam kırılıyor at {gridPos}, kalan can: {health}");

                    if (health <= 0)
                    {
                        // 1) Sözlükten çıkar
                        board.breakableManager.glassHealthDict.Remove(gridPos);

                        // 2) Sahnedeki Glass objesini bulup yok et
                        var glassObj = GameObject.Find($"Glass_{i}_{j}");
                        if (glassObj != null)
                        {
                            Destroy(glassObj);
                            Debug.Log($"🗑️ Glass_{i}_{j} sahneden silindi.");
                        }

                        // 3) Balonu serbest bırak
                        bi.isFrozen = false;
                        Debug.Log($"✅ Cam yok! Balon ({i},{j}) artık serbest.");
                    }
                }
                // Cam olan hücre için patlatmayı atla
                return true;
            }

            return false;
        }

        // Horizontal-4 special
        if (item.state == SpecialItem.SpecialState.Horizontal4)
        {
            for (int i = 0; i < board.width; i++)
            {
                if (TryHandleGlassAt(i, y)) continue;
                if (TriggerAnotherSpecialIfExists(i, y)) continue;
                var b = board.allBalloons[i, y];
                if (b != null)
                {
                    // Zincirleme kontrolü
                    var special = b.GetComponent<SpecialItem>();
                    if (special != null && special != item) // kendini tekrar tetikleme
                    {
                        board.allBalloons[i, y] = null;
                        StartCoroutine(TriggerSpecial(special, i, y));
                        Destroy(b);
                        continue;
                    }
                    if (focusEffectPrefab != null)
                    {
                        GameObject fx = Instantiate(focusEffectPrefab, b.transform.position, Quaternion.identity);
                        Destroy(fx, 1f);
                    }
                    toDestroy.Add(b);
                    board.allBalloons[i, y] = null;
                }
            }
        }
        else if (item.state == SpecialItem.SpecialState.Vertical4)
        {


            for (int j = 0; j < board.height; j++)
            {
                // Bu sırayla kontrol et, tüm sütun için çalışması için
                if (x < 0 || x >= board.width || j < 0 || j >= board.height)
                    continue;

                if (TryHandleGlassAt(x, j)) continue;
                if (TriggerAnotherSpecialIfExists(x, j)) continue;

                var b = board.allBalloons[x, j];
                if (b != null)
                {
                    var special = b.GetComponent<SpecialItem>();
                    if (special != null && special != item)
                    {
                        board.allBalloons[x, j] = null;
                        StartCoroutine(TriggerSpecial(special, x, j));
                        Destroy(b);
                        continue;
                    }
                    if (focusEffectPrefab != null)
                    {
                        GameObject fx = Instantiate(focusEffectPrefab, b.transform.position, Quaternion.identity);
                        Destroy(fx, 1f);
                    }
                    toDestroy.Add(b);
                    board.allBalloons[x, j] = null;
                }
            }
        }
        // Block Blaster (Square4) special
        else if (item.state == SpecialItem.SpecialState.Square4)
        {
            int radius = 1;
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int tx = x + dx, ty = y + dy;
                    if (tx < 0 || tx >= board.width || ty < 0 || ty >= board.height)
                        continue;
                    if (TryHandleGlassAt(tx, ty)) continue;
                    if (TriggerAnotherSpecialIfExists(tx, ty)) continue;
                    var b = board.allBalloons[tx, ty];
                    if (b != null)
                    {
                        if (focusEffectPrefab != null)
                        {
                            GameObject fx = Instantiate(focusEffectPrefab, b.transform.position, Quaternion.identity);
                            Destroy(fx, 1f);
                        }
                        toDestroy.Add(b);
                        board.allBalloons[tx, ty] = null;
                    }
                }
            }
        }
        // Block destreyer (Vertical5) special
        else if (item.state == SpecialItem.SpecialState.Vertical5)
        {
            List<Vector2Int> allBlockPositions = new List<Vector2Int>();

            // Cam blokları topla
            foreach (var entry in board.breakableManager.glassHealthDict)
                if (entry.Value > 0) allBlockPositions.Add(entry.Key);

            // Box blokları topla
            foreach (var entry in board.breakableManager.boxHealthDict)
                if (entry.Value > 0) allBlockPositions.Add(entry.Key);

            // 3 kez hasar gönder
            for (int i = 0; i < 3; i++)
            {
                if (allBlockPositions.Count == 0) break;

                int index = Random.Range(0, allBlockPositions.Count);
                Vector2Int target = allBlockPositions[index];

                board.breakableManager.TryDamageBlock(target); // ✅ Merkezden yönettiğin fonksiyon
                allBlockPositions.RemoveAt(index); // Aynı yere tekrar vurma
            }
        }

        // Color-clear (Horizontal5) special
        else if (item.state == SpecialItem.SpecialState.Horizontal5 && targetTag != null)
        {
            for (int i = 0; i < board.width; i++)
            {
                for (int j = 0; j < board.height; j++)
                {
                    if (TryHandleGlassAt(i, j)) continue;

                    var b = board.allBalloons[i, j];
                    if (b != null && b.tag == targetTag)
                    {
                        var bi = b.GetComponent<BalloonItem>();
                        if (bi != null && bi.isFrozen)
                            continue; // ❄️ Frozen balon → yok etme
                        if (focusEffectPrefab != null)
                        {
                            GameObject fx = Instantiate(focusEffectPrefab, b.transform.position, Quaternion.identity);
                            Destroy(fx, 1f);
                        }
                        toDestroy.Add(b);
                        board.allBalloons[i, j] = null;
                    }
                }
            }
        }

        // Klasik 3’lük/4’lük/5’lik match temizlemeleri bu metotta değil
        // Yalnızca special ile yok edilecekler
        if (toDestroy.Count > 0)
        {
            int multiplier = board.scoreMultiplier > 0 ? board.scoreMultiplier : 1;
            ScoreManager.Instance.AddScore(toDestroy.Count * multiplier);
        }

        foreach (var obj in toDestroy)
        {
            if (explosionEffectPrefab != null)
            {
                GameObject fx = Instantiate(explosionEffectPrefab, obj.transform.position, Quaternion.identity);
                Destroy(fx, 1f); // efekt 1 saniye sonra kaybolur
            }

            Destroy(obj);
        }

        yield return new WaitForSeconds(0.4f);

        GetComponent<DropManager>().DropBalloons();
    }


    private bool TriggerAnotherSpecialIfExists(int x, int y)
    {
        var b = board.allBalloons[x, y];
        if (b == null) return false;

        var special = b.GetComponent<SpecialItem>();
        if (special != null)
        {
            board.allBalloons[x, y] = null;

            if (focusEffectPrefab != null)
            {
                GameObject fx = Instantiate(focusEffectPrefab, b.transform.position, Quaternion.identity);
                Destroy(fx, 1f);
            }

            if (explosionEffectPrefab != null)
            {
                GameObject fx = Instantiate(explosionEffectPrefab, b.transform.position, Quaternion.identity);
                Destroy(fx, 1f);
            }

            StartCoroutine(TriggerSpecial(special, x, y));
            Destroy(b);
            return true;
        }

        return false;
    }
    private IEnumerator CheckMatchAfterSwap(int x1, int y1, int x2, int y2)
    {
        yield return new WaitForSeconds(0.3f);
        var matchManager = GetComponent<MatchManager>();

        if (matchManager.CheckAndClearMatches())
        {
            if (movesManager != null)
                movesManager.UseMove();
        }
        else
        {
            SwapWithoutCheck(x1, y1, x2, y2);
        }
    }

    private void SwapWithoutCheck(int x1, int y1, int x2, int y2)
    {
        var b1 = board.allBalloons[x1, y1];
        var b2 = board.allBalloons[x2, y2];
        if (b1 == null || b2 == null) return;

        var i1 = b1.GetComponent<BalloonItem>();
        var i2 = b2.GetComponent<BalloonItem>();

        board.allBalloons[x1, y1] = b2;
        board.allBalloons[x2, y2] = b1;
        i1.x = x2; i1.y = y2;
        i2.x = x1; i2.y = y1;

        i1.MoveTo(new Vector3(x2 * board.spacing + board.offsetX,
                               y2 * board.spacing + board.offsetY, 0));
        i2.MoveTo(new Vector3(x1 * board.spacing + board.offsetX,
                               y1 * board.spacing + board.offsetY, 0));
    }
}
