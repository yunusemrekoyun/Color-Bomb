using UnityEngine;
using UnityEngine.UI;

public class SoundToggleButton : MonoBehaviour
{
    public Sprite soundOnIcon;
    public Sprite soundOffIcon;
    public Image iconImage;

    private bool isSoundOn = true;

    void Start()
    {
        isSoundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;
        UpdateIcon();
    }

    public void ToggleSound()
    {
        isSoundOn = !isSoundOn;
        PlayerPrefs.SetInt("SoundOn", isSoundOn ? 1 : 0);
        PlayerPrefs.Save();

        UpdateIcon();

        Debug.Log(isSoundOn ? "Ses Efektleri Açýk" : "Ses Efektleri Kapalý");
    }

    void UpdateIcon()
    {
        iconImage.sprite = isSoundOn ? soundOnIcon : soundOffIcon;
    }
}
