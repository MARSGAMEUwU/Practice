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

        if (notificationText == null)
        {
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

            if (notificationText == null)
            {
                GameObject foundObj = GameObject.Find("NotificationText");
                if (foundObj != null)
                {
                    notificationText = foundObj.GetComponent<TextMeshProUGUI>();
                }
            }

            if (notificationText != null)
            {
                Debug.Log("<color=green>[WaveManager] ? UI таймера найден автоматически!</color>");
            }
            else
            {
                Debug.LogError("<color=red>[WaveManager] ? ОШИБКА: NotificationText не найден! Проверьте имена объектов в иерархии.</color>");
            }
        }

        if (notificationText != null)
        {
            notificationText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (isGameOver) return;

        totalTimer -= Time.deltaTime;

        if (notificationText != null)
        {
            notificationText.gameObject.SetActive(true);
            int minutes = Mathf.FloorToInt(totalTimer / 60f);
            int seconds = Mathf.FloorToInt(totalTimer % 60f);
            notificationText.text = $"{seconds}";
        }

        if (totalTimer <= 0f && !isVictorySequence)
        {
            StartCoroutine(SparkSequence());
            return;
        }

        if (isVictorySequence) return;

        if (!isSpawning && currentWaveIndex < waves.Length)
        {
            currentWaveTimer -= Time.deltaTime;

            if (currentWaveTimer <= 0f)
            {
                Debug.Log($"<color=yellow> Таймкод волны {currentWaveIndex + 1} истёк! Живых врагов: {enemiesAlive}. Запускаем следующую!</color>");
                StartCoroutine(StartNextWave());
                return;
            }
        }

        if (enemiesAlive == 0 && !isSpawning && currentWaveIndex < waves.Length)
        {
            StartCoroutine(StartNextWave());
        }
    }

    private IEnumerator SparkSequence()
    {
        isVictorySequence = true;
        isGameOver = true;

        if (notificationText != null)
        {
            notificationText.text = "";
            notificationText.gameObject.SetActive(true);
        }

        Debug.Log("<color=red> Устройство искрится!</color>");

        yield return new WaitForSeconds(sparkMessageDuration);

        Debug.Log($"<color=cyan> Загрузка сцены: {nextSceneName}</color>");
        
        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.TransitionToScene(nextSceneName);
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private IEnumerator StartNextWave()
    {
        if (currentWaveIndex >= waves.Length) yield break;

        isSpawning = true;

        Wave wave = waves[currentWaveIndex];
        Debug.Log($"<color=cyan> Начинается: {wave.waveName} (живых врагов на карте: {enemiesAlive})</color>");

        currentWaveTimer = wave.waveDuration;

        yield return new WaitForSeconds(timeBetweenWaves);

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

    public void OnEnemyDeath()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
    }

    public void GameOver()
    {
        isGameOver = true;
    }
}