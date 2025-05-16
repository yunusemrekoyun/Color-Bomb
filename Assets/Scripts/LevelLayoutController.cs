using System.Collections.Generic;
using UnityEngine;

public class LevelLayoutController : MonoBehaviour
{
    public List<GameObject> worldLayouts;

    void Start()
    {
        int selectedWorld = PlayerPrefs.GetInt("SelectedWorld", 0);
        ShowOnlySelectedWorld(selectedWorld);
    }

    void ShowOnlySelectedWorld(int index)
    {
        for (int i = 0; i < worldLayouts.Count; i++)
        {
            worldLayouts[i].SetActive(i == index);
        }
    }
}
