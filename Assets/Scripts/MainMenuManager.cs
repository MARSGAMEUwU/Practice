using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private string gameSceneName = "Scene2";

    // Добавь в раздел [Header("Настройки")] или в начало:
    [SerializeField] private SettingsUI settingsUI;

    public void StartGame()
    {
        Debug.Log("[MainMenu] Запуск игры...");
        SceneManager.LoadScene(gameSceneName);
    }
    public void OpenSettings()
    {
        if (settingsUI != null)
        {
            settingsUI.OpenSettings();
        }
    }
}