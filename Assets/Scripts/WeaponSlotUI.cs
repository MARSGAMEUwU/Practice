using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponSlotUI : MonoBehaviour
{
    [Header("Элементы плашки")]
    [SerializeField] private Image background;        // Серый фон плашки
    [SerializeField] private Image rarityOverlay;     // Полупрозрачный цветной слой
    [SerializeField] private Image weaponIcon;        // Иконка оружия
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Кнопки")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button purchaseButton;

    [Header("Рецепты")]
    [SerializeField] private RecipeDisplay upgradeRecipeDisplay;
    [SerializeField] private RecipeDisplay purchaseRecipeDisplay;

    [Header("Цвета")]
    [SerializeField] private Color emptySlotColor = new Color(0.25f, 0.25f, 0.25f);
    [SerializeField] private float overlayAlpha = 0.5f;

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
        // Определяем цвет плашки
        Color targetColor;

        if (!hasWeapon)
        {
            // Нет оружия — серый
            targetColor = emptySlotColor;
            if (rarityOverlay != null)
                rarityOverlay.color = new Color(0.3f, 0.3f, 0.3f, overlayAlpha);
        }
        else
        {
            // Есть оружие — цвет СЛЕДУЮЩЕЙ редкости
            WeaponRarity nextRarity = currentWeaponData != null
                ? currentWeaponData.GetNextRarity(currentRarity)
                : currentRarity;

            if (currentWeaponData != null)
                targetColor = currentWeaponData.GetRarityColor(nextRarity);
            else
                targetColor = Color.gray;

            // Полупрозрачный цветной слой поверх серого фона
            if (rarityOverlay != null)
                rarityOverlay.color = new Color(targetColor.r, targetColor.g, targetColor.b, overlayAlpha);
        }

        // Фон остаётся серым/белым базовым
        if (background != null)
            background.color = new Color(0.4f, 0.4f, 0.4f, 1f);

        // Иконка оружия
        if (weaponIcon != null)
        {
            if (hasWeapon && currentWeaponData != null && currentWeaponData.weaponIcon != null)
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
            if (hasWeapon && currentWeaponData != null)
            {
                weaponNameText.text = currentWeaponData.weaponName;
                weaponNameText.gameObject.SetActive(true);
            }
            else
            {
                weaponNameText.gameObject.SetActive(false);
            }
        }

        // Описание улучшения
        if (descriptionText != null)
        {
            if (hasWeapon && !isMaxRarity && currentWeaponData != null)
            {
                descriptionText.text = currentWeaponData.GetUpgradeDescription(currentRarity);
                descriptionText.gameObject.SetActive(true);
            }
            else
            {
                descriptionText.gameObject.SetActive(false);
            }
        }

        // Рецепты
        if (purchaseRecipeDisplay != null && currentWeaponData != null)
        {
            purchaseRecipeDisplay.SetRecipe(currentWeaponData.purchaseRecipe, inventoryManager);
        }

        if (upgradeRecipeDisplay != null && currentWeaponData != null && hasWeapon && !isMaxRarity)
        {
            int[] recipe = currentWeaponData.GetUpgradeRecipe(currentRarity);
            upgradeRecipeDisplay.SetRecipe(recipe, inventoryManager);
            upgradeRecipeDisplay.gameObject.SetActive(true);
        }
        else if (upgradeRecipeDisplay != null)
        {
            upgradeRecipeDisplay.gameObject.SetActive(false);
        }
    }

    private void UpdateButtons()
    {
        // Кнопка покупки
        if (purchaseButton != null)
        {
            bool canPurchase = hasWeapon == false
                && currentWeaponData != null
                && inventoryManager != null
                && inventoryManager.CanAfford(currentWeaponData.purchaseRecipe);

            purchaseButton.interactable = canPurchase;
            purchaseButton.gameObject.SetActive(!hasWeapon);
        }

        // Кнопка апгрейда
        if (upgradeButton != null)
        {
            bool canUpgrade = hasWeapon
                && !isMaxRarity
                && currentWeaponData != null
                && inventoryManager != null;

            if (canUpgrade)
            {
                int[] recipe = currentWeaponData.GetUpgradeRecipe(currentRarity);
                canUpgrade = inventoryManager.CanAfford(recipe);
            }

            upgradeButton.interactable = canUpgrade;
            upgradeButton.gameObject.SetActive(hasWeapon && !isMaxRarity);
        }
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

    // Вызывается из InventoryManager после крафта для обновления
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