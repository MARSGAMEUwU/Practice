using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class EnemyLoot : MonoBehaviour
{
    [Header("Настройки лутания")]
    [SerializeField] private float lootTime = 3f;
    [SerializeField] private InputAction lootAction;

    [Header("UI элементы (опционально)")]
    [SerializeField] private GameObject lootTextUI;
    [SerializeField] private Image progressBar;
    [SerializeField] private Image progressBarBG;

    [Header("Трупы")]
    [SerializeField] private GameObject upperCorpse;

    private bool isPlayerNear = false;
    private float lootProgress = 0f;
    private bool isLooting = false;
    private bool isLooted = false;

    private void Start()
    {
        // === АВТОМАТИЧЕСКИЙ ПОИСК UI ===
        FindUIElements();

        // Скрываем UI при старте
        HideUI();

        // Настраиваем прогресс-бар
        if (progressBar != null)
        {
            progressBar.fillAmount = 0f;
            progressBar.type = Image.Type.Filled;
            progressBar.fillMethod = Image.FillMethod.Horizontal;
            progressBar.fillOrigin = (int)Image.OriginHorizontal.Left;
        }

        if (lootAction != null) lootAction.Enable();
    }

    // === НОВЫЙ МЕТОД: поиск UI в сцене ===
    private void FindUIElements()
    {
        // Ищем Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas не найден!");
            return;
        }

        // Ищем LootText
        if (lootTextUI == null)
        {
            Transform t = canvas.transform.Find("LootText");
            if (t != null)
            {
                lootTextUI = t.gameObject;
                Debug.Log("LootText найден автоматически");
            }
        }

        // Ищем ProgressBarBG
        if (progressBarBG == null)
        {
            Transform t = canvas.transform.Find("LootProgressBarBG");
            if (t != null)
            {
                progressBarBG = t.GetComponent<Image>();
                Debug.Log("LootProgressBarBG найден автоматически");
            }
        }

        // Ищем ProgressBar
        if (progressBar == null)
        {
            Transform t = canvas.transform.Find("LootProgressBar");
            if (t != null)
            {
                progressBar = t.GetComponent<Image>();
                Debug.Log("LootProgressBar найден автоматически");
            }
        }
    }

    private void Update()
    {
        if (isLooted) return;

        bool isHoldingE = lootAction != null && lootAction.IsPressed();

        if (isPlayerNear)
        {
            if (isHoldingE)
            {
                isLooting = true;
                lootProgress += Time.deltaTime;

                if (progressBar != null)
                {
                    progressBar.fillAmount = lootProgress / lootTime;
                    progressBar.gameObject.SetActive(true);
                }

                if (lootProgress >= lootTime)
                {
                    CompleteLooting();
                }
            }
            else
            {
                isLooting = false;
                lootProgress = 0f;
                if (progressBar != null) progressBar.fillAmount = 0f;
            }
        }
        else
        {
            isLooting = false;
            lootProgress = 0f;
            if (progressBar != null) progressBar.fillAmount = 0f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerNear = true;
        if (lootTextUI != null) lootTextUI.SetActive(true);
        if (progressBarBG != null) progressBarBG.gameObject.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerNear = false;
        isLooting = false;
        lootProgress = 0f;
        HideUI();
    }

    private void CompleteLooting()
    {
        isLooted = true;
        isLooting = false;

        Debug.Log($"<color=green>✅ ЛУТ ЗАВЕРШЁН!</color>");

        HideUI();

        if (upperCorpse != null)
        {
            upperCorpse.SetActive(false);
        }
        else
        {
            Renderer renderer = GetComponentInChildren<Renderer>();
            if (renderer != null) renderer.gameObject.SetActive(false);
        }
    }

    private void HideUI()
    {
        if (lootTextUI != null) lootTextUI.SetActive(false);
        if (progressBar != null) progressBar.gameObject.SetActive(false);
        if (progressBarBG != null) progressBarBG.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (lootAction != null) lootAction.Disable();
    }
}