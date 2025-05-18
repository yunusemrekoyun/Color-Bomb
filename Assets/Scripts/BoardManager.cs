using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    // --- State Tanımı ---------------------------------------------------
    public enum MatchState
    {
        None,
        Horizontal5,
        Vertical5,
        Square4,      // 2x2 kare
        Horizontal4,
        Vertical4,
        Horizontal3,
        Vertical3,
        Unknown
    }

    private MatchState currentState = MatchState.None;

    [System.Serializable]
    public struct BlockedPosition { public int x, y; }

    [Header("Normal Balloon Prefabs")]
    public GameObject[] balloonPrefabs;
    public List<BlockedPosition> blockedPositions = new List<BlockedPosition>();

    [Header("Special Item Prefabs")]
    public GameObject horizontal5SpecialPrefab;
    public GameObject vertical5SpecialPrefab;
    public GameObject square4SpecialPrefab;
    public GameObject horizontal4SpecialPrefab;
    public GameObject vertical4SpecialPrefab;

    public int width = 8;
    public int height = 9;
    public float spacing = 3f;
    public GameObject itemBackgroundPrefab;

    private GameObject[,] allBalloons;
    private float offsetX, offsetY;

    private float idleTimer = 0f;
    public float hintDelay = 5f;

    // Son sürüklenen hedef koordinatı
    private Vector2Int lastDragTarget = new Vector2Int(-1, -1);

    void Start()
    {
        allBalloons = new GameObject[width, height];
        offsetX = -(width - 1) * spacing / 2f;
        offsetY = -(height - 1) * spacing / 2f;

        GenerateBoard();
        StartCoroutine(InitialClear());
        ResetIdleTimer();
        ValidateBlockedPositions();
    }

    void Update()
    {
        idleTimer += Time.deltaTime;
        if (idleTimer >= hintDelay)
        {
            ShowHint();
            idleTimer = 0f;
        }
    }

    public void ResetIdleTimer() => idleTimer = 0f;

    private void ShowHint()
    {
        var match = FindFirstValidSwap();
        if (match.HasValue)
        {
            HighlightBalloon(match.Value.Item1.x, match.Value.Item1.y);
            HighlightBalloon(match.Value.Item2.x, match.Value.Item2.y);
        }
    }

    private void HighlightBalloon(int x, int y)
    {
        var b = allBalloons[x, y];
        if (b != null)
        {
            var sr = b.GetComponent<SpriteRenderer>();
            if (sr != null)
                StartCoroutine(HintBlink(sr));
        }
    }

    private IEnumerator HintBlink(SpriteRenderer sr)
    {
        var tf = sr.transform;
        var originalScale = tf.localScale;
        for (int i = 0; i < 3; i++)
        {
            tf.localScale = originalScale * 1.2f;
            yield return new WaitForSeconds(0.2f);
            tf.localScale = originalScale;
            yield return new WaitForSeconds(0.2f);
        }
    }

    private (Vector2Int, Vector2Int)? FindFirstValidSwap()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (x < width - 1 && IsSwapMatch(x, y, x + 1, y))
                    return (new Vector2Int(x, y), new Vector2Int(x + 1, y));
                if (y < height - 1 && IsSwapMatch(x, y, x, y + 1))
                    return (new Vector2Int(x, y), new Vector2Int(x, y + 1));
            }
        return null;
    }

    private bool IsSwapMatch(int x1, int y1, int x2, int y2)
    {
        var b1 = allBalloons[x1, y1];
        var b2 = allBalloons[x2, y2];
        if (b1 == null || b2 == null) return false;

        allBalloons[x1, y1] = b2;
        allBalloons[x2, y2] = b1;

        bool result = CheckMatchExists(x1, y1) || CheckMatchExists(x2, y2);

        allBalloons[x1, y1] = b1;
        allBalloons[x2, y2] = b2;
        return result;
    }

    private bool CheckMatchExists(int x, int y)
    {
        var center = allBalloons[x, y];
        if (center == null) return false;
        string tag = center.tag;

        // Horizontal
        int count = 1;
        for (int i = x - 1; i >= 0 && allBalloons[i, y]?.tag == tag; i--) count++;
        for (int i = x + 1; i < width && allBalloons[i, y]?.tag == tag; i++) count++;
        if (count >= 3) return true;

        // Vertical
        count = 1;
        for (int i = y - 1; i >= 0 && allBalloons[x, i]?.tag == tag; i--) count++;
        for (int i = y + 1; i < height && allBalloons[x, i]?.tag == tag; i++) count++;
        if (count >= 3) return true;

        return false;
    }

    private IEnumerator InitialClear()
    {
        yield return new WaitForSeconds(0.2f);
        while (CheckAndClearMatches())
            yield return new WaitForSeconds(0.5f);
    }

    void ValidateBlockedPositions()
    {
        foreach (var pos in blockedPositions)
            if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height)
                Debug.LogWarning($"❗Blocked position ({pos.x},{pos.y}) is outside bounds.");
    }

    void GenerateBoard()
    {
        StartCoroutine(CheckBoardHasMoves());
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (blockedPositions.Exists(p => p.x == x && p.y == y)) continue;

                var spawnPos = new Vector2(x * spacing + offsetX, y * spacing + offsetY);
                if (itemBackgroundPrefab != null)
                {
                    var bg = Instantiate(itemBackgroundPrefab, spawnPos, Quaternion.identity, transform);
                    bg.transform.position = new Vector3(spawnPos.x, spawnPos.y, 1f);
                }

                int r = Random.Range(0, balloonPrefabs.Length);
                var b = Instantiate(balloonPrefabs[r], spawnPos, Quaternion.identity, transform);
                allBalloons[x, y] = b;
                var bs = b.GetComponent<BalloonItem>();
                if (bs != null) { bs.x = x; bs.y = y; }
            }
    }

    private IEnumerator CheckBoardHasMoves()
    {
        yield return new WaitForSeconds(0.1f);
        while (!HasAnyValidMoves())
        {
            Debug.Log("Geçerli hamle yok. Tahta karıştırılıyor...");
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (allBalloons[x, y] != null)
                        Destroy(allBalloons[x, y]);
            yield return new WaitForSeconds(0.1f);
            GenerateBoard();
            yield break;
        }
    }

    private bool HasAnyValidMoves() => FindFirstValidSwap().HasValue;

    public void SwapBalloons(int x1, int y1, int x2, int y2)
    {
        // Drag target’ı kaydet
        lastDragTarget = new Vector2Int(x2, y2);
        Debug.Log($"[SwapBalloons] lastDragTarget set to: {lastDragTarget}");

        var b1 = allBalloons[x1, y1];
        var b2 = allBalloons[x2, y2];
        if (b1 == null || b2 == null) return;

        // Swap işlemi
        allBalloons[x1, y1] = b2;
        allBalloons[x2, y2] = b1;
        var i1 = b1.GetComponent<BalloonItem>(); i1.x = x2; i1.y = y2;
        var i2 = b2.GetComponent<BalloonItem>(); i2.x = x1; i2.y = y1;

        i1.MoveTo(new Vector3(x2 * spacing + offsetX, y2 * spacing + offsetY, 0));
        i2.MoveTo(new Vector3(x1 * spacing + offsetX, y1 * spacing + offsetY, 0));

        StartCoroutine(CheckMatchAfterSwap(x1, y1, x2, y2));
    }

    private IEnumerator CheckMatchAfterSwap(int x1, int y1, int x2, int y2)
    {
        yield return new WaitForSeconds(0.3f);
        if (!CheckAndClearMatches())
            SwapWithoutCheck(x1, y1, x2, y2);
    }

    private void SwapWithoutCheck(int x1, int y1, int x2, int y2)
    {
        var b1 = allBalloons[x1, y1];
        var b2 = allBalloons[x2, y2];
        if (b1 == null || b2 == null) return;

        var i1 = b1.GetComponent<BalloonItem>();
        var i2 = b2.GetComponent<BalloonItem>();

        allBalloons[x1, y1] = b2;
        allBalloons[x2, y2] = b1;
        i1.x = x2; i1.y = y2;
        i2.x = x1; i2.y = y1;

        i1.MoveTo(new Vector3(x2 * spacing + offsetX, y2 * spacing + offsetY, 0));
        i2.MoveTo(new Vector3(x1 * spacing + offsetX, y1 * spacing + offsetY, 0));
    }

    // --- Yeni Match-Checking ---------------------------------------------------
    private bool CheckAndClearMatches()
    {
        // Horizontal 5
        for (int y = 0; y < height; y++)
            for (int x = 0; x <= width - 5; x++)
                if (TryMatchLine(x, y, 1, 0, 5, MatchState.Horizontal5, "5li yatay patladı", horizontal5SpecialPrefab))
                    return true;

        // Vertical 5
        for (int x = 0; x < width; x++)
            for (int y = 0; y <= height - 5; y++)
                if (TryMatchLine(x, y, 0, 1, 5, MatchState.Vertical5, "5li dikey patladı", vertical5SpecialPrefab))
                    return true;

        // Square 4 (2x2)
        for (int x = 0; x < width - 1; x++)
            for (int y = 0; y < height - 1; y++)
            {
                if (allBalloons[x, y] != null &&
                    allBalloons[x + 1, y] != null &&
                    allBalloons[x, y + 1] != null &&
                    allBalloons[x + 1, y + 1] != null &&
                    allBalloons[x, y].tag == allBalloons[x + 1, y].tag &&
                    allBalloons[x, y].tag == allBalloons[x, y + 1].tag &&
                    allBalloons[x, y].tag == allBalloons[x + 1, y + 1].tag)
                {
                    currentState = MatchState.Square4;
                    Debug.Log("4lü kare patladı");

                    var items = new List<GameObject> {
                allBalloons[x, y],
                allBalloons[x + 1, y],
                allBalloons[x, y + 1],
                allBalloons[x + 1, y + 1]
            };

                    int spawnX = x + 1, spawnY = y + 1;
                    if (lastDragTarget.x >= 0 &&
                        ((lastDragTarget.x == x && lastDragTarget.y == y) ||
                         (lastDragTarget.x == x + 1 && lastDragTarget.y == y) ||
                         (lastDragTarget.x == x && lastDragTarget.y == y + 1) ||
                         (lastDragTarget.x == x + 1 && lastDragTarget.y == y + 1)))
                    {
                        spawnX = lastDragTarget.x;
                        spawnY = lastDragTarget.y;
                    }

                    // Special prefab artık buradan gönderiliyor
                    StartCoroutine(MergeAndDestroy(items, spawnX, spawnY, square4SpecialPrefab));

                    lastDragTarget = new Vector2Int(-1, -1);
                    currentState = MatchState.None;
                    return true;
                }
            }

        // Horizontal 4
        for (int y = 0; y < height; y++)
            for (int x = 0; x <= width - 4; x++)
                if (TryMatchLine(x, y, 1, 0, 4, MatchState.Horizontal4, "4lü yatay patladı", horizontal4SpecialPrefab))
                    return true;

        // Vertical 4
        for (int x = 0; x < width; x++)
            for (int y = 0; y <= height - 4; y++)
                if (TryMatchLine(x, y, 0, 1, 4, MatchState.Vertical4, "4lü dikey patladı", vertical4SpecialPrefab))
                    return true;

        // Horizontal 3
        for (int y = 0; y < height; y++)
            for (int x = 0; x <= width - 3; x++)
                if (TryMatchLine(x, y, 1, 0, 3, MatchState.Horizontal3, "3lü yatay patladı", null))
                    return true;

        // Vertical 3
        for (int x = 0; x < width; x++)
            for (int y = 0; y <= height - 3; y++)
                if (TryMatchLine(x, y, 0, 1, 3, MatchState.Vertical3, "3lü dikey patladı", null))
                    return true;

        return false;
    }

    // Helper: düz çizgi match ve işlem
    // Helper: düz çizgi match, animasyon + spawn özel
    private bool TryMatchLine(int startX, int startY, int dx, int dy, int len,
                              MatchState state, string logMsg, GameObject specialPrefab)
    {
        var first = allBalloons[startX, startY];
        if (first == null) return false;
        
        string tag = first.tag;
        for (int i = 1; i < len; i++)
            if (allBalloons[startX + dx * i, startY + dy * i]?.tag != tag)
                return false;

        currentState = state;
        Debug.Log(logMsg);

        // Eşleşen objeleri topla
        var items = new List<GameObject>();
        for (int i = 0; i < len; i++)
            items.Add(allBalloons[startX + dx * i, startY + dy * i]);

        // Spawn pozisyonu
        int centerX = startX + dx * ((len - 1) / 2);
        int centerY = startY + dy * ((len - 1) / 2);
        int spawnX = centerX, spawnY = centerY;

        // Eğer drag edilen hedef bu eşleşme içindeyse, ona yönel
        if (lastDragTarget.x >= 0)
        {
            for (int i = 0; i < len; i++)
            {
                int xi = startX + dx * i;
                int yi = startY + dy * i;
                if (xi == lastDragTarget.x && yi == lastDragTarget.y)
                {
                    spawnX = xi; spawnY = yi;
                    Debug.Log($"[TryMatchLine] spawnX/Y override with lastDragTarget: ({spawnX}, {spawnY})");

                    break;
                }
            }
        }
        Debug.Log($"[TryMatchLine] Final spawnX/Y: ({spawnX}, {spawnY})");

        // Animasyon & Destroy
        // StartCoroutine(MergeAndDestroy(items, spawnX, spawnY));

        // // Spawn special varsa
        // if (specialPrefab != null)
        //     SpawnSpecial(specialPrefab, spawnX, spawnY);
        StartCoroutine(MergeAndDestroy(items, spawnX, spawnY, specialPrefab));
        lastDragTarget = new Vector2Int(-1, -1);
        currentState = MatchState.None;
        return true;
    }


    // Yeni helper: special item spawn
    private void SpawnSpecial(GameObject prefab, int x, int y)
    {
        Debug.Log($"[SpawnSpecial] Special item spawn ediliyor → Grid: ({x}, {y})");

        if (prefab == null) return;

        Vector3 worldPos = new Vector3(x * spacing + offsetX, y * spacing + offsetY, 0);
        var special = Instantiate(prefab, worldPos, Quaternion.identity, transform);

        allBalloons[x, y] = special;

        // BalloonItem bileşeni yoksa ekle
        var balloon = special.GetComponent<BalloonItem>();
        if (balloon == null)
            balloon = special.AddComponent<BalloonItem>();

        // Konum bilgisi ver ve yerleştir
        balloon.x = x;
        balloon.y = y;
        balloon.MoveTo(worldPos); // düzgün hizalama
    }
    // --- Merge ve Destroy ---------------------------------------------------
    // 1) MergeAndDestroy imzasını değiştir:
    private IEnumerator MergeAndDestroy(List<GameObject> items, int spawnX, int spawnY, GameObject specialPrefab = null)
    {
        // — Animasyon (aynı) —
        float duration = 0.2f, t = 0f;
        Vector3 target = new Vector3(spawnX * spacing + offsetX, spawnY * spacing + offsetY, 0);
        Debug.Log($"[MergeAndDestroy] Animasyon hedefi: ({spawnX}, {spawnY}) - World Pos: {target}, ItemCount: {items.Count}");
        Vector3[] starts = new Vector3[items.Count];
        for (int i = 0; i < items.Count; i++)
            starts[i] = items[i].transform.position;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            for (int i = 0; i < items.Count; i++)
                items[i].transform.position = Vector3.Lerp(starts[i], target, t);
            yield return null;
        }

        // — Grid’den temizle (destroy’dan önce!) —
        foreach (var g in items)
        {
            var bi = g.GetComponent<BalloonItem>();
            if (bi != null)
                allBalloons[bi.x, bi.y] = null;
        }

        // — Sonra objeleri yok et —
        foreach (var g in items)
            Destroy(g);

        // — Şimdi special spawn et —
        if (specialPrefab != null)
        {
            Vector3 pos = new Vector3(spawnX * spacing + offsetX,
                                      spawnY * spacing + offsetY,
                                      0);
            var special = Instantiate(specialPrefab, pos, Quaternion.identity, transform);
            allBalloons[spawnX, spawnY] = special;
            // **BalloonItem ekle-işleme YOK**; böylece sürüklenmez.
        }

        yield return new WaitForSeconds(0.1f);
        DropBalloons();
    }


    private void DropBalloons()
    {
        for (int x = 0; x < width; x++)
        {
            int emptyY = -1;
            for (int y = 0; y < height; y++)
            {
                if (blockedPositions.Exists(p => p.x == x && p.y == y))
                    continue;

                if (allBalloons[x, y] == null)
                {
                    if (emptyY < 0) emptyY = y;
                }
                else if (emptyY >= 0)
                {
                    // Taşı
                    var obj = allBalloons[x, y];
                    allBalloons[x, emptyY] = obj;
                    allBalloons[x, y] = null;

                    // Eğer normal BalloonItem ise animasyonla taşı, değilse (special) doğrudan snap
                    var bi = obj.GetComponent<BalloonItem>();
                    Vector3 dest = new Vector3(x * spacing + offsetX, emptyY * spacing + offsetY, 0);
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
                    while (emptyY < height && blockedPositions.Exists(p => p.x == x && p.y == emptyY))
                        emptyY++;
                }
            }

            // Boşlardan yeni normal balon spawn et
            for (int y = height - 1; y >= 0; y--)
            {
                if (allBalloons[x, y] == null && !blockedPositions.Exists(p => p.x == x && p.y == y))
                {
                    Vector3 spawnPos = new Vector3(x * spacing + offsetX, (y + height) * spacing + offsetY, 0);
                    int r = Random.Range(0, balloonPrefabs.Length);
                    var nb = Instantiate(balloonPrefabs[r], spawnPos, Quaternion.identity, transform);
                    allBalloons[x, y] = nb;
                    var newBi = nb.GetComponent<BalloonItem>();
                    newBi.x = x;
                    newBi.y = y;
                    newBi.MoveTo(new Vector3(x * spacing + offsetX, y * spacing + offsetY, 0));
                }
            }
        }
        StartCoroutine(ClearAfterFall());
    }

    private IEnumerator ClearAfterFall()
    {
        yield return new WaitForSeconds(0.4f);

        if (CheckAndClearMatches())
        {
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            // ** Eğer artık hiç geçerli hamle yoksa karıştır **
            StartCoroutine(CheckBoardHasMoves());
        }
    }
}