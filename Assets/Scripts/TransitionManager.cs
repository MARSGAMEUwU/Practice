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
    [SerializeField] private Sprite[] lightningSprites;

    private Canvas canvas;
    private Image fadeImage;
    private bool isTransitioning = false;
    private PlayerController playerController;
    private Image lightningImage;

    private readonly float[] fadeOutSteps = { 0.25f, 0.5f, 0.75f, 1.0f };
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

        if (transitionAudio == null)
        {
            transitionAudio = gameObject.AddComponent<AudioSource>();
            transitionAudio.playOnAwake = false;
            transitionAudio.spatialBlend = 0f;
        }

        SetupCanvas();
    }

    private IEnumerator ShowLightningRoutine()
    {
        if (lightningSprites == null || lightningSprites.Length == 0)
            yield break;

        lightningImage.sprite =
            lightningSprites[Random.Range(0, lightningSprites.Length)];

        lightningImage.enabled = true;

        yield return new WaitForSecondsRealtime(0.05f);

        lightningImage.enabled = false;
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
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        fadeImage.raycastTarget = false;

        GameObject lightningObj = new GameObject("LightningImage");
        lightningObj.transform.SetParent(canvas.transform, false);

        lightningImage = lightningObj.AddComponent<Image>();
        lightningImage.raycastTarget = false;
        lightningImage.enabled = false;

        RectTransform lightningRt = lightningObj.GetComponent<RectTransform>();
        lightningRt.anchorMin = Vector2.zero;
        lightningRt.anchorMax = Vector2.one;
        lightningRt.offsetMin = Vector2.zero;
        lightningRt.offsetMax = Vector2.zero;

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
        Time.timeScale = 0f;
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();

        if (playerController != null)
            playerController.LockControlsWithoutMouse();

        fadeImage.raycastTarget = true;

        yield return new WaitForSecondsRealtime(0.1f);

        for (int i = 0; i < fadeOutSteps.Length; i++)
        {
            PlayStepSound(i);

            // Резко меняем прозрачность
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, fadeOutSteps[i]);
            StartCoroutine(ShowLightningRoutine());
            yield return new WaitForSecondsRealtime(stepDuration);
        }

        SceneManager.LoadScene(sceneName);
        Time.timeScale = 0f;
        yield return null;
        yield return null;

        for (int i = 0; i < fadeInSteps.Length; i++)
        {
            PlayStepSound(fadeOutSteps.Length + i);

            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, fadeInSteps[i]);
            StartCoroutine(ShowLightningRoutine());
            yield return new WaitForSecondsRealtime(stepDuration);
        }

        fadeImage.raycastTarget = false;
        if (playerController != null)
            playerController.UnlockControlsWhithoutMouse();
        Time.timeScale = 1f;
        isTransitioning = false;
    }

    private void PlayStepSound(int stepIndex)
    {
        if (transitionAudio == null) return;

        if (stepSounds != null && stepSounds.Length > 0)
        {
            AudioClip clip = stepSounds[stepIndex % stepSounds.Length];
            if (clip != null)
            {
                transitionAudio.PlayOneShot(clip, 0.3f);
            }
        }
    }
}