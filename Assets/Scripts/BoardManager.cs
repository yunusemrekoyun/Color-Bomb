using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [System.Serializable]
    public struct BlockedPosition
    {
        public int x;
        public int y;
    }

    public List<BlockedPosition> blockedPositions = new List<BlockedPosition>();
    public GameObject[] balloonPrefabs;
    public int width = 8;
    public int height = 9;
    public float spacing = 3f;
    public GameObject itemBackgroundPrefab;
    private GameObject[,] allBalloons;

    private float idleTimer = 0f;
    public float hintDelay = 5f;

    void Start()
    {

        allBalloons = new GameObject[width, height];
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
    /*******************************
    TIMER START
    *******************************/
    public void ResetIdleTimer()
    {
        idleTimer = 0f;
    }

    /*******************************
    İPUCU GÖSTER FONKSİYONU
    *******************************/
    private void ShowHint()
    {
        var match = FindFirstValidSwap();
        if (match.HasValue)
        {
            Vector2Int a = match.Value.Item1;
            Vector2Int b = match.Value.Item2;
            HighlightBalloon(a.x, a.y);
            HighlightBalloon(b.x, b.y);
        }
    }

    /*******************************
    BALON HIGHLIGHT FONKSİYONU
    *******************************/
    private void HighlightBalloon(int x, int y)
    {
        GameObject b = allBalloons[x, y];
        if (b != null)
        {
            SpriteRenderer sr = b.GetComponent<SpriteRenderer>();
            if (sr != null)
                StartCoroutine(HintBlink(sr));
        }
    }


    /*******************************
    İPUCU BLINK
    *******************************/
    private IEnumerator HintBlink(SpriteRenderer sr)
    {
        Transform tf = sr.transform;
        Vector3 originalScale = tf.localScale;

        for (int i = 0; i < 3; i++)
        {
            tf.localScale = originalScale * 1.2f;
            yield return new WaitForSeconds(0.2f);
            tf.localScale = originalScale;
            yield return new WaitForSeconds(0.2f);
        }
    }

    /*******************************
    İLK GEÇERLİ HAMLEYİ BUL
    *******************************/
    private (Vector2Int, Vector2Int)? FindFirstValidSwap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x < width - 1 && IsSwapMatch(x, y, x + 1, y))
                    return (new Vector2Int(x, y), new Vector2Int(x + 1, y));

                if (y < height - 1 && IsSwapMatch(x, y, x, y + 1))
                    return (new Vector2Int(x, y), new Vector2Int(x, y + 1));
            }
        }
        return null;
    }
    /*******************************
    SWAP KONTROL
    *******************************/
    private bool IsSwapMatch(int x1, int y1, int x2, int y2)
    {
        GameObject b1 = allBalloons[x1, y1];
        GameObject b2 = allBalloons[x2, y2];
        if (b1 == null || b2 == null) return false;

        allBalloons[x1, y1] = b2;
        allBalloons[x2, y2] = b1;

        bool result = CheckMatchExists(x1, y1) || CheckMatchExists(x2, y2);

        allBalloons[x1, y1] = b1;
        allBalloons[x2, y2] = b2;

        return result;
    }

    /*******************************
    EŞLEŞME KONTROL
    *******************************/
    private bool CheckMatchExists(int x, int y)
    {
        GameObject center = allBalloons[x, y];
        if (center == null) return false;
        string tag = center.tag;

        // Horizontal
        int count = 1;
        for (int i = x - 1; i >= 0 && allBalloons[i, y] != null && allBalloons[i, y].tag == tag; i--) count++;
        for (int i = x + 1; i < width && allBalloons[i, y] != null && allBalloons[i, y].tag == tag; i++) count++;
        if (count >= 3) return true;

        // Vertical
        count = 1;
        for (int i = y - 1; i >= 0 && allBalloons[x, i] != null && allBalloons[x, i].tag == tag; i--) count++;
        for (int i = y + 1; i < height && allBalloons[x, i] != null && allBalloons[x, i].tag == tag; i++) count++;
        if (count >= 3) return true;

        return false;
    }

    /*******************************
    İLK TEMİZLEME
    *******************************/
    private IEnumerator InitialClear()
    {
        yield return new WaitForSeconds(0.2f);
        while (CheckAndClearMatches())
            yield return new WaitForSeconds(0.5f);
    }
    /*******************************
    BLOCKED POZİSYONLARI KONTROL ET
    *******************************/
    void ValidateBlockedPositions()
    {
        foreach (var pos in blockedPositions)
        {
            if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height)
            {
                Debug.LogWarning($"❗Blocked position ({pos.x},{pos.y}) is outside the grid bounds.");
            }
        }
    }

    /*******************************
        BALONLARI ÜRET
    *******************************/
    void GenerateBoard()
    {


        StartCoroutine(CheckBoardHasMoves());
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // BLOCKED pozisyon kontrolü
                bool isBlocked = blockedPositions.Exists(pos => pos.x == x && pos.y == y);
                if (isBlocked)
                {
                    continue;
                }

                Vector2 spawnPosition = new Vector2(x * spacing, y * spacing);

                // Arkaplan
                if (itemBackgroundPrefab != null)
                {
                    GameObject background = Instantiate(itemBackgroundPrefab, spawnPosition, Quaternion.identity);
                    background.transform.parent = this.transform;
                    background.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
                    background.transform.position = new Vector3(spawnPosition.x, spawnPosition.y, 1f);
                }

                // Balon
                int randomBalloon = Random.Range(0, balloonPrefabs.Length);
                GameObject balloon = Instantiate(balloonPrefabs[randomBalloon], spawnPosition, Quaternion.identity);
                balloon.transform.parent = this.transform;
                allBalloons[x, y] = balloon;

                BalloonItem balloonScript = balloon.GetComponent<BalloonItem>();
                if (balloonScript != null)
                {
                    balloonScript.x = x;
                    balloonScript.y = y;
                }
            }
        }
    }


    /*******************************
        GEÇERLİ HAMLE VAR MI KONTROL ET
    *******************************/
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

    /*******************************
        GEÇERLİ HAMLE VAR MI KONTROL ET
        (ÖZEL)
    *******************************/
    private bool HasAnyValidMoves()
    {
        return FindFirstValidSwap().HasValue;
    }

    /*******************************
        BALONLARI DEĞİŞTİR
    *******************************/
    public void SwapBalloons(int x1, int y1, int x2, int y2)
    {
        GameObject b1 = allBalloons[x1, y1];
        GameObject b2 = allBalloons[x2, y2];

        if (b1 == null || b2 == null) return;

        BalloonItem item1 = b1.GetComponent<BalloonItem>();
        BalloonItem item2 = b2.GetComponent<BalloonItem>();

        // Swap
        allBalloons[x1, y1] = b2;
        allBalloons[x2, y2] = b1;

        item1.x = x2;
        item1.y = y2;

        item2.x = x1;
        item2.y = y1;

        item1.MoveTo(new Vector3(x2 * spacing, y2 * spacing, 0));
        item2.MoveTo(new Vector3(x1 * spacing, y1 * spacing, 0));

        // Kontrol et
        StartCoroutine(CheckMatchAfterSwap(x1, y1, x2, y2));
    }
    /*******************************
        EŞLEŞME KONTROLÜNDEN SONRA GERİ SAR
        (ÖZEL)
        Bu fonksiyon, balonları değiştirdikten sonra eşleşme kontrolü yapar.
    *******************************/
    private System.Collections.IEnumerator CheckMatchAfterSwap(int x1, int y1, int x2, int y2)
    {
        yield return new WaitForSeconds(0.3f);

        if (!CheckAndClearMatches())
        {
            // Eşleşme yoksa geri sar ama kontrolsüz
            SwapWithoutCheck(x1, y1, x2, y2);
        }
    }
    /*******************************
        BALONLARI GERİ SAR
        (ÖZEL)
        Bu fonksiyon, balonları değiştirdikten sonra eşleşme kontrolü yapar.
    *******************************/
    private void SwapWithoutCheck(int x1, int y1, int x2, int y2)
    {
        GameObject b1 = allBalloons[x1, y1];
        GameObject b2 = allBalloons[x2, y2];

        if (b1 == null || b2 == null) return;

        BalloonItem item1 = b1.GetComponent<BalloonItem>();
        BalloonItem item2 = b2.GetComponent<BalloonItem>();

        allBalloons[x1, y1] = b2;
        allBalloons[x2, y2] = b1;

        item1.x = x2;
        item1.y = y2;

        item2.x = x1;
        item2.y = y1;

        item1.MoveTo(new Vector3(x2 * spacing, y2 * spacing, 0));
        item2.MoveTo(new Vector3(x1 * spacing, y1 * spacing, 0));
    }
    /*******************************
        EŞLEŞMELERİ KONTROL ET VE TEMİZLE
        (ÖZEL)
    *******************************/
   private bool CheckAndClearMatches()
    {
        bool matchFound = false;
        bool[,] matched = new bool[width, height];

        // Satır kontrolü
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width - 2; x++)
            {
                GameObject b1 = allBalloons[x, y];
                GameObject b2 = allBalloons[x + 1, y];
                GameObject b3 = allBalloons[x + 2, y];

                if (b1 != null && b2 != null && b3 != null &&
                    b1.tag == b2.tag && b2.tag == b3.tag)
                {
                    matched[x, y] = true;
                    matched[x + 1, y] = true;
                    matched[x + 2, y] = true;
                    matchFound = true;
                }
            }
        }

        // Sütun kontrolü
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height - 2; y++)
            {
                GameObject b1 = allBalloons[x, y];
                GameObject b2 = allBalloons[x, y + 1];
                GameObject b3 = allBalloons[x, y + 2];

                if (b1 != null && b2 != null && b3 != null &&
                    b1.tag == b2.tag && b2.tag == b3.tag)
                {
                    matched[x, y] = true;
                    matched[x, y + 1] = true;
                    matched[x, y + 2] = true;
                    matchFound = true;
                }
            }
        }

        if (matchFound)
        {
            // Coroutine başlat yok etmek için
            StartCoroutine(DestroyMatched(matched));
        }

        return matchFound;
    }
    /*******************************
        BALONLARI DÜŞÜR
        (ÖZEL)
        Bu fonksiyon, eşleşmelerden sonra balonları düşürür ve yeni balonlar üretir.
        Ayrıca, düşen balonların altında engel olup olmadığını kontrol eder.
    *******************************/
    private void DropBalloons()
    {
        for (int x = 0; x < width; x++)
        {
            int emptyY = -1;

            for (int y = 0; y < height; y++)
            {
                // ❌ Blocked pozisyon ise devam
                if (blockedPositions.Exists(p => p.x == x && p.y == y))
                    continue;

                if (allBalloons[x, y] == null)
                {
                    if (emptyY == -1) emptyY = y;
                }
                else if (emptyY != -1)
                {
                    // ✅ Aşağı kaydır
                    if (!blockedPositions.Exists(p => p.x == x && p.y == emptyY))
                    {
                        allBalloons[x, emptyY] = allBalloons[x, y];
                        allBalloons[x, y] = null;

                        BalloonItem b = allBalloons[x, emptyY].GetComponent<BalloonItem>();
                        b.x = x;
                        b.y = emptyY;

                        b.MoveTo(new Vector3(x * spacing, emptyY * spacing, 0));
                        emptyY++;

                        // 🔁 Boşluk takip için bir sonrakini bul
                        while (emptyY < height && blockedPositions.Exists(p => p.x == x && p.y == emptyY))
                            emptyY++;
                    }
                }
            }

            // ❗ Spawn sadece block olmayan yerlere
            for (int y = height - 1; y >= 0; y--)
            {
                if (allBalloons[x, y] == null && !blockedPositions.Exists(p => p.x == x && p.y == y))
                {
                    Vector2 spawnPos = new Vector2(x * spacing, (y + height) * spacing);
                    int rand = Random.Range(0, balloonPrefabs.Length);
                    GameObject newBalloon = Instantiate(balloonPrefabs[rand], spawnPos, Quaternion.identity);
                    newBalloon.transform.parent = this.transform;

                    allBalloons[x, y] = newBalloon;

                    BalloonItem b = newBalloon.GetComponent<BalloonItem>();
                    b.x = x;
                    b.y = y;
                    b.MoveTo(new Vector3(x * spacing, y * spacing, 0));
                }
            }
        }

        StartCoroutine(ClearAfterFall());
    }
    /*******************************
        DÜŞME SONRASI TEMİZLE
        (ÖZEL)
        Bu fonksiyon, düşme sonrası eşleşmeleri kontrol eder ve temizler.
    *******************************/
    private System.Collections.IEnumerator ClearAfterFall()
    {
        yield return new WaitForSeconds(0.4f);

        if (CheckAndClearMatches())
        {
            // Eğer yeni eşleşmeler varsa zincirleme patlama
            yield return new WaitForSeconds(0.2f);
        }
    }
    /*******************************
        BALONLARI TEMİZLE
        (ÖZEL)
        Bu fonksiyon, eşleşen balonları temizler ve puan ekler.
    *******************************/
    private System.Collections.IEnumerator DestroyMatched(bool[,] matched)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (matched[x, y] && allBalloons[x, y] != null)
                {
                    Destroy(allBalloons[x, y]);
                    allBalloons[x, y] = null;
                    ScoreManager.Instance.AddScore(10); // PUAN EKLE
                    yield return new WaitForSeconds(0.05f); // küçük efekt gecikmesi
                }
            }
        }

        yield return new WaitForSeconds(0.1f);
        DropBalloons();
    }

    /*******************************
        EŞLEŞEN BALONLARI BUL
        (ÖZEL)
        Bu fonksiyon, eşleşen balonları bulur ve döndürür.
    *******************************/
    // List<Vector2Int> FindConnectedMatches(int startX, int startY, string tag)
    // {
    //     List<Vector2Int> connected = new List<Vector2Int>();
    //     bool[,] visited = new bool[width, height];
    //     Queue<Vector2Int> queue = new Queue<Vector2Int>();
    //     queue.Enqueue(new Vector2Int(startX, startY));

    //     while (queue.Count > 0)
    //     {
    //         Vector2Int current = queue.Dequeue();
    //         int x = current.x;
    //         int y = current.y;

    //         if (x < 0 || x >= width || y < 0 || y >= height)
    //             continue;

    //         if (visited[x, y] || allBalloons[x, y] == null || allBalloons[x, y].tag != tag)
    //             continue;

    //         visited[x, y] = true;
    //         connected.Add(current);

    //         // 4 yönlü komşular
    //         queue.Enqueue(new Vector2Int(x + 1, y));
    //         queue.Enqueue(new Vector2Int(x - 1, y));
    //         queue.Enqueue(new Vector2Int(x, y + 1));
    //         queue.Enqueue(new Vector2Int(x, y - 1));
    //     }

    //     return connected;
    // }




}