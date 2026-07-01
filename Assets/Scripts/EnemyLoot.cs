using UnityEngine;
using UnityEngine.UI;

public class EnemyLoot : MonoBehaviour
{
    [Header("Настройки лутинга")]
    [SerializeField] private float lootTime = 3f;
    [SerializeField] private KeyCode lootKey = KeyCode.E;

    [Header("UI элементы")]
    [SerializeField] private GameObject lootTextUI;
    [SerializeField] private Image progressBarFill;
    [SerializeField] private Image progressBarBackground;

    [Header("Верхний труп")]
    [SerializeField] private GameObject upperCorpse;

    private bool isPlayerNear = false;
    private float lootProgress = 0f;
    private bool isLooting = false;
    private bool isLooted = false;

    private void Start()
    {
        if (lootTextUI != null) lootTextUI.SetActive(false);
        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = 0f;
            progressBarFill.gameObject.SetActive(false);
        }
        if (progressBarBackground != null) progressBarBackground.gameObject.SetActive(false);

        // Если upperCorpse не назначен, используем этот объект
        if (upperCorpse == null)
        {
            upperCorpse = gameObject;
        }
    }

    private void Update()
    {
        if (isLooted) return;

        if (isPlayerNear)
        {
            if (Input.GetKey(lootKey))
            {
                StartLooting();
                UpdateLootProgress();
            }
            else
            {
                StopLooting();
            }
        }
        else
        {
            StopLooting();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            if (lootTextUI != null) lootTextUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (lootTextUI != null) lootTextUI.SetActive(false);
            StopLooting();
        }
    }

    private void StartLooting()
    {
        if (isLooting) return;

        isLooting = true;
        if (progressBarFill != null) progressBarFill.gameObject.SetActive(true);
        if (progressBarBackground != null) progressBarBackground.gameObject.SetActive(true);
        if (lootTextUI != null) lootTextUI.SetActive(false);
    }

    private void StopLooting()
    {
        if (!isLooting) return;

        isLooting = false;
        lootProgress = 0f;

        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = 0f;
            progressBarFill.gameObject.SetActive(false);
        }
        if (progressBarBackground != null) progressBarBackground.gameObject.SetActive(false);
        if (lootTextUI != null && isPlayerNear) lootTextUI.SetActive(true);
    }

    private void UpdateLootProgress()
    {
        if (!isLooting) return;

        lootProgress += Time.deltaTime;
        float progress = Mathf.Clamp01(lootProgress / lootTime);

        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = progress;
        }

        if (lootProgress >= lootTime)
        {
            CompleteLooting();
        }
    }

    private void CompleteLooting()
    {
        isLooted = true;
        isLooting = false;

        // === ЗАГЛУШКА: функция лутинга ===
        Debug.Log($"<color=green>Лутинг завершён! Получено: золото, зелья, оружие</color>");

        // Скрываем UI
        if (progressBarFill != null) progressBarFill.gameObject.SetActive(false);
        if (progressBarBackground != null) progressBarBackground.gameObject.SetActive(false);
        if (lootTextUI != null) lootTextUI.SetActive(false);

        // Удаляем верхний труп
        if (upperCorpse != null)
        {
            Destroy(upperCorpse);
            Debug.Log("Верхний труп удалён");
        }
    }
}