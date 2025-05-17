using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; // <-- Bunu ekle

public class WorldPreview : MonoBehaviour
{
    public Button worldButton;
    public Image worldImage;
    public TMP_Text worldLabel;
    public Image[] starImages;

    public int worldIndex; // Hangi dünyaya týklandýðýný bilmek için

    public void SetData(WorldData data, int index)
    {
        worldImage.sprite = data.worldPreview;
        worldLabel.text = "World " + (index + 1);
        worldIndex = index;

        for (int i = 0; i < starImages.Length; i++)
        {
            starImages[i].color = i < data.earnedStars ? Color.white : Color.gray;
        }

        worldButton.onClick.RemoveAllListeners();
        worldButton.onClick.AddListener(OnWorldSelected);
    }

    void OnWorldSelected()
    {
        Debug.Log("WORLD " + (worldIndex + 1) + " SEÇÝLDÝ");
        PlayerPrefs.SetInt("SelectedWorld", worldIndex); // Hangi dünya seçildiðini kaydet
        
        SceneManager.LoadScene("LevelSelectScene");
    }

}
