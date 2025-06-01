using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(GameBoard), typeof(HintManager))]
public class SwapManager : MonoBehaviour
{
    private GameBoard board;
    private HintManager hintManager;
    private MovesManager movesManager;

    public GameObject focusEffectPrefab;
    public GameObject explosionEffectPrefab;

    private void Awake()
    {
        board = GetComponent<GameBoard>();
        hintManager = GetComponent<HintManager>();
        movesManager = FindFirstObjectByType<MovesManager>();
    }

    public void SwapBalloons(int x1, int y1, int x2, int y2)
    {
        hintManager.ResetIdleTimer();

        var b1 = board.allBalloons[x1, y1];
        var b2 = board.allBalloons[x2, y2];
        if (b1 == null || b2 == null) return;

        var bi1 = b1.GetComponent<BalloonItem>();
        var bi2 = b2.GetComponent<BalloonItem>();
        if ((bi1 != null && bi1.isFrozen) || (bi2 != null && bi2.isFrozen)) return;

        board.allBalloons[x1, y1] = b2;
        board.allBalloons[x2, y2] = b1;

        bi1.x = x2; bi1.y = y2;
        bi2.x = x1; bi2.y = y1;

        bi1.MoveTo(GetWorldPosition(x2, y2));
        bi2.MoveTo(GetWorldPosition(x1, y1));

        StartCoroutine(HandlePostSwap(b1, b2, x1, y1, x2, y2));
    }

    private Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x * board.spacing + board.offsetX, y * board.spacing + board.offsetY, 0);
    }

    private IEnumerator HandlePostSwap(GameObject b1, GameObject b2, int x1, int y1, int x2, int y2)
    {
        yield return new WaitForSeconds(0.25f);

        if (b1 == null || b2 == null || !b1 || !b2) yield break;

        SpecialItem s1 = b1.GetComponent<SpecialItem>();
        SpecialItem s2 = b2.GetComponent<SpecialItem>();

        if (HandleSpecialCombo(s1, s2, x1, y1, x2, y2, b1, b2)) yield break;

        bool specialTriggered = false;

        if (s1 != null)
        {
            board.allBalloons[x2, y2] = null;
            StartCoroutine(TriggerSpecial(s1, x2, y2, b2?.tag));
            Destroy(b1);
            specialTriggered = true;
            movesManager?.UseMove();
        }

        if (s2 != null)
        {
            board.allBalloons[x1, y1] = null;
            StartCoroutine(TriggerSpecial(s2, x1, y1, b1?.tag));
            Destroy(b2);
            specialTriggered = true;
            movesManager?.UseMove();
        }

        if (!specialTriggered)
        {
            StartCoroutine(CheckMatchAfterSwap(x1, y1, x2, y2));
        }
    }

    private bool HandleSpecialCombo(SpecialItem s1, SpecialItem s2, int x1, int y1, int x2, int y2, GameObject b1, GameObject b2)
    {
        if (s1 != null && s2 != null)
        {
            bool comboHandled = SpecialComboManager.TryHandleCombo(s1, s2, x1, y1, x2, y2, board);
            if (comboHandled)
            {
                Destroy(b1);
                Destroy(b2);
                board.allBalloons[x1, y1] = null;
                board.allBalloons[x2, y2] = null;
                movesManager?.UseMove();
                StartCoroutine(DelayedDrop(0.2f));
                return true;
            }
        }
        return false;
    }

    private IEnumerator DelayedDrop(float delay)
    {
        yield return new WaitForSeconds(delay);
        board.GetComponent<DropManager>()?.DropBalloons();
    }

    private IEnumerator CheckMatchAfterSwap(int x1, int y1, int x2, int y2)
    {
        yield return new WaitForSeconds(0.3f);
        var matchManager = GetComponent<MatchManager>();

        if (matchManager.CheckAndClearMatches())
        {
            movesManager?.UseMove();
        }
        else
        {
            SwapWithoutCheck(x1, y1, x2, y2);
        }
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

        i1.MoveTo(GetWorldPosition(x2, y2));
        i2.MoveTo(GetWorldPosition(x1, y1));
    }

    public IEnumerator TriggerSpecialDelayed(SpecialItem item, int x, int y, string targetTag = null, float delay = 0.2f)
    {
        yield return new WaitForSeconds(delay);
        yield return TriggerSpecial(item, x, y, targetTag);
    }

    public IEnumerator TriggerSpecial(SpecialItem item, int x, int y, string targetTag = null)
    {
        switch (item.state)
        {
            case SpecialItem.SpecialState.Horizontal4:
                yield return TriggerHorizontal(x, y, item);
                break;
            case SpecialItem.SpecialState.Vertical4:
                yield return TriggerVertical(x, y, item);
                break;
            case SpecialItem.SpecialState.Square4:
                yield return TriggerSquare(x, y, item);
                break;
            case SpecialItem.SpecialState.Vertical5:
                yield return TriggerBlockBreaker(x, y);
                break;
            case SpecialItem.SpecialState.Horizontal5:
                if (targetTag != null)
                    yield return TriggerColorClear(targetTag);
                break;
        }

        yield return new WaitForSeconds(0.4f);
        GetComponent<DropManager>().DropBalloons();
    }

    private IEnumerator TriggerHorizontal(int x, int yRow, SpecialItem item)
    {
        for (int i = 0; i < board.width; i++)
        {
            TryDestroyAt(i, yRow, item);
        }
        yield return null;
    }

    private IEnumerator TriggerVertical(int xCol, int y, SpecialItem item)
    {
        for (int j = 0; j < board.height; j++)
        {
            TryDestroyAt(xCol, j, item);
        }
        yield return null;
    }

    private IEnumerator TriggerSquare(int x, int y, SpecialItem item)
    {
        int radius = 1;
        for (int dx = -radius; dx <= radius; dx++)
            for (int dy = -radius; dy <= radius; dy++)
                TryDestroyAt(x + dx, y + dy, item);
        yield return null;
    }

    private IEnumerator TriggerBlockBreaker(int x, int y)
    {
        board.breakableManager.DamageRandomBlocks(3);
        yield return null;
    }

    private IEnumerator TriggerColorClear(string tag)
    {
        for (int x = 0; x < board.width; x++)
            for (int y = 0; y < board.height; y++)
                if (board.allBalloons[x, y] != null && board.allBalloons[x, y].tag == tag)
                    TryDestroyAt(x, y);
        yield return null;
    }

 private void TryDestroyAt(int x, int y, SpecialItem currentItem = null)
{
    if (x < 0 || y < 0 || x >= board.width || y >= board.height) return;

    // ➤ Eğer burada cam veya kutu varsa, sadece hasar ver ve balona dokunma
    if (board.breakableManager.HasBlock(x, y))
    {
        board.breakableManager.TryDamageBlock(new Vector2Int(x, y));
        return;
    }

    var obj = board.allBalloons[x, y];
    if (obj == null) return;

    var special = obj.GetComponent<SpecialItem>();
    if (special != null && special != currentItem)
    {
        board.allBalloons[x, y] = null;
        StartCoroutine(TriggerSpecial(special, x, y));
    }

    if (focusEffectPrefab != null)
    {
        GameObject fx = Instantiate(focusEffectPrefab, obj.transform.position, Quaternion.identity);
        Destroy(fx, 1f);
    }
    if (explosionEffectPrefab != null)
    {
        GameObject fx = Instantiate(explosionEffectPrefab, obj.transform.position, Quaternion.identity);
        Destroy(fx, 1f);
    }

    board.allBalloons[x, y] = null;
    Destroy(obj);
}
}