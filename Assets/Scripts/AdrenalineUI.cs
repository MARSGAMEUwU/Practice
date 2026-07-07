using UnityEngine;
using UnityEngine.UI;

public class AdrenalineUI : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private Image fillImage; // Спрайт заполнения
    [SerializeField] private Adrenaline adrenalineSystem; // Ссылка на систему адреналина игрока

    [Header("Настройки тряски")]
    [SerializeField] private float shakeIntensity = 5f; // Сила тряски (пиксели)
    [SerializeField] private float shakeSpeed = 25f; // Скорость тряски
    [SerializeField] private float criticalThreshold = 1f; // Порог критического состояния (1%)

    private RectTransform rectTransform;
    private Vector3 originalPosition;
    private float shakeTimer = 0f;
    private float shakeDuration = 0f;
    private bool isShaking = false;
    private bool isCriticalState = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition3D;
    }

    private void Update()
    {
        if (adrenalineSystem == null || fillImage == null) return;

        // Обновляем заполнение полоски
        float percentage = adrenalineSystem.AdrenalinePercentage;
        fillImage.fillAmount = percentage;

        // Проверяем критическое состояние (< 1%)
        bool shouldBeCritical = percentage <= criticalThreshold / 100f && percentage > 0f;

        if (shouldBeCritical && !isCriticalState)
        {
            // Входим в критическое состояние - начинаем постоянную тряску
            isCriticalState = true;
            StartShake(float.MaxValue); // Бесконечная тряска
        }
        else if (!shouldBeCritical && isCriticalState)
        {
            // Выходим из критического состояния - останавливаем постоянную тряску
            isCriticalState = false;
            StopShake();
        }

        // Обрабатываем тряску
        if (isShaking)
        {
            ProcessShake();
        }
    }

    /// <summary>
    /// Вызывается из Adrenaline при получении урона
    /// </summary>
    public void TriggerShake()
    {
        if (!isCriticalState) // Не прерываем критическую тряску
        {
            StartShake(0.5f); // Тряска 0.5 секунды
        }
    }

    private void StartShake(float duration)
    {
        isShaking = true;
        shakeDuration = duration;
        shakeTimer = 0f;
    }

    private void StopShake()
    {
        isShaking = false;
        rectTransform.anchoredPosition3D = originalPosition;
    }

    private void ProcessShake()
    {
        shakeTimer += Time.deltaTime;

        if (shakeTimer >= shakeDuration && shakeDuration != float.MaxValue)
        {
            StopShake();
            return;
        }

        // Генерируем случайное смещение для тряски
        float offsetX = Random.Range(-shakeIntensity, shakeIntensity);
        float offsetY = Random.Range(-shakeIntensity, shakeIntensity);

        // Применяем смещение к позиции
        Vector3 shakePosition = originalPosition + new Vector3(offsetX, offsetY, 0f);
        rectTransform.anchoredPosition3D = shakePosition;
    }
}