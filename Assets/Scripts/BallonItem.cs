using UnityEngine;
using System.Collections;

public class BalloonItem : MonoBehaviour
{
    private SwapManager swapManager;
    private HintManager hintManager;

    public bool isFrozen = false;
    public int x;
    public int y;

    private Vector3 touchStart, touchEnd;

    private void Start()
    {
        Debug.Log($"[BalloonItem] Script çalıştı → {gameObject.name} | isFrozen = {isFrozen}");

        swapManager = FindFirstObjectByType<SwapManager>();
        hintManager = FindFirstObjectByType<HintManager>();

        if (swapManager == null) Debug.LogError("SwapManager bulunamadı!");
        if (hintManager == null) Debug.LogError("HintManager bulunamadı!");
    }

    private void OnMouseDown()
    {
        if (isFrozen) return;
        touchStart = Input.mousePosition;
    }

    private void OnMouseUp()
    {
        if (isFrozen) return;
        touchEnd = Input.mousePosition;
        Vector2 delta = Camera.main.ScreenToWorldPoint(touchEnd) - Camera.main.ScreenToWorldPoint(touchStart);
        HandleSwipe(delta);
    }

    private void Update()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            Vector3 worldTouch = Camera.main.ScreenToWorldPoint(touch.position);
            worldTouch.z = 0f;

            if (touch.phase == TouchPhase.Began)
            {
                touchStart = worldTouch;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                touchEnd = worldTouch;
                Vector2 delta = touchEnd - touchStart;
                HandleSwipe(delta);
            }
        }
    }

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
        StopCoroutine("MoveRoutine");
        StartCoroutine("MoveRoutine", target);
    }

    private IEnumerator MoveRoutine(Vector3 target)
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