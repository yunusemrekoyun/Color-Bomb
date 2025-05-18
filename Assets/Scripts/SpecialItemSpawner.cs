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

        // Hücrenin dünya pozisyonunu al
        Vector3 worldPos = board.CellToWorld(x, y);

        // Instantiate
        var special = Instantiate(prefab, worldPos, Quaternion.identity, transform);
        board.allBalloons[x, y] = special;

        // BalloonItem component’i ekle/güncelle
        var bi = special.GetComponent<BalloonItem>() ?? special.AddComponent<BalloonItem>();
        bi.x = x;
        bi.y = y;
        bi.MoveTo(worldPos);
    }
}