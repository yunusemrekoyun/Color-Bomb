using UnityEngine;

public class BalloonItem : MonoBehaviour
{
    private SwapManager swapManager;
    private HintManager hintManager;

    public bool isFrozen = false;
    public int x;
    public int y;

    private void Start()
    {
        Debug.Log($"[BalloonItem] Script çalıştı → {gameObject.name} | isFrozen = {isFrozen}");

        swapManager = FindFirstObjectByType<SwapManager>();
        hintManager = FindFirstObjectByType<HintManager>();

        if (swapManager == null) Debug.LogError("SwapManager bulunamadı!");
        if (hintManager == null) Debug.LogError("HintManager bulunamadı!");
    }

#if UNITY_EDITOR
    private Vector3 touchStart, touchEnd;

    private void OnMouseDown()
    {
        Debug.Log($"[BalloonItem] OnMouseDown: {gameObject.name}");
        touchStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        touchStart.z = 0f;
    }

    private void OnMouseUp()
    {
        if (isFrozen)
        {
            Debug.Log($"🚫 Bu balon donmuş ve hareket ettirilemez: ({x},{y})");
            return;
        }

        touchEnd = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        touchEnd.z = 0f;
        HandleSwipe(touchEnd - touchStart);
    }
#endif

    private void HandleSwipe(Vector2 delta)
    {
        if (swapManager == null) return;

        int newX = x, newY = y;
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            newX += delta.x > 0 ? 1 : -1;
        }
        else
        {
            newY += delta.y > 0 ? 1 : -1;
        }

        var board = swapManager.GetComponent<GameBoard>();
        if (board == null) return;

        if (newX < 0 || newX >= board.width || newY < 0 || newY >= board.height)
            return;

        swapManager.SwapBalloons(x, y, newX, newY);
        hintManager.ResetIdleTimer();
    }

    public void MoveTo(Vector3 target)
    {
        StopCoroutine("MoveRoutine"); // sadece kendi coroutine'ini durdur
        StartCoroutine("MoveRoutine", target);
    }

    private System.Collections.IEnumerator MoveRoutine(Vector3 target)
    {
        float t = 0f;
        Vector3 start = transform.position;
        while (t < 1f)
        {
            t += Time.deltaTime * 10f;
            transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }
        transform.position = new Vector3(target.x, target.y, 0f);
    }
}