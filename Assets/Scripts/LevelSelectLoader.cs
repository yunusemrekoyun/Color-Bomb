using UnityEngine;

public class LevelSelectLoader : MonoBehaviour
{
    public GameObject[] worldLayouts; // World1Layout, World2Layout, World3Layout gibi

    void Start()
    {
        int selectedWorld = PlayerPrefs.GetInt("SelectedWorld", 0); // varsayýlan 0
        for (int i = 0; i < worldLayouts.Length; i++)
        {
            worldLayouts[i].SetActive(i == selectedWorld);
        }
    }
}
