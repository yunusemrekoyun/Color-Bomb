using UnityEngine;


public class VictoryPanelController : MonoBehaviour
{
    public GameObject victoryWindow; // VictoryWindow'u buraya dragle

    private void Start()
    {
        victoryWindow.transform.localScale = Vector3.zero; // başta görünmesin
        victoryWindow.SetActive(false);
    }

    public void ShowVictory()
    {
        victoryWindow.SetActive(true);
        victoryWindow.transform.localScale = Vector3.zero;
        LeanTween.scale(victoryWindow, Vector3.one, 0.4f).setEaseOutBack();
    }

    public void HideVictory()
    {
        LeanTween.scale(victoryWindow, Vector3.zero, 0.3f).setEaseInBack()
            .setOnComplete(() => victoryWindow.SetActive(false));
    }
}