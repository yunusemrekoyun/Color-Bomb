using System.Collections;
using System.Collections.Generic;   // HashSet veya List gerekirse
using UnityEngine;

public class BalloonItem : MonoBehaviour
{
    private SwapManager swapManager;
    private HintManager hintManager;
    private DropManager dropManager; // ─── DropManager referansı

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

        // ─── dropManager referansını alalım
        dropManager = FindObjectOfType<DropManager>();
        if (dropManager == null) Debug.LogError("DropManager bulunamadı!");
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
            Debug.Log($"🚫 Bu balon donuk ve hareket ettirilemez: ({x},{y})");
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
            if (delta.x > 0) newX = x + 1;
            else newX = x - 1;
        }
        else
        {
            if (delta.y > 0) newY = y + 1;
            else newY = y - 1;
        }

        // sınırlar içinde mi?
        var board = swapManager.GetComponent<GameBoard>();
        if (newX < 0 || newX >= board.width || newY < 0 || newY >= board.height)
            return;

        swapManager.SwapBalloons(x, y, newX, newY);
        hintManager.ResetIdleTimer();
    }

    public void MoveTo(Vector3 target)
    {
        StopAllCoroutines();
        StartCoroutine(MoveRoutine(target));
    }

    private System.Collections.IEnumerator MoveRoutine(Vector3 target)
    {
        float t = 0f;
        Vector3 start = transform.position;

        // ─── 3.1: Animasyon başladı, DropManager.fallingItems'a ekle
        if (dropManager != null)
        {
            dropManager.RegisterFalling(gameObject);
        }
        else
        {
            var dm = FindObjectOfType<DropManager>();
            if (dm != null)
                dm.RegisterFalling(gameObject);
        }

        while (t < 1f)
        {
            t += Time.deltaTime * 10f;
            transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }
        transform.position = new Vector3(target.x, target.y, 0f);

        // ─── 3.2: Animasyon bitti, DropManager.fallingItems'dan çıkar
        if (dropManager != null)
        {
            dropManager.UnregisterFalling(gameObject);
        }
        else
        {
            var dm = FindObjectOfType<DropManager>();
            if (dm != null)
                dm.UnregisterFalling(gameObject);
        }
    }
}