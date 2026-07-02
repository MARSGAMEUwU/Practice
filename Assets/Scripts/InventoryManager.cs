using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private PlayerWeaponInventory playerInventory;

    [Header("Ресурсы")]
    [Tooltip("Порядок: [0]Ствол, [1]Магазин, [2]Рукоять, [3]Прицел")]
    private int[] materialsAmount = new int[4];

    [Header("Иконки ресурсов для UI")]
    public Sprite barrelIcon;
    public Sprite magazineIcon;
    public Sprite handleIcon;
    public Sprite scopeIcon;

    public Sprite[] resourceIcons;

    private void Awake()
    {
        resourceIcons = new Sprite[] { barrelIcon, magazineIcon, handleIcon, scopeIcon };

        if (playerInventory == null)
            playerInventory = FindObjectOfType<PlayerWeaponInventory>();
    }

    // === Ресурсы ===

    public void AddResource(int resourceIndex, int amount)
    {
        if (resourceIndex < 0 || resourceIndex >= materialsAmount.Length) return;
        materialsAmount[resourceIndex] += amount;
        Debug.Log($"[Инвентарь] +{amount} ресурса #{resourceIndex}. Всего: {materialsAmount[resourceIndex]}");

        // Уведомляем UI через FindObjectOfType
        InventoryUI ui = FindObjectOfType<InventoryUI>();
        if (ui != null) ui.RefreshResources();
    }

    public int GetResource(int index)
    {
        if (index < 0 || index >= materialsAmount.Length) return 0;
        return materialsAmount[index];
    }

    public int[] GetAllResources() => materialsAmount;

    public bool CanAfford(int[] recipe)
    {
        if (recipe == null || recipe.Length != 4) return false;
        for (int i = 0; i < 4; i++)
        {
            if (materialsAmount[i] < recipe[i]) return false;
        }
        return true;
    }

    public void SpendResources(int[] recipe)
    {
        if (recipe == null) return;
        for (int i = 0; i < 4; i++)
        {
            materialsAmount[i] -= recipe[i];
        }
        Debug.Log($"[Инвентарь] Списаны ресурсы: {recipe[0]}|{recipe[1]}|{recipe[2]}|{recipe[3]}");
    }

    // === Крафт (покупка) ===

    public void CraftPurchase(WeaponData weapon)
    {
        if (weapon == null) return;
        if (!CanAfford(weapon.purchaseRecipe))
        {
            Debug.Log($"[Крафт] Не хватает ресурсов для покупки {weapon.weaponName}");
            return;
        }

        if (HasWeaponOfType(weapon.weaponType))
        {
            Debug.Log($"[Крафт] Оружие типа {weapon.weaponType} уже есть в инвентаре!");
            return;
        }

        SpendResources(weapon.purchaseRecipe);

        if (!playerInventory.AddWeapon(weapon, WeaponRarity.Common))
        {
            playerInventory.SpawnWeaponPickup(weapon, WeaponRarity.Common);
            Debug.Log($"[Крафт] Нет места! {weapon.weaponName} упал на землю");
        }
        else
        {
            Debug.Log($"[Крафт] Скрафчен {weapon.weaponName} (Common)");
        }

        RefreshUI();
    }

    // === Апгрейд ===

    public void CraftUpgrade(WeaponData weapon)
    {
        if (weapon == null) return;

        int slotIndex = FindWeaponSlot(weapon.weaponType);
        if (slotIndex == -1)
        {
            Debug.Log($"[Апгрейд] Оружие типа {weapon.weaponType} не найдено!");
            return;
        }

        WeaponRarity currentRarity = playerInventory.GetRarityInSlot(slotIndex);
        if (currentRarity >= WeaponRarity.Legendary)
        {
            Debug.Log($"[Апгрейд] {weapon.weaponName} уже максимального уровня!");
            return;
        }

        int[] recipe = weapon.GetUpgradeRecipe(currentRarity);
        if (recipe == null)
        {
            Debug.Log($"[Апгрейд] Рецепт для {currentRarity}→{weapon.GetNextRarity(currentRarity)} не задан!");
            return;
        }

        if (!CanAfford(recipe))
        {
            Debug.Log($"[Апгрейд] Не хватает ресурсов!");
            return;
        }

        SpendResources(recipe);
        WeaponRarity nextRarity = weapon.GetNextRarity(currentRarity);
        playerInventory.SetWeaponRarity(slotIndex, nextRarity);

        Debug.Log($"[Апгрейд] {weapon.weaponName} улучшен до {nextRarity}");
        RefreshUI();
    }

    // === Вспомогательные ===

    private bool HasWeaponOfType(WeaponType type)
    {
        for (int i = 0; i < 2; i++)
        {
            WeaponData w = playerInventory.GetWeaponInSlot(i);
            if (w != null && w.weaponType == type) return true;
        }
        return false;
    }

    private int FindWeaponSlot(WeaponType type)
    {
        for (int i = 0; i < 2; i++)
        {
            WeaponData w = playerInventory.GetWeaponInSlot(i);
            if (w != null && w.weaponType == type) return i;
        }
        return -1;
    }

    public WeaponRarity GetCurrentRarity(WeaponType type)
    {
        int slot = FindWeaponSlot(type);
        if (slot == -1) return WeaponRarity.Common;
        return playerInventory.GetRarityInSlot(slot);
    }

    public bool HasWeapon(WeaponType type) => FindWeaponSlot(type) != -1;

    private void RefreshUI()
    {
        InventoryUI ui = FindObjectOfType<InventoryUI>();
        if (ui != null)
        {
            ui.RefreshResources();
            ui.RefreshSlots();
        }
    }
}