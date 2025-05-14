using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public GameObject[] balloonPrefabs;
    public int width = 8;
    public int height = 9;
    public float spacing = 3f; // spacing aralığı
    public GameObject itemBackgroundPrefab;
    private GameObject[,] allBalloons;

    void Start()
    {
        allBalloons = new GameObject[width, height];
        GenerateBoard();
        // AdjustCamera();


    }
    void GenerateBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 spawnPosition = new Vector2(x * spacing, y * spacing);

                // Background instantiate
                if (itemBackgroundPrefab != null)
                {
                    GameObject background = Instantiate(itemBackgroundPrefab, spawnPosition, Quaternion.identity);
                    background.transform.parent = this.transform;

                    background.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
                    background.transform.position = new Vector3(spawnPosition.x, spawnPosition.y, 1f);
                }

                // Balloon instantiate
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

    private System.Collections.IEnumerator CheckMatchAfterSwap(int x1, int y1, int x2, int y2)
    {
        yield return new WaitForSeconds(0.3f);

        if (!CheckAndClearMatches())
        {
            // Eşleşme yoksa geri sar
            SwapBalloons(x1, y1, x2, y2);
        }
    }
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
    private void DropBalloons()
    {
        for (int x = 0; x < width; x++)
        {
            int emptyY = -1;

            for (int y = 0; y < height; y++)
            {
                if (allBalloons[x, y] == null)
                {
                    if (emptyY == -1) emptyY = y;
                }
                else if (emptyY != -1)
                {
                    // Balonu aşağı taşı
                    allBalloons[x, emptyY] = allBalloons[x, y];
                    allBalloons[x, y] = null;

                    BalloonItem b = allBalloons[x, emptyY].GetComponent<BalloonItem>();
                    b.x = x;
                    b.y = emptyY;

                    b.MoveTo(new Vector3(x * spacing, emptyY * spacing, 0));
                    emptyY++;
                }
            }

            // Yeni balonları en üstten oluştur
            for (int y = height - 1; y >= 0; y--)
            {
                if (allBalloons[x, y] == null)
                {
                    Vector2 spawnPos = new Vector2(x * spacing, (y + height) * spacing); // yukarıda başlasın
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

        // Yeni düşenler eşleşti mi tekrar kontrol et
        StartCoroutine(ClearAfterFall());
    }

    private System.Collections.IEnumerator ClearAfterFall()
    {
        yield return new WaitForSeconds(0.4f);

        if (CheckAndClearMatches())
        {
            // Eğer yeni eşleşmeler varsa zincirleme patlama
            yield return new WaitForSeconds(0.2f);
        }
    }

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
                    yield return new WaitForSeconds(0.05f); // küçük efekt gecikmesi
                }
            }
        }

        yield return new WaitForSeconds(0.1f);
        DropBalloons();
    }




    // private void AdjustCamera()
    // {
    //     Camera cam = Camera.main;

    //     // Ana kamera yoksa yeni oluştur
    //     // if (cam == null)
    //     // {
    //     //     GameObject camObj = new GameObject("Main Camera");
    //     //     cam = camObj.AddComponent<Camera>();
    //     //     cam.orthographic = true;
    //     //     cam.tag = "MainCamera";
    //     // }

    //     // Grid'in merkezini hesapla
    //     float centerX = ((width - 1) * spacing) / 2f;
    //     float centerY = ((height - 1) * spacing) / 2f;
    //     cam.transform.position = new Vector3(centerX, centerY, -10f);

    //     // Oranları al
    //     float screenRatio = (float)Screen.width / (float)Screen.height;
    //     float boardWidth = width * spacing;
    //     float boardHeight = height * spacing;

    //     // Çevrede ekstra alan bırak
    //     float margin = 4f;

    //     // Kamera boyutu ayarla (yükseklik öncelikli)
    //     float verticalSize = (boardHeight / 2f) + margin;
    //     float horizontalSize = (boardWidth / (2f * screenRatio)) + margin;
    //     cam.orthographicSize = Mathf.Max(verticalSize, horizontalSize);

    //     // Kamera'nın ekran kenarlarını hesapla
    //     // float camHalfHeight = cam.orthographicSize;
    //     // float camHalfWidth = camHalfHeight * screenRatio;

    //     // float camLeft = cam.transform.position.x - camHalfWidth;
    //     // float camRight = cam.transform.position.x + camHalfWidth;

    //     // // Grid sınırları
    //     // float gridLeft = 0f;
    //     // float gridRight = (width - 1) * spacing;

    //     // // Boşluk farklarını hesapla
    //     // float leftMargin = camLeft - gridLeft;
    //     // float rightMargin = camRight - gridRight;

    //     // // Konsola yazdır
    //     // Debug.Log("🟩 Camera HalfWidth: " + camHalfWidth);
    //     // Debug.Log("🟩 Grid Width: " + gridRight);
    //     // Debug.Log("🟩 Cam Left: " + camLeft + " | Grid Left: " + gridLeft + " → Left Margin: " + leftMargin);
    //     // Debug.Log("🟩 Cam Right: " + camRight + " | Grid Right: " + gridRight + " → Right Margin: " + rightMargin);
    //     // Debug.Log("🟩 Fark (sağ - sol): " + (rightMargin - leftMargin));
    // }

}