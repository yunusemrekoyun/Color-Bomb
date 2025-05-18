using UnityEngine;
using UnityEngine.SceneManagement;

public class OpeningSceneController : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("WorldSelectScene");
    }
}