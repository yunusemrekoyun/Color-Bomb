using UnityEngine;

public class SpecialItem : MonoBehaviour
{
    public enum SpecialState
    {
        Horizontal5,
        Vertical5,
        Square4,
        Horizontal4,
        Vertical4
    }

    [Header("This special’s type")]
    public SpecialState state;

    // Mouse kontrolü
#if UNITY_EDITOR
    private void OnMouseDown()
    {
        Debug.Log($"SpecialItem dragged: {state}");
    }
#endif

    // Touch kontrolü (mobil)
    void Update()
    {
#if !UNITY_EDITOR
        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);
            Vector3 wp = Camera.main.ScreenToWorldPoint(t.position);
            wp.z = 0f;

            if (t.phase == TouchPhase.Began)
            {
                Collider2D hit = Physics2D.OverlapPoint(wp);
                if (hit != null && hit.gameObject == gameObject)
                    Debug.Log($"SpecialItem touch began: {state}");
            }
        }
#endif
    }
}