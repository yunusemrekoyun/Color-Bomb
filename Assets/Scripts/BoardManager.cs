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
        AdjustCamera();


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

                // Daha küçük scale (görselin dışa taşmaması için)
                background.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

                // Balonların arkasında kalması için Z geriye al
                background.transform.position = new Vector3(spawnPosition.x, spawnPosition.y, 1f);
            }

            // Balloon instantiate
            int randomBalloon = Random.Range(0, balloonPrefabs.Length);
            GameObject balloon = Instantiate(balloonPrefabs[randomBalloon], spawnPosition, Quaternion.identity);
            balloon.transform.parent = this.transform;
            allBalloons[x, y] = balloon;
        }
    }
}
    private void AdjustCamera()
    {
        Camera cam = Camera.main;

        // Ana kamera yoksa yeni oluştur
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.tag = "MainCamera";
        }

        // Grid'in merkezini hesapla
        float centerX = (width - 1) * spacing / 2f;
        float centerY = (height - 1) * spacing / 2f;
        cam.transform.position = new Vector3(centerX, centerY, -10f);

        // Oranları al
        float screenRatio = (float)Screen.width / (float)Screen.height;
        float boardWidth = width * spacing;
        float boardHeight = height * spacing;

        // Çevrede ekstra alan bırak
        float margin = 2f;

        // Kamera boyutu ayarla (yükseklik öncelikli)
        float verticalSize = (boardHeight / 2f) + margin;
        float horizontalSize = (boardWidth / (2f * screenRatio)) + margin;
        cam.orthographicSize = Mathf.Max(verticalSize, horizontalSize);
    }

}