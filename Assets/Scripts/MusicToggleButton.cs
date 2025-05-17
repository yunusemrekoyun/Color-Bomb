using UnityEngine;
using UnityEngine.UI;

public class MusicToggleButton : MonoBehaviour
{
    public Sprite musicOnIcon;
    public Sprite musicOffIcon;
    public Image iconImage;

    private bool isMusicOn = true;

    void Start()
    {
        isMusicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        UpdateIcon();
    }

    public void ToggleMusic()
    {
        isMusicOn = !isMusicOn;
        PlayerPrefs.SetInt("MusicOn", isMusicOn ? 1 : 0);
        PlayerPrefs.Save();

        UpdateIcon();

        Debug.Log(isMusicOn ? "Müzik Açýk" : "Müzik Kapalý");
    }

    void UpdateIcon()
    {
        iconImage.sprite = isMusicOn ? musicOnIcon : musicOffIcon;
    }
}
