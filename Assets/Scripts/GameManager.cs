using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private CameraShaderController cameraEffect;

    [Header("UI")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private GameObject timerPanel;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI killCounterText;

    [Header("Настройки локаций")]
    [SerializeField] private float bottomHeight = -72f;
    [SerializeField] private float topHeight = 74f;

    [Header("Тайминги")]
    [SerializeField] private float delayAfterAllDead = 5f;
    [SerializeField] private float lootTimerDuration = 20f;

    [Header("Настройки убийств")]
    [SerializeField] private int requiredKills = 4;

    [Header("Материалы камеры")]
    [SerializeField] private Material bottomLocationMaterial;
    [SerializeField] private Material topLocationMaterial;

    private int killsCount = 0;
    private bool phaseTriggered = false;
    private bool isLootPhase = false;
    private bool hasTransitionedToInventory = false;
    private float lootTimer = 0f;

    private void Start()
    {
        if (notificationPanel != null) notificationPanel.SetActive(false);
        if (timerPanel != null) timerPanel.SetActive(false);

        if (cameraEffect != null && bottomLocationMaterial != null)
            cameraEffect.ApplyMaterial(bottomLocationMaterial);

        UpdateKillCounter();

        Debug.Log($"[GameManager] Нужно убить врагов: {requiredKills}");
    }

    private void Update()
    {
        // Логика таймера лутинга
        if (isLootPhase)
        {
            lootTimer -= Time.deltaTime;

            if (timerText != null)
            {
                int seconds = Mathf.CeilToInt(lootTimer);
                timerText.text = seconds > 0 ? seconds.ToString() : "0";
            }

            if (lootTimer <= 0 && !hasTransitionedToInventory)
            {
                hasTransitionedToInventory = true;
                StartInventoryPhase();
            }
        }
    }

    // Вызывается из Damageable когда враг умирает
    public void OnEnemyDied(Damageable enemy)
    {
        if (phaseTriggered) return;

        killsCount++;
        Debug.Log($"[GameManager] Убийство #{killsCount}/{requiredKills}. Враг: {enemy.gameObject.name}");

        UpdateKillCounter();

        if (killsCount >= requiredKills)
        {
            phaseTriggered = true;
            Debug.Log($"[GameManager] Цель достигнута! Убито: {killsCount}");
            StartCoroutine(StartLootPhase());
        }
    }

    private void UpdateKillCounter()
    {
        if (killCounterText != null)
        {
            killCounterText.text = $"Убийств: {killsCount}/{requiredKills}";
            killCounterText.gameObject.SetActive(true);
        }
    }

    private IEnumerator StartLootPhase()
    {
        yield return new WaitForSeconds(delayAfterAllDead);

        isLootPhase = true;

        TeleportPlayer(topHeight);

        if (cameraEffect != null && topLocationMaterial != null)
            cameraEffect.ApplyMaterial(topLocationMaterial);

        ShowNotification("Нужно обыскать трупы!!!", Color.red, 4f);

        lootTimer = lootTimerDuration;
        if (timerPanel != null) timerPanel.SetActive(true);

        if (killCounterText != null) killCounterText.gameObject.SetActive(false);

        Debug.Log("[GameManager] Фаза лутинга началась");
    }

    private void StartInventoryPhase()
    {
        isLootPhase = false;

        if (timerPanel != null) timerPanel.SetActive(false);

        TeleportPlayer(bottomHeight);

        if (cameraEffect != null && bottomLocationMaterial != null)
            cameraEffect.ApplyMaterial(bottomLocationMaterial);

        ShowNotification("Открыть инвентарь [Tab]", Color.red, 5f);

        Debug.Log("[GameManager] Фаза инвентаря началась");
    }

    private void TeleportPlayer(float targetY)
    {
        if (playerTransform == null)
        {
            Debug.LogError("[GameManager] PlayerTransform не назначен!");
            return;
        }

        CharacterController cc = playerTransform.GetComponent<CharacterController>();
        Vector3 newPos = playerTransform.position;
        newPos.y += targetY;

        if (cc != null)
        {
            cc.enabled = false;
            playerTransform.position = newPos;
            cc.enabled = true;
        }
        else
        {
            playerTransform.position = newPos;
        }

        Debug.Log($"[GameManager] Игрок телепортирован на высоту {targetY}");
    }

    private void ShowNotification(string message, Color color, float duration)
    {
        if (notificationPanel == null || notificationText == null) return;

        notificationText.text = message;
        notificationText.color = color;
        notificationPanel.SetActive(true);

        StartCoroutine(HideNotificationAfterDelay(duration));
    }

    private IEnumerator HideNotificationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (notificationPanel != null)
            notificationPanel.SetActive(false);
    }
}