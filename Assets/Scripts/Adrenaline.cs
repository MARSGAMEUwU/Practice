using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Adrenaline : MonoBehaviour
{
    [Header("Основные настройки")]
    [SerializeField] private float maxAdrenaline = 100f;
    [SerializeField] private float currentAdrenaline = 0f;
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
    [SerializeField] private AudioSource track1;
    [SerializeField] private AudioSource track2;
    [SerializeField] private AudioSource track3;
    [SerializeField] private AudioSource track4;
    [SerializeField] private float volume = 0.3f;

    [Header("UI")]
    [SerializeField] private AdrenalineUI adrenalineUI;

    public float AdrenalinePercentage => currentAdrenaline / maxAdrenaline;

    private float nextInjTime;
    private float currentSaturation;
    private float currentContrast;
    private float currentFov;

    private void Awake()
    {
        currentSaturation = 0.5f;
        currentContrast = 1f;
        shader.SetFloat("_Saturation", currentSaturation);
        shader.SetFloat("_Contrast", currentContrast);
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
        if (Time.timeScale <= 0f) return;

        if (currentAdrenaline > 0)
        {
            currentAdrenaline -= decayRate * Time.deltaTime;
            currentAdrenaline = Mathf.Clamp(currentAdrenaline, 1f, maxAdrenaline);

            currentSaturation = Mathf.Lerp(minSaturation, maxSaturation, AdrenalinePercentage);
            currentContrast = Mathf.Lerp(minContrast, maxContrast, AdrenalinePercentage);
            shader.SetFloat("_Saturation", currentSaturation);
            shader.SetFloat("_Contrast", currentContrast);

            currentFov = Mathf.Lerp(minFov, maxFov, AdrenalinePercentage);
            mainCamera.fieldOfView = currentFov;
        }

        if (useSyringe.IsPressed() && syringeAmount > 0 && Time.time >= nextInjTime)
        {
            UseSyringe();
        }

        if (track1 != null) track1.volume = volume;
        if (track2 != null) track2.volume = Mathf.InverseLerp(10f, 30f, currentAdrenaline) * volume;
        if (track3 != null) track3.volume = Mathf.InverseLerp(30f, 60f, currentAdrenaline) * volume;
        if (track4 != null) track4.volume = Mathf.InverseLerp(60f, 90f, currentAdrenaline) * volume;
    }

    // Метод для плавного восстановления адреналина
    private System.Collections.IEnumerator SmoothHealRoutine(float amountToHeal)
    {
        // 1. Вычисляем целевое значение адреналина
        float targetAdrenaline = Mathf.Clamp(currentAdrenaline + amountToHeal, 0f, maxAdrenaline);

        // 2. Пока текущее меньше целевого, плавно повышаем...
        while (currentAdrenaline < targetAdrenaline)
        {
            // Mathf.MoveTowards плавно движется к цели
            currentAdrenaline = Mathf.MoveTowards(currentAdrenaline, targetAdrenaline + 1, 50f * Time.deltaTime);

            // 3. Ждем следующий кадр
            yield return null;
        }

        // На всякий случай жестко фиксируем финальное значение
        currentAdrenaline = targetAdrenaline;
    }

    // НОВАЯ КОРУТИНА: Ждет анимацию, затем лечит
    private System.Collections.IEnumerator DelayedHealRoutine()
    {
        // Ждем указанное время (пока персонаж делает анимацию укола)
        yield return new WaitForSeconds(injectionDelay);

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
        //Time.timeScale = 0f;
        UnityEngine.Debug.Log("Игрок погиб");
        if (WaveManager.Instance != null) WaveManager.Instance.GameOver();
        SceneManager.LoadScene("GameOver");
    }

    public void TakeDamage(float damageAmount)
    {
        StartCoroutine(SmoothHealRoutine(-damageAmount));
        if (adrenalineUI != null)
        {
            adrenalineUI.TriggerShake();
        }
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
    }
}