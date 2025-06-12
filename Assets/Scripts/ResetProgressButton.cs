using System.IO;  // eğer doğrudan File kullanmak istersen
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetProgressButton : MonoBehaviour
{
    public void ResetAllProgress()
    {
        // 1) PlayerPrefs’i temizle
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // 2) SaveManager ile JSON’u ve RAM’i sıfırla
        if (SaveManager.Instance != null)
            SaveManager.Instance.ResetProgress();

        Debug.Log("⚠️ Tüm ilerleme verileri sıfırlandı.");

        // 3) Sahneyi yeniden yükle (yeniden başlat)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}