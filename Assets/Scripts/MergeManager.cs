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
        // 1) Başlangıç pozlarını al
        float duration = 0.2f, t = 0f;
        Vector3 target = board.CellToWorld(spawnX, spawnY);
        Vector3[] starts = new Vector3[items.Count];
        for (int i = 0; i < items.Count; i++)
        {
            var go = items[i];
            starts[i] = go != null ? go.transform.position : target;
        }

        // 2) Merge animasyonu
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

        // 3) Grid'den temizle
        foreach (var g in items)
        {
            if (g == null) continue;
            var bi = g.GetComponent<BalloonItem>();
            if (bi != null)
                board.allBalloons[bi.x, bi.y] = null;
        }

        // 4) Objeleri yok et
        foreach (var g in items)
            if (g != null) Destroy(g);

        // 5) Special spawn
        if (specialPrefab != null)
            spawner.SpawnSpecial(specialPrefab, spawnX, spawnY);

        yield return new WaitForSeconds(0.1f);

        // 6) Dökülme devam etsin
        dropManager.DropBalloons();
    }
}