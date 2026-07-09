using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [System.Serializable]
    public class Wave
    {
        public string waveName = "Волна 1";
        public GameObject[] enemyPrefabs;
        public int enemyCount;
        public float timeBetweenSpawns = 1.5f;

        [Tooltip("Таймкод: через сколько секунд ПРИНУДИТЕЛЬНО начнётся следующая волна (даже если эта не зачищена)")]
        public float waveDuration = 60f;
    }

    [Header("Волны и спавн")]
    [SerializeField] private Wave[] waves;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float timeBetweenWaves = 3f;

    [Header("Общий таймер уровня")]
    [Tooltip("Общее время на весь уровень (секунды)")]
    [SerializeField] private float totalLevelTime = 300f;

    [Header("Финальная сцена")]
    [SerializeField] private string nextSceneName = "MainMenu";
    [Tooltip("Сколько секунд показывать 'Устройство искрится' перед загрузкой")]
    [SerializeField] private float sparkMessageDuration = 3f;

    [Header("UI — обратный отсчёт")]
    [Tooltip("TextMeshPro для отображения таймера и сообщения")]
    [SerializeField] private TextMeshProUGUI notificationText;

    // === СОСТОЯНИЕ ===
    private int currentWaveIndex = 0;
    private int enemiesAlive = 0;
    private bool isSpawning = false;
    private bool isGameOver = false;
    private bool isVictorySequence = false;

    private float currentWaveTimer = 0f;
    private float totalTimer = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        totalTimer = totalLevelTime;

        // === АВТОМАТИЧЕСКИЙ ПОИСК UI (ЕСЛИ НЕ НАЗНАЧЕН В ИНСПЕКТОРЕ) ===
        if (notificationText == null)
        {
            // 1. Пытаемся найти по точному пути в иерархии: Canvas -> NotificationPanel -> NotificationText
            Transform canvas = GameObject.Find("Canvas")?.transform;
            if (canvas != null)
            {
                Transform panel = canvas.Find("NotificationPanel");
                if (panel != null)
                {
                    Transform textObj = panel.Find("NotificationText");
                    if (textObj != null)
                    {
                        notificationText = textObj.GetComponent<TextMeshProUGUI>();
                    }
                }
            }

            // 2. Если по пути не нашли, пробуем глобальный поиск по имени объекта
            if (notificationText == null)
            {
                GameObject foundObj = GameObject.Find("NotificationText");
                if (foundObj != null)
                {
                    notificationText = foundObj.GetComponent<TextMeshProUGUI>();
                }
            }

            // Выводим результат поиска в консоль для отладки
            if (notificationText != null)
            {
                Debug.Log("<color=green>[WaveManager] ? UI таймера найден автоматически!</color>");
            }
            else
            {
                Debug.LogError("<color=red>[WaveManager] ? ОШИБКА: NotificationText не найден! Проверьте имена объектов в иерархии.</color>");
            }
        }

        // Скрываем текст в начале игры
        if (notificationText != null)
        {
            notificationText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (isGameOver) return;

        // ========================================
        // 1. ОБЩИЙ ТАЙМЕР УРОВНЯ (обратный отсчёт)
        // ========================================
        totalTimer -= Time.deltaTime;

        // Показываем обратный отсчёт на UI
        if (notificationText != null)
        {
            notificationText.gameObject.SetActive(true);
            int minutes = Mathf.FloorToInt(totalTimer / 60f);
            int seconds = Mathf.FloorToInt(totalTimer % 60f);
            notificationText.text = $"{seconds}";
        }

        // Общий таймер истёк — запускаем финальную последовательность
        if (totalTimer <= 0f && !isVictorySequence)
        {
            StartCoroutine(SparkSequence());
            return;
        }

        if (isVictorySequence) return;

        // ========================================
        // 2. ТАЙМКОД ТЕКУЩЕЙ ВОЛНЫ
        // ========================================
        if (!isSpawning && currentWaveIndex < waves.Length)
        {
            currentWaveTimer -= Time.deltaTime;

            // Таймкод истёк — принудительно начинаем СЛЕДУЮЩУЮ волну (старые враги остаются!)
            if (currentWaveTimer <= 0f)
            {
                Debug.Log($"<color=yellow> Таймкод волны {currentWaveIndex + 1} истёк! Живых врагов: {enemiesAlive}. Запускаем следующую!</color>");
                StartCoroutine(StartNextWave());
                return;
            }
        }

        // ========================================
        // 3. ОБЫЧНАЯ ПРОВЕРКА — все враги мертвы
        // ========================================
        if (enemiesAlive == 0 && !isSpawning && currentWaveIndex < waves.Length)
        {
            StartCoroutine(StartNextWave());
        }
        else if (enemiesAlive == 0 && !isSpawning && currentWaveIndex >= waves.Length)
        {
            // Все волны зачищены досрочно — тоже запускаем финал
            Debug.Log("<color=green> Все волны зачищены досрочно!</color>");
            StartCoroutine(SparkSequence());
        }
    }

    /// <summary>
    /// Финальная последовательность: "Устройство искрится" ? загрузка сцены
    /// </summary>
    private IEnumerator SparkSequence()
    {
        isVictorySequence = true;
        isGameOver = true;

        // Показываем сообщение
        if (notificationText != null)
        {
            notificationText.text = "";
            notificationText.gameObject.SetActive(true);
        }

        Debug.Log("<color=red> Устройство искрится!</color>");

        // Ждём указанное время
        yield return new WaitForSeconds(sparkMessageDuration);

        // Загружаем следующую сцену
        Debug.Log($"<color=cyan> Загрузка сцены: {nextSceneName}</color>");
        // === ЭФЕКТ ПЕРЕХОДА ===
        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.TransitionToScene(nextSceneName);
        }
        else
        {
            // Запасной вариант, если менеджер переходов по какой-то причине не найден
            SceneManager.LoadScene(nextSceneName);
        }
    }

    /// <summary>
    /// Запускает спавн текущей волны (не убивает старых врагов!)
    /// </summary>
    private IEnumerator StartNextWave()
    {
        if (currentWaveIndex >= waves.Length) yield break;

        isSpawning = true;

        Wave wave = waves[currentWaveIndex];
        Debug.Log($"<color=cyan> Начинается: {wave.waveName} (живых врагов на карте: {enemiesAlive})</color>");

        // Ставим таймкод для ЭТОЙ волны
        currentWaveTimer = wave.waveDuration;

        // Пауза перед спавном
        yield return new WaitForSeconds(timeBetweenWaves);

        // Спавним врагов
        for (int i = 0; i < wave.enemyCount; i++)
        {
            if (isGameOver) yield break;
            SpawnEnemy(wave.enemyPrefabs);
            yield return new WaitForSeconds(wave.timeBetweenSpawns);
        }

        currentWaveIndex++;
        isSpawning = false;
    }

    private void SpawnEnemy(GameObject[] prefabs)
    {
        if (prefabs.Length == 0 || spawnPoints.Length == 0) return;

        Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject prefabToSpawn = prefabs[Random.Range(0, prefabs.Length)];
        Instantiate(prefabToSpawn, sp.position, sp.rotation);

        enemiesAlive++;
    }

    // === ПУБЛИЧНЫЕ МЕТОДЫ ===

    /// <summary>
    /// Враги вызывают это перед уничтожением
    /// </summary>
    public void OnEnemyDeath()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
    }

    /// <summary>
    /// Вызывается при смерти игрока
    /// </summary>
    public void GameOver()
    {
        isGameOver = true;
    }
}