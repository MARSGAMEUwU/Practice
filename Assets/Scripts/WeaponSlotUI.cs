using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponSlotUI : MonoBehaviour
{
    [Header("Визуальные элементы слота")]
    [SerializeField] private Image background;        // Фон слота
    [SerializeField] private Image rarityOverlay;     // Цветная подложка (редкость)
    [SerializeField] private Image weaponIcon;        // Иконка оружия
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Кнопки")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button purchaseButton;

    [Header("Рецепты")]
    [SerializeField] private RecipeDisplay upgradeRecipeDisplay;
    [SerializeField] private RecipeDisplay purchaseRecipeDisplay;

    [Header("Настройки")]
    [SerializeField] private Color emptySlotColor = new Color(0.2f, 0.2f, 0.2f); // Чуть темнее для "пустого" состояния
    [SerializeField] private float overlayAlpha = 0.4f;

    private InventoryManager inventoryManager;
    private WeaponData currentWeaponData;
    private bool hasWeapon = false;
    private WeaponRarity currentRarity = WeaponRarity.Common;
    private bool isMaxRarity = false;

    public void Setup(
        WeaponData weaponData,
        bool hasWeapon,
        WeaponRarity currentRarity,
        bool isMaxRarity,
        InventoryManager manager)
    {
        currentWeaponData = weaponData;
        this.hasWeapon = hasWeapon;
        this.currentRarity = currentRarity;
        this.isMaxRarity = isMaxRarity;
        inventoryManager = manager;

        UpdateVisuals();
        UpdateButtons();
    }

    private void UpdateVisuals()
    {
        // Если WeaponData вообще не назначен (ошибка конфигурации), скрываем всё
        if (currentWeaponData == null)
        {
            SetSlotEmpty();
            return;
        }

        // 1. ФОН СЛОТА
        if (background != null)
        {
            background.color = hasWeapon ? new Color(0.35f, 0.35f, 0.35f, 1f) : emptySlotColor;
        }

        // 2. ОВЕРЛЕЙ РЕДКОСТИ
        if (rarityOverlay != null)
        {
            // Если оружия нет, показываем цвет Common. Если есть - текущий цвет.
            WeaponRarity displayRarity = hasWeapon ? currentRarity : WeaponRarity.Common;
            Color rarityColor = currentWeaponData.GetRarityColor(displayRarity);
            rarityOverlay.color = new Color(rarityColor.r, rarityColor.g, rarityColor.b, overlayAlpha);
        }

        // 3. ИКОНКА, НАЗВАНИЕ, ОПИСАНИЕ (Всегда активны, если есть WeaponData)
        // Иконка
        if (weaponIcon != null)
        {
            if (currentWeaponData.weaponIcon != null)
            {
                weaponIcon.sprite = currentWeaponData.weaponIcon;
                weaponIcon.gameObject.SetActive(true);
            }
            else
            {
                weaponIcon.gameObject.SetActive(false);
            }
        }

        // Название
        if (weaponNameText != null)
        {
            weaponNameText.text = currentWeaponData.weaponName;
            weaponNameText.gameObject.SetActive(true);
            weaponNameText.color = Color.white; // Белый текст для читаемости на темном фоне
        }

        // 4. ОПИСАНИЕ (Учитываем новую логику 4-х элементов)
        if (descriptionText != null)
        {
            string desc = "";

            if (!hasWeapon)
            {
                if (currentWeaponData.descriptionsByRarity != null && currentWeaponData.descriptionsByRarity.Length > 0)
                {
                    desc = currentWeaponData.descriptionsByRarity[0];
                }
            }
            else
            {
                int nextDescIndex = (int)currentRarity + 1;

                if (currentWeaponData.descriptionsByRarity != null && nextDescIndex < currentWeaponData.descriptionsByRarity.Length)
                {
                    desc = currentWeaponData.descriptionsByRarity[nextDescIndex];
                }
            }

            if (!string.IsNullOrEmpty(desc))
            {
                descriptionText.text = desc;
                descriptionText.gameObject.SetActive(true);
                descriptionText.color = Color.white;
            }
            else
            {
                descriptionText.gameObject.SetActive(false);
            }
        }

        // 4. РЕЦЕПТЫ (Логика взаимного исключения)
        // Рецепт покупки: ТОЛЬКО если оружия НЕТ
        if (purchaseRecipeDisplay != null)
        {
            if (!hasWeapon)
            {
                purchaseRecipeDisplay.SetRecipe(currentWeaponData.purchaseRecipe, inventoryManager);
                purchaseRecipeDisplay.gameObject.SetActive(true);
            }
            else
            {
                purchaseRecipeDisplay.gameObject.SetActive(false); // СКРЫВАЕМ, если оружие уже есть
            }
        }

        // Рецепт апгрейда: ТОЛЬКО если оружие ЕСТЬ и оно не максимального уровня
        if (upgradeRecipeDisplay != null)
        {
            if (hasWeapon && !isMaxRarity)
            {
                int[] recipe = currentWeaponData.GetUpgradeRecipe(currentRarity);
                upgradeRecipeDisplay.SetRecipe(recipe, inventoryManager);
                upgradeRecipeDisplay.gameObject.SetActive(true);
            }
            else
            {
                upgradeRecipeDisplay.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateButtons()
    {
        // Кнопка покупки: ТОЛЬКО если оружия НЕТ
        if (purchaseButton != null)
        {
            bool showPurchase = !hasWeapon;
            purchaseButton.gameObject.SetActive(showPurchase);

            if (showPurchase)
            {
                bool canAfford = inventoryManager != null && inventoryManager.CanAfford(currentWeaponData.purchaseRecipe);
                purchaseButton.interactable = canAfford;
            }
        }

        // Кнопка апгрейда: ТОЛЬКО если оружие ЕСТЬ и не максимального уровня
        if (upgradeButton != null)
        {
            bool showUpgrade = hasWeapon && !isMaxRarity;
            upgradeButton.gameObject.SetActive(showUpgrade);

            if (showUpgrade)
            {
                int[] recipe = currentWeaponData.GetUpgradeRecipe(currentRarity);
                bool canAfford = inventoryManager != null && inventoryManager.CanAfford(recipe);
                upgradeButton.interactable = canAfford;
            }
        }
    }

    // Вспомогательный метод для полного скрытия (если WeaponData битый)
    private void SetSlotEmpty()
    {
        if (background != null) background.color = emptySlotColor;
        if (rarityOverlay != null) rarityOverlay.color = new Color(0, 0, 0, 0);
        if (weaponIcon != null) weaponIcon.gameObject.SetActive(false);
        if (weaponNameText != null) weaponNameText.gameObject.SetActive(false);
        if (descriptionText != null) descriptionText.gameObject.SetActive(false);
        if (purchaseRecipeDisplay != null) purchaseRecipeDisplay.gameObject.SetActive(false);
        if (upgradeRecipeDisplay != null) upgradeRecipeDisplay.gameObject.SetActive(false);
        if (purchaseButton != null) purchaseButton.gameObject.SetActive(false);
        if (upgradeButton != null) upgradeButton.gameObject.SetActive(false);
    }

    // === Обработчики кнопок ===
    public void OnPurchaseClicked()
    {
        if (currentWeaponData != null && inventoryManager != null)
        {
            inventoryManager.CraftPurchase(currentWeaponData);
        }
    }

    public void OnUpgradeClicked()
    {
        if (currentWeaponData != null && inventoryManager != null)
        {
            inventoryManager.CraftUpgrade(currentWeaponData);
        }
    }

    // Обновление слота извне (например, при изменении ресурсов)
    public void Refresh()
    {
        if (inventoryManager != null && currentWeaponData != null)
        {
            Setup(
                currentWeaponData,
                inventoryManager.HasWeapon(currentWeaponData.weaponType),
                inventoryManager.GetCurrentRarity(currentWeaponData.weaponType),
                false,
                inventoryManager
            );
        }
    }
}