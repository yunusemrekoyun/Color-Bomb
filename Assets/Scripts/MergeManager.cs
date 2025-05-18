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

    private IEnumerator MergeAndDestroy(List<GameObject> items, int spawnX, int spawnY, GameObject specialPrefab)
    {
        float duration = 0.2f, t = 0f;
        Vector3 target = new Vector3(spawnX * board.spacing + board.offsetX,
                                     spawnY * board.spacing + board.offsetY, 0);
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

        foreach (var g in items)
        {
            var bi = g.GetComponent<BalloonItem>();
            if (bi != null)
                board.allBalloons[bi.x, bi.y] = null;
        }
        foreach (var g in items) Destroy(g);

        if (specialPrefab != null)
            spawner.SpawnSpecial(specialPrefab, spawnX, spawnY);

        yield return new WaitForSeconds(0.1f);
        dropManager.DropBalloons();
    }
}

