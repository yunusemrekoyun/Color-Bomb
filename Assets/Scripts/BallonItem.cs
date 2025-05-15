using UnityEngine;

public class BalloonItem : MonoBehaviour
{
    private Vector3 startPos;
    private Vector3 touchStart;
    private Vector3 touchEnd;
    private BoardManager board;

    public int x;
    public int y;

    private void Start()
    {
        board = Object.FindFirstObjectByType<BoardManager>();
    }

#if UNITY_EDITOR
    // Sadece editörde mouse testine izin ver
    private void OnMouseDown()
    {
        touchStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        touchStart.z = 0f;
    }

    private void OnMouseUp()
    {
        touchEnd = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        touchEnd.z = 0f;
        HandleSwipe(touchEnd - touchStart);
    }
#endif

    void Update()
    {
#if !UNITY_EDITOR
        // Sadece mobilde çalışır
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 wp = Camera.main.ScreenToWorldPoint(touch.position);
            wp.z = 0f;

            if (touch.phase == TouchPhase.Began)
            {
                Collider2D hit = Physics2D.OverlapPoint(wp);
                if (hit != null && hit.gameObject == this.gameObject)
                {
                    touchStart = wp;
                }
            }

            if (touch.phase == TouchPhase.Ended)
            {
                Collider2D hit = Physics2D.OverlapPoint(wp);
                if (hit != null && hit.gameObject == this.gameObject)
                {
                    touchEnd = wp;
                    HandleSwipe(touchEnd - touchStart);
                }
            }
        }
#endif
    }

    void HandleSwipe(Vector2 delta)
    {
        if (board == null) return;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            // Yatay
            if (delta.x > 0 && x < board.width - 1)
                board.SwapBalloons(x, y, x + 1, y); // sağ
            else if (delta.x < 0 && x > 0)
                board.SwapBalloons(x, y, x - 1, y); // sol
        }
        else
        {
            // Dikey
            if (delta.y > 0 && y < board.height - 1)
                board.SwapBalloons(x, y, x, y + 1); // yukarı
            else if (delta.y < 0 && y > 0)
                board.SwapBalloons(x, y, x, y - 1); // aşağı
        }
        if (board != null)
        {
            board.ResetIdleTimer(); // kullanıcı hamle yaptı  süreyi sıfırla
        }
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

        while (t < 1f)
        {
            t += Time.deltaTime * 10f;
            transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.position = target;
    }
}