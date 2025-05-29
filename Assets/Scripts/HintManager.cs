using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
[RequireComponent(typeof(GameBoard))]
public class HintManager : MonoBehaviour
{
    private (Vector2Int from, Vector2Int to, List<GameObject> matchedItems)? cachedHint = null;
    private GameBoard board;
    private float idleTimer;

    private void Awake() => board = GetComponent<GameBoard>();

    private void Update()
    {
        idleTimer += Time.deltaTime;
        if (idleTimer >= board.hintDelay)
        {
            ShowHint();
            idleTimer = 0f;
        }
    }

    public void ResetIdleTimer()
    {
        idleTimer = 0f;
        cachedHint = null;
    }
    private void ShowHint()
    {
        if (cachedHint == null)
            cachedHint = GetComponent<MatchManager>().GetBestValidSwapWithMatches();

        if (!cachedHint.HasValue)
            return;

        // donuk balonları filtreleyelim
        var itemsToBlink = cachedHint.Value.matchedItems
            .Where(b => b != null && b.GetComponent<BalloonItem>()?.isFrozen == false)
            .ToList();

        if (itemsToBlink.Count == 0)
            return; // sadece frozen’lar varsa hiçbir ipucu gösterme

        foreach (var balloon in itemsToBlink)
        {
            var sr = balloon.GetComponent<SpriteRenderer>();
            if (sr != null)
                StartCoroutine(HintBlink(sr));
        }
    }
    private IEnumerator HintBlink(SpriteRenderer sr)
    {
        if (sr == null) yield break;

        Transform tf = sr.transform;
        if (tf == null || tf.gameObject == null) yield break;

        Vector3 originalScale = tf.localScale;

        for (int i = 0; i < 3; i++)
        {
            if (tf == null) yield break;
            tf.localScale = originalScale * 1.2f;
            yield return new WaitForSeconds(0.2f);

            if (tf == null) yield break;
            tf.localScale = originalScale;
            yield return new WaitForSeconds(0.2f);
        }
    }
}
