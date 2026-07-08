using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }

    [Header("Настройки перехода")]
    [SerializeField] private Color fadeColor = Color.black;

    [Header("Пошаговая анимация (Секретный режим)")]
    [Tooltip("Задержка между каждым 'шагом' (щелчком) в секундах")]
    [SerializeField] private float stepDuration = 0.12f;

    [Header("Звуки шагов")]
    [Tooltip("AudioSource для воспроизведения звуков (создастся автоматически, если пуст)")]
    [SerializeField] private AudioSource transitionAudio;

    [Tooltip("Массив звуков. Если назначен 1 звук - он будет повторяться на каждом шаге. Если 4 - они проиграются по очереди (щелчок, щелчок, щелчок, УДАР).")]
    [SerializeField] private AudioClip[] stepSounds;

    private Canvas canvas;
    private Image fadeImage;
    private bool isTransitioning = false;
    private PlayerController playerController;

    // Массивы шагов прозрачности (от 0 до 1 и обратно)
    // 4 шага затемнения: 0.25 -> 0.5 -> 0.75 -> 1.0
    private readonly float[] fadeOutSteps = { 0.25f, 0.5f, 0.75f, 1.0f };
    // 4 шага рассветления: 0.75 -> 0.5 -> 0.25 -> 0.0
    private readonly float[] fadeInSteps = { 0.75f, 0.5f, 0.25f, 0.0f };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Создаем AudioSource, если забыли назначить
        if (transitionAudio == null)
        {
            transitionAudio = gameObject.AddComponent<AudioSource>();
            transitionAudio.playOnAwake = false;
            transitionAudio.spatialBlend = 0f; // 2D звук
        }

        SetupCanvas();
    }

    private void SetupCanvas()
    {
        GameObject canvasObj = new GameObject("TransitionCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        canvasObj.transform.SetParent(transform);

        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform, false);
        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f); // Изначально прозрачный
        fadeImage.raycastTarget = false;

        RectTransform rt = imageObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    public void TransitionToScene(string sceneName)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionRoutine(sceneName));
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        isTransitioning = true;

        // 1. БЛОКИРОВКА ГЕЙМПЛЕЯ
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();

        if (playerController != null)
            playerController.LockControls();

        fadeImage.raycastTarget = true; // Блокируем клики

        // Небольшая пауза перед началом, чтобы игрок успел увидеть кадр
        yield return new WaitForSecondsRealtime(0.1f);

        // ==========================================
        // 2. ЗАТЕМНЕНИЕ (4 ШАГА)
        // ==========================================
        for (int i = 0; i < fadeOutSteps.Length; i++)
        {
            // Проигрываем звук ПЕРЕД изменением цвета (или после, на вкус)
            PlayStepSound(i);

            // Резко меняем прозрачность
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, fadeOutSteps[i]);

            // Ждем перед следующим щелчком (Realtime, чтобы работало на паузе/при смерти)
            yield return new WaitForSecondsRealtime(stepDuration);
        }

        // Экран полностью черный. Загружаем сцену.
        SceneManager.LoadScene(sceneName);

        // Ждем 2 кадра, чтобы новая сцена проинициализировалась
        yield return null;
        yield return null;

        // ==========================================
        // 3. РАССВЕТЛЕНИЕ (4 ШАГА)
        // ==========================================
        for (int i = 0; i < fadeInSteps.Length; i++)
        {
            // Продолжаем нумерацию звуков (или зацикливаем, если их меньше 8)
            PlayStepSound(fadeOutSteps.Length + i);

            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, fadeInSteps[i]);

            yield return new WaitForSecondsRealtime(stepDuration);
        }

        // ==========================================
        // 4. РАЗБЛОКИРОВКА
        // ==========================================
        fadeImage.raycastTarget = false;
        if (playerController != null)
            playerController.UnlockControls();

        isTransitioning = false;
    }

    /// <summary>
    /// Проигрывает звук для текущего шага. 
    /// Если звуков несколько - берет по индексу. Если один - повторяет его.
    /// </summary>
    private void PlayStepSound(int stepIndex)
    {
        if (transitionAudio == null) return;

        if (stepSounds != null && stepSounds.Length > 0)
        {
            // Берем звук по индексу. Если массив короткий, используем остаток от деления (зацикливаем)
            AudioClip clip = stepSounds[stepIndex % stepSounds.Length];
            if (clip != null)
            {
                transitionAudio.PlayOneShot(clip);
            }
        }
    }
}