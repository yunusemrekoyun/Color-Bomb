// SpecialItemSpawner.cs
using UnityEngine;

[RequireComponent(typeof(GameBoard))]
public class SpecialItemSpawner : MonoBehaviour
{
    private GameBoard board;

    private void Awake() => board = GetComponent<GameBoard>();

   public void SpawnSpecial(GameObject prefab, int x, int y)
{
    if (prefab == null) return;

    // 🔥 Mevcut varsa yok et
    var existing = board.allBalloons[x, y];
    if (existing != null)
    {
        Destroy(existing);
        board.allBalloons[x, y] = null;
    }

    Vector3 worldPos = board.CellToWorld(x, y);
    worldPos.z = 0f;

    var special = Instantiate(prefab, worldPos, Quaternion.identity, transform);
    board.allBalloons[x, y] = special;

    var bi = special.GetComponent<BalloonItem>() ?? special.AddComponent<BalloonItem>();
    bi.x = x;
    bi.y = y;
    bi.MoveTo(worldPos);

    var sr = special.GetComponent<SpriteRenderer>();
    if (sr != null)
    {
        sr.sortingLayerName = "SpecialItem"; // UI'de üstte kalsın
        sr.sortingOrder = 10;
    }
}
}