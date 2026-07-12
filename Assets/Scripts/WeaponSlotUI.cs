using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponSlotUI : MonoBehaviour
{
    [Header("Визуальные элементы слота")]
    [SerializeField] private Image background;
    [SerializeField] private Image rarityOverlay;
    [SerializeField] private Image weaponIcon;
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Кнопки")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button purchaseButton;

    [Header("Рецепты")]
    [SerializeField] private RecipeDisplay upgradeRecipeDisplay;
    [SerializeField] private RecipeDisplay purchaseRecipeDisplay;

    [Header("Настройки")]
    [SerializeField] private Color emptySlotColor = new Color(0.2f, 0.2f, 0.2f);
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
        if (currentWeaponData == null)
        {
            SetSlotEmpty();
            return;
        }

        if (background != null)
        {
            background.color = hasWeapon ? new Color(0.35f, 0.35f, 0.35f, 0f) : emptySlotColor;
        }

        if (rarityOverlay != null)
        {
            WeaponRarity displayRarity = hasWeapon ? currentRarity : WeaponRarity.Common;
            Color rarityColor = currentWeaponData.GetRarityColor(displayRarity);
            rarityOverlay.color = new Color(rarityColor.r, rarityColor.g, rarityColor.b, overlayAlpha);
        }

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

        if (weaponNameText != null)
        {
            weaponNameText.text = currentWeaponData.weaponName;
            weaponNameText.gameObject.SetActive(true);
            weaponNameText.color = Color.white;
        }

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

        if (purchaseRecipeDisplay != null)
        {
            if (!hasWeapon)
            {
                purchaseRecipeDisplay.SetRecipe(currentWeaponData.purchaseRecipe, inventoryManager);
                purchaseRecipeDisplay.gameObject.SetActive(true);
            }
            else
            {
                purchaseRecipeDisplay.gameObject.SetActive(false);
            }
        }

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