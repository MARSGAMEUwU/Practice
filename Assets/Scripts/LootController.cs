using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

public class LootController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction lootAction; // E

    [Header("UI")]
    [SerializeField] private GameObject lootTextUI;
    [SerializeField] private Image progressBar;
    [SerializeField] private Image progressBarBG;

    [Header("Настройки")]
    [SerializeField] private float lootTime = 3f;

    private List<GameObject> corpsesInRange = new List<GameObject>();
    private float lootProgress = 0f;

    private void Awake()
    {
        if (lootAction != null) lootAction.Enable();
        FindUI();
        HideUI();
    }

    private void FindUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        if (lootTextUI == null)
        {
            Transform t = FindChildRecursive(canvas.transform, "LootText");
            if (t != null) lootTextUI = t.gameObject;
        }

        if (progressBar == null)
        {
            Transform t = FindChildRecursive(canvas.transform, "LootProgressBar");
            if (t != null)
            {
                progressBar = t.GetComponent<Image>();
                progressBar.type = Image.Type.Filled;
                progressBar.fillMethod = Image.FillMethod.Horizontal;
                progressBar.fillOrigin = (int)Image.OriginHorizontal.Left;
            }
        }

        if (progressBarBG == null)
        {
            Transform t = FindChildRecursive(canvas.transform, "LootProgressBarBG");
            if (t != null) progressBarBG = t.GetComponent<Image>();
        }
    }

    private Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform found = FindChildRecursive(child, name);
            if (found != null) return found;
        }
        return null;
    }

    private void Update()
    {
        bool isHoldingE = lootAction != null && lootAction.IsPressed();

        if (corpsesInRange.Count > 0)
        {
            if (isHoldingE)
            {
                lootProgress += Time.deltaTime;

                if (progressBar != null)
                {
                    progressBar.gameObject.SetActive(true);
                    progressBar.fillAmount = Mathf.Clamp01(lootProgress / lootTime);
                }

                if (lootProgress >= lootTime)
                {
                    CompleteLooting();
                }
            }
            else
            {
                lootProgress = 0f;
                if (progressBar != null) progressBar.fillAmount = 0f;
            }
        }
        else
        {
            lootProgress = 0f;
            HideUI();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Corpse"))
        {
            if (lootTextUI != null) lootTextUI.SetActive(true);
            if (progressBarBG != null) progressBarBG.gameObject.SetActive(true);
            if (!corpsesInRange.Contains(other.gameObject))
            {
                corpsesInRange.Add(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Corpse"))
        {
            if (lootTextUI != null) lootTextUI.SetActive(false);
            if (progressBarBG != null) progressBarBG.gameObject.SetActive(false);
            corpsesInRange.Remove(other.gameObject);
        }
    }

    private void CompleteLooting()
    {
        int corpseCount = corpsesInRange.Count;

        // Выдаём ресурсы за каждый труп
        InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();
        if (inventoryManager != null)
        {
            for (int i = 0; i < corpseCount; i++)
            {
                inventoryManager.AddResource(0, 1);
                inventoryManager.AddResource(1, 1);
                inventoryManager.AddResource(2, 1);
                inventoryManager.AddResource(3, 1);
            }
            Debug.Log($"<color=green>✅ Обобрано трупов: {corpseCount}. Ресурсы добавлены.</color>");
        }

        // Уничтожаем все трупы
        foreach (var corpse in corpsesInRange)
        {
            if (corpse != null) Destroy(corpse);
        }

        corpsesInRange.Clear();
        lootProgress = 0f;
        HideUI();
    }

    private void HideUI()
    {
        if (lootTextUI != null) lootTextUI.SetActive(false);
        if (progressBar != null)
        {
            progressBar.fillAmount = 0f;
            progressBar.gameObject.SetActive(false);
        }
        if (progressBarBG != null) progressBarBG.gameObject.SetActive(false);
    }
}