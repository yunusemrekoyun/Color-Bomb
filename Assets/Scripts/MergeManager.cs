// MergeManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GameBoard), typeof(SpecialItemSpawner), typeof(DropManager))]
public class MergeManager : MonoBehaviour
{
    private GameBoard board;
    private SpecialItemSpawner spawner;
    private DropManager dropManager;

    private void Awake()
    {
        board = GetComponent<GameBoard>();
        spawner = GetComponent<SpecialItemSpawner>();
        dropManager = GetComponent<DropManager>();
    }

    public void StartMerge(List<GameObject> items, int spawnX, int spawnY, GameObject specialPrefab)
    {
        StartCoroutine(MergeAndDestroy(items, spawnX, spawnY, specialPrefab));
    }

    private IEnumerator MergeAndDestroy(
     List<GameObject> items,
     int spawnX,
     int spawnY,
     GameObject specialPrefab)
    {
        float duration = 0.2f, t = 0f;
        Vector3 target = board.CellToWorld(spawnX, spawnY);
        Vector3[] starts = new Vector3[items.Count];

        for (int i = 0; i < items.Count; i++)
        {
            var go = items[i];
            starts[i] = go != null ? go.transform.position : target;
        }

        // ➤ Cam ve kutulara hasar ver (aynı pozisyona sadece 1 kez)
        HashSet<Vector2Int> damagedPositions = new();

        foreach (var item in items)
        {
            if (item == null) continue;
            var bi = item.GetComponent<BalloonItem>();
            if (bi == null) continue;

            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(1, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, -1)
            };

            foreach (var dir in directions)
            {
                int nx = bi.x + dir.x;
                int ny = bi.y + dir.y;
                Vector2Int nPos = new Vector2Int(nx, ny);

                if (nx < 0 || nx >= board.width || ny < 0 || ny >= board.height) continue;

                if (!damagedPositions.Contains(nPos))
                {
                    board.breakableManager.TryDamageBlock(nPos);
                    damagedPositions.Add(nPos);
                }
            }
        }

        // ➤ Merge animasyonu
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            for (int i = 0; i < items.Count; i++)
            {
                var go = items[i];
                if (go != null)
                    go.transform.position = Vector3.Lerp(starts[i], target, t);
            }
            yield return null;
        }

        // ➤ Grid'den temizle
        foreach (var g in items)
        {
            if (g == null) continue;
            var bi = g.GetComponent<BalloonItem>();
            if (bi != null)
                board.allBalloons[bi.x, bi.y] = null;
        }

        // ➤ Objeleri yok et
        foreach (var g in items)
            if (g != null) Destroy(g);

        // ➤ Special item spawn
        if (specialPrefab != null)
        {
            if (board.allBalloons[spawnX, spawnY] != null)
            {
                var existing = board.allBalloons[spawnX, spawnY];
                board.allBalloons[spawnX, spawnY] = null;

                if (existing != null && existing) // hem referans hem sahnede var mı
                    Destroy(existing);
            }
            spawner.SpawnSpecial(specialPrefab, spawnX, spawnY);

            spawner.SpawnSpecial(specialPrefab, spawnX, spawnY);
        }

        yield return new WaitForSeconds(0.1f);

        // ➤ Balonlar düşsün
        dropManager.DropBalloons();
    }
}