using UnityEngine;
using UnityEngine.SceneManagement; // Нужно для перезагрузки уровня

public class GameManager : MonoBehaviour
{
    [Header("Настройки")]
    public static GameManager Instance { get; private set; } // Тот самый Синглтон

    // Перечисление всех возможных состояний игры
    public enum GameState { Menu, Playing, Paused, GameOver }
    public GameState State { get; private set; } // Текущее состояние

    private void Awake()
    {
        // === НАСТРОЙКА СИНГЛТОНА ===
        // Если менеджер уже есть, а мы пытаемся создать второго (например, при загрузке сцены) — уничтожаем дубликат
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // (Опционально) Делаем так, чтобы менеджер не уничтожался при переходе между уровнями
        // DontDestroyOnLoad(gameObject); 
    }

    private void Start()
    {
        // При старте уровня сразу переводим игру в режим "Играем"
        UpdateGameState(GameState.Playing);
    }

    // === ГЛАВНЫЙ ЦЕНТР УПРАВЛЕНИЯ ===
    public void UpdateGameState(GameState newState)
    {
        State = newState;

        switch (newState)
        {
            case GameState.Menu:
                // Логика для главного меню
                Time.timeScale = 1f;
                break;
            case GameState.Playing:
                // Обычная игра
                Time.timeScale = 1f;
                break;
            case GameState.Paused:
                // Ставим игру на паузу (останавливаем время)
                Time.timeScale = 0f;
                break;
            case GameState.GameOver:
                // Игрок умер. Останавливаем время и показываем экран проигрыша
                Time.timeScale = 0f;
                Debug.Log("<color=red>ИГРА ОКОНЧЕНА!</color>");
                // Здесь можно включить UI-панель смерти: gameOverPanel.SetActive(true);
                break;
        }
    }

    // === ПУБЛИЧНЫЕ МЕТОДЫ ДЛЯ ДРУГИХ СКРИПТОВ ===

    public void OnPlayerDied()
    {
        UpdateGameState(GameState.GameOver);
    }

    public void RestartGame()
    {
        // Возвращаем время в норму и перезагружаем текущую сцену
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void TogglePause()
    {
        // Переключатель паузы на кнопку ESC
        if (State == GameState.Playing)
            UpdateGameState(GameState.Paused);
        else if (State == GameState.Paused)
            UpdateGameState(GameState.Playing);
    }
}