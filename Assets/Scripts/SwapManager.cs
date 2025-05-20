// SwapManager.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

        // Swap işlemi
        board.allBalloons[x1, y1] = b2;
        board.allBalloons[x2, y2] = b1;

        var i1 = b1.GetComponent<BalloonItem>();
        var i2 = b2.GetComponent<BalloonItem>();

        i1.x = x2; i1.y = y2;
        i2.x = x1; i2.y = y1;

        i1.MoveTo(new Vector3(x2 * board.spacing + board.offsetX,
                               y2 * board.spacing + board.offsetY, 0));
        i2.MoveTo(new Vector3(x1 * board.spacing + board.offsetX,
                               y1 * board.spacing + board.offsetY, 0));

        // 👇 Swap sonrası special olup olmadığını kontrol edip coroutine başlat
        StartCoroutine(HandlePostSwap(b1, b2, x1, y1, x2, y2));
    }

    private IEnumerator HandlePostSwap(GameObject b1, GameObject b2, int x1, int y1, int x2, int y2)
    {
        yield return new WaitForSeconds(0.25f); // Swap animasyonu süresi

        SpecialItem s1 = b1.GetComponent<SpecialItem>();
        SpecialItem s2 = b2.GetComponent<SpecialItem>();

        bool specialTriggered = false;

        if (s1 != null)
        {
            board.allBalloons[x2, y2] = null;
            StartCoroutine(TriggerSpecial(s1, x2, y2, b2.tag)); // önce patlat
            Destroy(b1); // sonra yok et
            specialTriggered = true;
        }

        if (s2 != null)
        {
            board.allBalloons[x1, y1] = null;
            StartCoroutine(TriggerSpecial(s2, x1, y1, b1.tag));
            Destroy(b2); // sonra yok et
            specialTriggered = true;
        }

        if (!specialTriggered)
        {
            StartCoroutine(CheckMatchAfterSwap(x1, y1, x2, y2));
        }
    }



    private IEnumerator TriggerSpecial(SpecialItem item, int x, int y, string targetTag = null)
    {
        List<GameObject> toDestroy = new List<GameObject>();

        if (item.state == SpecialItem.SpecialState.Horizontal4)
        {
            for (int i = 0; i < board.width; i++)
            {
                if (board.allBalloons[i, y] != null)
                {
                    toDestroy.Add(board.allBalloons[i, y]);
                    board.allBalloons[i, y] = null;
                }
            }
        }
        else if (item.state == SpecialItem.SpecialState.Vertical4)
        {
            for (int j = 0; j < board.height; j++)
            {
                if (board.allBalloons[x, j] != null)
                {
                    toDestroy.Add(board.allBalloons[x, j]);
                    board.allBalloons[x, j] = null;
                }
            }
        }
        else if (item.state == SpecialItem.SpecialState.Vertical5) // bomb gibi
        {
            int radius = 1;
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int tx = x + dx;
                    int ty = y + dy;
                    if (tx >= 0 && tx < board.width && ty >= 0 && ty < board.height)
                    {
                        var b = board.allBalloons[tx, ty];
                        if (b != null && !toDestroy.Contains(b))
                        {
                            toDestroy.Add(b);
                            board.allBalloons[tx, ty] = null;
                        }
                    }
                }
            }
        }
        else if (item.state == SpecialItem.SpecialState.Horizontal5 && targetTag != null) // renk yok edici
        {
            for (int i = 0; i < board.width; i++)
            {
                for (int j = 0; j < board.height; j++)
                {
                    var b = board.allBalloons[i, j];
                    if (b != null && b.tag == targetTag)
                    {
                        toDestroy.Add(b);
                        board.allBalloons[i, j] = null;
                    }
                }
            }
        }

        foreach (var obj in toDestroy)
            Destroy(obj);

        yield return new WaitForSeconds(0.4f); // 🎯 Patlama efekti gibi kısa bir duraklama

        GetComponent<DropManager>().DropBalloons();
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
