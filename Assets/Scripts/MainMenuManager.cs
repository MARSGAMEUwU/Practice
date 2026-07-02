using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private string gameSceneName = "Scene2";

    public void StartGame()
    {
        Debug.Log("[MainMenu] Запуск игры...");
        SceneManager.LoadScene(gameSceneName);
    }
}