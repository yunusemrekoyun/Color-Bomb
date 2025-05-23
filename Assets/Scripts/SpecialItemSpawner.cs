// SpecialItemSpawner.cs
using UnityEngine;

[RequireComponent(typeof(GameBoard))]
public class SpecialItemSpawner : MonoBehaviour
{
    private GameBoard board;

    private void Awake() => board = GetComponent<GameBoard>();

    public void SpawnSpecial(GameObject prefab, int x, int y)
{
    if (board.allBalloons[x, y] != null)
{
    Destroy(board.allBalloons[x, y]); // Çakışma varsa eskiyi sil
}
    if (prefab == null) return;

    Vector3 worldPos = board.CellToWorld(x, y);
    worldPos.z = 0f; // Balonlar gibi davranacak

    // 🔥 ESKİ BALONU YOK ET (şart!)
    var existing = board.allBalloons[x, y];
    if (existing != null)
    {
        Destroy(existing);
        board.allBalloons[x, y] = null;
    }

    // ✅ SPECIAL ITEM SPAWN
    var special = Instantiate(prefab, worldPos, Quaternion.identity, transform);
    board.allBalloons[x, y] = special;

    var bi = special.GetComponent<BalloonItem>() ?? special.AddComponent<BalloonItem>();
    bi.x = x;
    bi.y = y;
    bi.MoveTo(worldPos);

    var sr = special.GetComponent<SpriteRenderer>();
    if (sr != null)
    {
        sr.sortingLayerName = "SpecialItem"; // Unity’de tanımlı olmalı
        sr.sortingOrder = 10; // Balonların üstünde kalmalı
    }
}
}