using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PersistentManager : MonoBehaviour
{
    [Header("Ссылки на корневые объекты (Перетащите из Hierarchy!)")]
    [Tooltip("Самый верхний объект Player в иерархии")]
    [SerializeField] private GameObject playerRoot;
    [Tooltip("Самый верхний объект Canvas")]
    [SerializeField] private GameObject canvasRoot;
    [Tooltip("Объект, на котором висит InventoryManager")]
    [SerializeField] private GameObject inventoryRoot;
    [Tooltip("Объект EventSystem")]
    [SerializeField] private GameObject eventSystemRoot;

    // Статическая переменная, чтобы знать, что система уже инициализирована
    private static bool isInitialized = false;

    private void Awake()
    {
        // Если мы уже делали объекты бессмертными (например, при перезагрузке сцены)
        if (isInitialized)
        {
            // Уничтожаем дубликаты, если они случайно попали в новую сцену
            CleanUpDuplicates();
            Destroy(gameObject); // Уничтожаем сам менеджер, он больше не нужен
            return;
        }

        // === ДЕЛАЕМ ОБЪЕКТЫ БЕССМЕРТНЫМИ ===
        if (playerRoot != null) DontDestroyOnLoad(playerRoot);
        if (canvasRoot != null) DontDestroyOnLoad(canvasRoot);
        if (inventoryRoot != null) DontDestroyOnLoad(inventoryRoot);
        if (eventSystemRoot != null) DontDestroyOnLoad(eventSystemRoot);

        // Подписываемся на загрузку сцен, чтобы автоматически чистить мусор
        SceneManager.sceneLoaded += OnSceneLoaded;

        isInitialized = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // При загрузке новой локации вызываем чистку
        CleanUpScene();
    }

    private void CleanUpScene()
    {
        // 1. Удаляем лишние EventSystem (Unity любит создавать их в новых сценах)
        EventSystem[] allEventSystems = FindObjectsOfType<EventSystem>();
        foreach (var es in allEventSystems)
        {
            if (es.gameObject != eventSystemRoot)
            {
                Destroy(es.gameObject);
            }
        }

        // 2. Удаляем лишние AudioListener (иначе будет ошибка "There are 2 audio listeners")
        AudioListener[] allListeners = FindObjectsOfType<AudioListener>();
        for (int i = 1; i < allListeners.Length; i++)
        {
            Destroy(allListeners[i].gameObject); // Оставляем только один (на камере игрока)
        }
    }

    private void CleanUpDuplicates()
    {
        // На случай, если вы случайно скопировали Player или Canvas в новую сцену-локацию
        // Этот код найдет и удалит чужеродные объекты, не трогая наши "вечные"

        // Проверка Player (по тегу)
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var p in players)
        {
            if (p.transform.root.gameObject != playerRoot) Destroy(p.transform.root.gameObject);
        }

        // Проверка Canvas
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (var c in canvases)
        {
            if (c.transform.root.gameObject != canvasRoot) Destroy(c.transform.root.gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}