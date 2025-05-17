using UnityEngine;

public class SettingsPanelController : MonoBehaviour
{
    private Animator animator;
    private bool isOpen = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void ToggleSettings()
    {
        if (isOpen)
        {
            // Paneli kapat
            gameObject.SetActive(false);
        }
        else
        {
            // Paneli görünür yap ve animasyonu oynat
            gameObject.SetActive(true);
            animator.Play("SlideIn");
        }

        isOpen = !isOpen;
    }
}
