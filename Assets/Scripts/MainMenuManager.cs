using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private string gameSceneName = "Scene2";
    [SerializeField] private SettingsUI settingsUI;

    public void StartGame()
    {
        Debug.Log("[MainMenu] Загрузка уровня...");

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.TransitionToScene(gameSceneName);
        }
        else
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }

    public void OpenSettings()
    {
        if (settingsUI != null) settingsUI.OpenSettings();
    }
}