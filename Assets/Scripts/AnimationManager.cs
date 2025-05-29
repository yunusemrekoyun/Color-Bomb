using UnityEngine;
using System.Collections;

public class AnimationManager : MonoBehaviour
{
    public static AnimationManager Instance;

    [Header("▶ Animation Prefab")]
    public GameObject animationPrefab;  // Sahneye Instantiate edilecek prefab

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void PlayEffect(string triggerName, Vector3 position)
    {
        Debug.Log($"🎬 Animasyon çağrıldı → Trigger: {triggerName}, Pos: {position}");

        GameObject obj = Instantiate(animationPrefab, position, Quaternion.identity);
        Animator anim = obj.GetComponent<Animator>();

        if (anim != null)
        {
            StartCoroutine(TriggerAfterOneFrame(anim, triggerName));
        }

        // Animasyon 1 saniyeyse, 1.5 saniyede yok et
        Destroy(obj, 1.5f);
    }

    private IEnumerator TriggerAfterOneFrame(Animator anim, string trigger)
    {
        yield return null; // Bir frame bekle ki Animator aktifleşsin
        anim.SetTrigger(trigger);
    }
}