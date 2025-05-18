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
        Vector3 worldPos = new Vector3(x * board.spacing + board.offsetX,
                                       y * board.spacing + board.offsetY, 0);
        var special = Instantiate(prefab, worldPos, Quaternion.identity, transform);
        board.allBalloons[x, y] = special;
        var balloon = special.GetComponent<BalloonItem>() ?? special.AddComponent<BalloonItem>();
        balloon.x = x; balloon.y = y;
        balloon.MoveTo(worldPos);
    }
}