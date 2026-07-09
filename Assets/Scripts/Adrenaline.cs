using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Adrenaline : MonoBehaviour
{
    [Header("Основные настройки")]
    [SerializeField] private float maxAdrenaline = 100f;
    [SerializeField] public float currentAdrenaline = 4f;
    [SerializeField] private float decayRate = 1f;
    [SerializeField] private float killReward = 20f;

    [Header("Шприцы")]
    [SerializeField] private float injectionBoost = 50f;
    [SerializeField] private int syringeAmount = 4;
    [SerializeField] private float cooldown = 5f;
    [SerializeField] private InputAction useSyringe;

    [Header("3D Анимация и задержка инъекции")]
    [Tooltip("Animator руки/оружия, на котором висит анимация укола")]
    [SerializeField] private Animator injectorAnimator;
    [Tooltip("Имя триггера в Animator, который запускает анимацию")]
    [SerializeField] private string injectTriggerName = "Inject";
    [Tooltip("Задержка в секундах перед началом восстановления (должна совпадать с моментом 'втыкания' в анимации)")]
    [SerializeField] private float injectionDelay = 0.6f;

    [Header("Effects")]
    [SerializeField] private Material shader;
    [SerializeField] private float maxSaturation = 2f;
    [SerializeField] private float minSaturation = 0.5f;
    [SerializeField] private float maxContrast = 2f;
    [SerializeField] private float minContrast = 1f;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float minFov = 30f;
    [SerializeField] private float maxFov = 100f;

    [Header("Music")]
    [SerializeField] public AudioSource track1;
    [SerializeField] public AudioSource track2;
    [SerializeField] public AudioSource track3;
    [SerializeField] public AudioSource track4;
    [SerializeField] public float volume = 0.3f;

    [Header("UI")]
    [SerializeField] private AdrenalineUI adrenalineUI;
    [SerializeField] private SyringeUI syringeUI;

    public float AdrenalinePercentage => currentAdrenaline / maxAdrenaline;

    private float nextInjTime;
    private float currentSaturation;
    private float currentContrast;
    private float currentFov;

    private void Awake()
    {
        currentSaturation = 0.5f;
        currentContrast = 1f;
        if (shader != null)
        {
            shader.SetFloat("_Saturation", currentSaturation);
            shader.SetFloat("_Contrast", currentContrast);
        }

        // === ФИКС МУЗЫКИ: СРАЗУ МЬЮТИМ ТРЕКИ ПРИ СПАВНЕ, ЧТОБЫ НЕ БЫЛО ВЗРЫВА ЗВУКА ===
        if (track1 != null) track1.volume = 0f;
        if (track2 != null) track2.volume = 0f;
        if (track3 != null) track3.volume = 0f;
        if (track4 != null) track4.volume = 0f;
    }

    private void Start()
    {
        // === СИНХРОНИЗАЦИЯ С ГЛОБАЛЬНЫМ ИНВЕНТАРЕМ ПРИ СПАВНЕ ===
        if (InventoryManager.Instance != null)
        {
            currentAdrenaline = InventoryManager.Instance.savedAdrenaline;
            syringeAmount = InventoryManager.Instance.savedSyringes;
            Debug.Log($"<color=cyan>[Adrenaline] Загружено из глобала: Адреналин={currentAdrenaline}, Шприцы={syringeAmount}</color>");
        }

        // Сразу применяем эффекты и музыку, чтобы не ждать первого кадра Update
        ApplyVisualEffects();
        ApplyMusicVolumes();
    }

    private void OnEnable()
    {
        if (useSyringe != null) { useSyringe.Enable(); }
    }

    private void OnDisable()
    {
        if (useSyringe != null) { useSyringe.Disable(); }
    }

    void Update()
    {
        // === ЖЕСТКАЯ ЗАЩИТА ОТ ПАУЗЫ ===
        if (Time.timeScale <= 0f) return;

        if (currentAdrenaline >= 0)
        {
            currentAdrenaline -= decayRate * Time.deltaTime;

            // === ИСПРАВЛЕНИЕ №2: Clamp от 0f, а не от 1f! ===
            currentAdrenaline = Mathf.Clamp(currentAdrenaline, 0f, maxAdrenaline);

            ApplyVisualEffects();
        }
        else
        {
            // Если адреналин упал в абсолютный ноль, вызываем смерть
            GameOver();
        }

        if (useSyringe.IsPressed() && syringeAmount > 0 && Time.time >= nextInjTime)
        {
            UseSyringe();
        }

        ApplyMusicVolumes();

        // Сохраняем текущее состояние в глобальный инвентарь каждый кадр
        SyncToGlobal();
    }

    private void ApplyVisualEffects()
    {
        currentSaturation = Mathf.Lerp(minSaturation, maxSaturation, AdrenalinePercentage);
        currentContrast = Mathf.Lerp(minContrast, maxContrast, AdrenalinePercentage);
        if (shader != null)
        {
            shader.SetFloat("_Saturation", currentSaturation);
            shader.SetFloat("_Contrast", currentContrast);
        }

        currentFov = Mathf.Lerp(minFov, maxFov, AdrenalinePercentage);
        if (mainCamera != null) mainCamera.fieldOfView = currentFov;
    }

    private void ApplyMusicVolumes()
    {
        if (track1 != null) track1.volume = volume;
        if (track2 != null) track2.volume = Mathf.InverseLerp(10f, 30f, currentAdrenaline) * volume;
        if (track3 != null) track3.volume = Mathf.InverseLerp(30f, 60f, currentAdrenaline) * volume;
        if (track4 != null) track4.volume = Mathf.InverseLerp(60f, 90f, currentAdrenaline) * volume;
    }

    private void SyncToGlobal()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.savedAdrenaline = currentAdrenaline;
            InventoryManager.Instance.savedSyringes = syringeAmount;
        }
    }

    // === ИСПРАВЛЕННАЯ КОРУТИНА ЛЕЧЕНИЯ ===
    private System.Collections.IEnumerator SmoothHealRoutine(float amountToHeal)
    {
        // 1. Вычисляем целевое значение
        float targetAdrenaline = Mathf.Clamp(currentAdrenaline + amountToHeal, 0f, maxAdrenaline);

        // 2. Если уже на максимуме, выходим
        if (currentAdrenaline >= targetAdrenaline) yield break;

        // 3. Плавно повышаем
        while (currentAdrenaline < targetAdrenaline)
        {
            // === ИСПРАВЛЕНИЕ №1: Убрано "+ 1". Используем unscaledDeltaTime для работы даже на паузе ===
            currentAdrenaline = Mathf.MoveTowards(currentAdrenaline, targetAdrenaline, 50f * Time.unscaledDeltaTime);

            // Если значение достигло цели, прерываем цикл, чтобы Update не успел его уменьшить и создать бесконечный цикл
            if (currentAdrenaline >= targetAdrenaline)
            {
                break;
            }

            yield return null;
        }

        // 4. Жестко фиксируем финальное значение
        currentAdrenaline = targetAdrenaline;
        ApplyVisualEffects();
        ApplyMusicVolumes();
        SyncToGlobal();
    }

    // НОВАЯ КОРУТИНА: Ждет анимацию, затем лечит
    private System.Collections.IEnumerator DelayedHealRoutine()
    {
        // === ИСПРАВЛЕНИЕ №3: Используем Realtime, чтобы ждать даже если игра на паузе ===
        yield return new WaitForSecondsRealtime(injectionDelay);

        // После задержки запускаем плавное восстановление адреналина
        StartCoroutine(SmoothHealRoutine(injectionBoost));
    }

    public void UseSyringe()
    {
        // 1. СРАЗУ запускаем 3D анимацию укола
        if (injectorAnimator != null)
        {
            injectorAnimator.SetTrigger(injectTriggerName);
        }

        // 2. Запускаем восстановление адреналина С ЗАДЕРЖКОЙ
        StartCoroutine(DelayedHealRoutine());

        syringeAmount--;
        UnityEngine.Debug.Log($"+{injectionBoost} adrenaline (начнется через {injectionDelay} сек)");
        nextInjTime = Time.time + cooldown;

        // Сохраняем сразу после использования
        SyncToGlobal();
    }

    public void KillReward()
    {
        if (currentAdrenaline > 0)
        {
            StartCoroutine(SmoothHealRoutine(killReward));
            UnityEngine.Debug.Log($"+{killReward} adrenaline");
        }
    }

    public void GameOver()
    {
        UnityEngine.Debug.Log("<color=red>Игрок погиб. Переход в меню...</color>");

        // Блокируем управление
        PlayerController pc = GetComponent<PlayerController>();
        if (pc != null) pc.LockControls();

        // Красиво уходим в MainMenu через шторку
        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.TransitionToScene("MainMenu");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }

    public void TakeDamage(float damageAmount)
    {
        StartCoroutine(SmoothHealRoutine(-damageAmount));
        if (adrenalineUI != null)
        {
            adrenalineUI.TriggerShake();
        }

        // Сохраняем сразу после урона
        SyncToGlobal();

        if (currentAdrenaline <= 0)
        {
            GameOver();
        }
    }

    public void GetSyringe()
    {
        syringeAmount++;
        syringeAmount = Mathf.Clamp(syringeAmount, 0, 4);
        UnityEngine.Debug.Log("+ syringe");

        // Сохраняем сразу после подбора
        SyncToGlobal();
    }

    public int GetSyringeAmount() => syringeAmount;
}