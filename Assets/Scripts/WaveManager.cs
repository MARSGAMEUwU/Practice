using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    // Делаем Синглтон, чтобы враги могли сообщать ему о своей смерти
    public static WaveManager Instance { get; private set; }

    [System.Serializable]
    public class Wave
    {
        public string waveName = "Волна 1";
        [Tooltip("Какие враги могут спавниться (менеджер выберет случайно из списка)")]
        public GameObject[] enemyPrefabs;
        [Tooltip("Сколько всего врагов за волну")]
        public int enemyCount;
        [Tooltip("Задержка между спавном врагов (в секундах)")]
        public float timeBetweenSpawns = 1.5f;
    }

    [Header("Настройки спавна")]
    [SerializeField] private Wave[] waves;
    [SerializeField] private Transform[] spawnPoints; // Пустышки на сцене, откуда лезут враги
    [SerializeField] private float timeBetweenWaves = 5f; // Время на передышку

    private int currentWaveIndex = 0;
    private int enemiesAlive = 0;
    private bool isSpawning = false; // Защита от двойного запуска

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        // Если игра на паузе или игрок мертв — волны не идут
        if (GameManager.Instance.State != GameManager.GameState.Playing) return;

        // Если все враги мертвы, мы сейчас ничего не спавним, и волны еще не закончились
        if (enemiesAlive == 0 && !isSpawning && currentWaveIndex < waves.Length)
        {
            StartCoroutine(StartNextWave());
        }
        else if (enemiesAlive == 0 && !isSpawning && currentWaveIndex >= waves.Length)
        {
            Debug.Log("<color=green>ПОБЕДА! ВСЕ ВОЛНЫ ПРОЙДЕНЫ!</color>");
            // Тут можно вызвать GameManager.Instance.OnVictory()
        }
    }

    private IEnumerator StartNextWave()
    {
        isSpawning = true;

        // Пауза перед началом новой волны (чтобы игрок успел перезарядиться)
        yield return new WaitForSeconds(timeBetweenWaves);

        Wave wave = waves[currentWaveIndex];
        Debug.Log($"<color=cyan>Начинается: {wave.waveName}</color>");

        // Цикл спавна врагов
        for (int i = 0; i < wave.enemyCount; i++)
        {
            // Проверка, если вдруг игрок умер во время спавна волны
            if (GameManager.Instance.State != GameManager.GameState.Playing) yield break;

            SpawnEnemy(wave.enemyPrefabs);

            // Ждем перед спавном следующего
            yield return new WaitForSeconds(wave.timeBetweenSpawns);
        }

        // Подготавливаем индекс для следующей волны
        currentWaveIndex++;
        isSpawning = false;
    }

    private void SpawnEnemy(GameObject[] prefabs)
    {
        if (prefabs.Length == 0 || spawnPoints.Length == 0) return;

        // Выбираем случайную точку спавна
        Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
        // Выбираем случайного врага из разрешенных в этой волне
        GameObject prefabToSpawn = prefabs[Random.Range(0, prefabs.Length)];

        Instantiate(prefabToSpawn, sp.position, sp.rotation);

        // Увеличиваем счетчик живых врагов
        enemiesAlive++;
    }

    // Этот метод будут вызывать враги перед своей смертью
    public void OnEnemyDeath()
    {
        enemiesAlive--;
    }
}