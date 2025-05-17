using UnityEngine;

public class ResetProgressButton : MonoBehaviour
{
    public void ResetAllProgress()
    {
        PlayerPrefs.DeleteAll(); // Tüm kayýtlarý siler
        PlayerPrefs.Save();

        Debug.Log("Tüm ilerleme verileri sýfýrlandý.");

        // Ýsteðe baðlý: Sahneyi yeniden yükle (refresh gibi)
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
