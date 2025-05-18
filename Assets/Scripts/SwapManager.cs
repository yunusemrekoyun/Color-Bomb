// SwapManager.cs
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(GameBoard), typeof(HintManager))]
public class SwapManager : MonoBehaviour
{
    private GameBoard board;
    private HintManager hintManager;

    private void Awake()
    {
        board = GetComponent<GameBoard>();
        hintManager = GetComponent<HintManager>();
    }

    public void SwapBalloons(int x1, int y1, int x2, int y2)
    {
        hintManager.ResetIdleTimer();

        var b1 = board.allBalloons[x1, y1];
        var b2 = board.allBalloons[x2, y2];
        if (b1 == null || b2 == null) return;

        board.allBalloons[x1, y1] = b2;
        board.allBalloons[x2, y2] = b1;
        var i1 = b1.GetComponent<BalloonItem>(); i1.x = x2; i1.y = y2;
        var i2 = b2.GetComponent<BalloonItem>(); i2.x = x1; i2.y = y1;

        i1.MoveTo(new Vector3(x2 * board.spacing + board.offsetX,
                               y2 * board.spacing + board.offsetY, 0));
        i2.MoveTo(new Vector3(x1 * board.spacing + board.offsetX,
                               y1 * board.spacing + board.offsetY, 0));

        StartCoroutine(CheckMatchAfterSwap(x1, y1, x2, y2));
    }

    private IEnumerator CheckMatchAfterSwap(int x1, int y1, int x2, int y2)
    {
        yield return new WaitForSeconds(0.3f);
        var matchManager = GetComponent<MatchManager>();
        if (!matchManager.CheckAndClearMatches())
            SwapWithoutCheck(x1, y1, x2, y2);
    }

    private void SwapWithoutCheck(int x1, int y1, int x2, int y2)
    {
        var b1 = board.allBalloons[x1, y1];
        var b2 = board.allBalloons[x2, y2];
        if (b1 == null || b2 == null) return;

        var i1 = b1.GetComponent<BalloonItem>();
        var i2 = b2.GetComponent<BalloonItem>();

        board.allBalloons[x1, y1] = b2;
        board.allBalloons[x2, y2] = b1;
        i1.x = x2; i1.y = y2;
        i2.x = x1; i2.y = y1;

        i1.MoveTo(new Vector3(x2 * board.spacing + board.offsetX,
                               y2 * board.spacing + board.offsetY, 0));
        i2.MoveTo(new Vector3(x1 * board.spacing + board.offsetX,
                               y1 * board.spacing + board.offsetY, 0));
    }
}
