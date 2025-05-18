using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    if (cachedHint.HasValue)
    {
        foreach (GameObject balloon in cachedHint.Value.matchedItems)
        {
            if (balloon == null) continue;
            var sr = balloon.GetComponent<SpriteRenderer>();
            if (sr != null)
                StartCoroutine(HintBlink(sr));
        }
    }
}

    private IEnumerator HintBlink(SpriteRenderer sr)
    {
        var tf = sr.transform;
        var originalScale = tf.localScale;
        for (int i = 0; i < 3; i++)
        {
            tf.localScale = originalScale * 1.2f;
            yield return new WaitForSeconds(0.2f);
            tf.localScale = originalScale;
            yield return new WaitForSeconds(0.2f);
        }
    }
}
