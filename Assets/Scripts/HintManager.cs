// HintManager.cs
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(GameBoard))]
public class HintManager : MonoBehaviour
{
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

    public void ResetIdleTimer() => idleTimer = 0f;

    private void ShowHint()
    {
        var match = GetComponent<MatchManager>().GetFirstValidSwap();

        if (match.HasValue)
        {
            HighlightBalloon(match.Value.Item1.x, match.Value.Item1.y);
            HighlightBalloon(match.Value.Item2.x, match.Value.Item2.y);
        }
    }

    private void HighlightBalloon(int x, int y)
    {
        var b = board.allBalloons[x, y];
        if (b == null) return;
        var sr = b.GetComponent<SpriteRenderer>();
        if (sr != null)
            StartCoroutine(HintBlink(sr));
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