using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Глобальное хранилище оружия (Сохраняется между сценами)")]
    [SerializeField] private WeaponData[] savedWeapons = new WeaponData[2];
    [SerializeField] private WeaponRarity[] savedRarities = new WeaponRarity[2];

    [Header("Ресурсы")]
    private int[] materialsAmount = new int[4];

    [Header("Иконки ресурсов для UI")]
    public Sprite barrelIcon;
    public Sprite magazineIcon;
    public Sprite handleIcon;
    public Sprite scopeIcon;
    public Sprite[] resourceIcons;

    [Header("Сохранение состояния игрока (Адреналин и шприцы)")]
    [Tooltip("Текущий уровень адреналина при переходе между сценами")]
    public float savedAdrenaline = 0f;

    [Tooltip("Количество шприцов при переходе между сценами")]
    public int savedSyringes = 4;

    private void Awake()
    {
        // === НАСТРОЙКА СИНГЛТОНА И DDOL ===
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Живет вечно

        resourceIcons = new Sprite[] { barrelIcon, magazineIcon, handleIcon, scopeIcon };
    }

    // === МЕТОДЫ ДЛЯ WEAPONCONTROLLER (Чтение данных при спавне) ===
    public WeaponData[] GetSavedWeapons() => savedWeapons;
    public WeaponRarity[] GetSavedRarities() => savedRarities;

    // === МЕТОДЫ ДЛЯ ИГРОКА (Запись данных при подборе/крафте) ===
    public void SaveWeapon(int slotIndex, WeaponData weapon, WeaponRarity rarity)
    {
        if (slotIndex < 0 || slotIndex >= savedWeapons.Length) return;
        savedWeapons[slotIndex] = weapon;
        savedRarities[slotIndex] = rarity;
        Debug.Log($"[InventoryManager] Сохранено: {weapon.weaponName} ({rarity})");
        RefreshUI();
    }

    public void ClearWeapon(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= savedWeapons.Length) return;
        savedWeapons[slotIndex] = null;
        savedRarities[slotIndex] = WeaponRarity.Common;
        RefreshUI();
    }

    public void UpdateWeaponRarity(int slotIndex, WeaponRarity rarity)
    {
        if (slotIndex < 0 || slotIndex >= savedRarities.Length) return;
        savedRarities[slotIndex] = rarity;
        RefreshUI();
    }

    // === Ресурсы ===
    public void AddResource(int resourceIndex, int amount)
    {
        if (resourceIndex < 0 || resourceIndex >= materialsAmount.Length) return;
        materialsAmount[resourceIndex] += amount;
        Debug.Log($"[Инвентарь] +{amount} ресурса #{resourceIndex}. Всего: {materialsAmount[resourceIndex]}");
        RefreshUI();
    }

    public int GetResource(int index) => (index >= 0 && index < materialsAmount.Length) ? materialsAmount[index] : 0;
    public int[] GetAllResources() => materialsAmount;

    public bool CanAfford(int[] recipe)
    {
        if (recipe == null || recipe.Length != 4) return false;
        for (int i = 0; i < 4; i++) if (materialsAmount[i] < recipe[i]) return false;
        return true;
    }

    public void SpendResources(int[] recipe)
    {
        if (recipe == null) return;
        for (int i = 0; i < 4; i++) materialsAmount[i] -= recipe[i];
    }

    // === Проверки для UI ===
    public bool HasWeapon(WeaponType type)
    {
        for (int i = 0; i < savedWeapons.Length; i++)
            if (savedWeapons[i] != null && savedWeapons[i].weaponType == type) return true;
        return false;
    }

    public WeaponRarity GetCurrentRarity(WeaponType type)
    {
        for (int i = 0; i < savedWeapons.Length; i++)
            if (savedWeapons[i] != null && savedWeapons[i].weaponType == type) return savedRarities[i];
        return WeaponRarity.Common;
    }

    // === Крафт ===
    public void CraftPurchase(WeaponData weapon)
    {
        if (weapon == null || !CanAfford(weapon.purchaseRecipe)) return;
        if (HasWeaponOfType(weapon.weaponType)) return;

        SpendResources(weapon.purchaseRecipe);

        // Ищем пустой слот в глобальном хранилище
        int emptySlot = -1;
        for (int i = 0; i < savedWeapons.Length; i++)
        {
            if (savedWeapons[i] == null) { emptySlot = i; break; }
        }

        if (emptySlot != -1)
        {
            SaveWeapon(emptySlot, weapon, WeaponRarity.Common);
            // Пытаемся выдать оружие локальному игроку, если он есть на сцене
            PlayerWeaponInventory localInv = FindObjectOfType<PlayerWeaponInventory>();
            if (localInv != null) localInv.SetWeaponFromGlobal(emptySlot, weapon, WeaponRarity.Common);
        }
    }

    public void CraftUpgrade(WeaponData weapon)
    {
        if (weapon == null) return;
        int slotIndex = FindWeaponSlot(weapon.weaponType);
        if (slotIndex == -1) return;

        WeaponRarity currentRarity = savedRarities[slotIndex];
        if (currentRarity >= WeaponRarity.Legendary) return;

        int[] recipe = weapon.GetUpgradeRecipe(currentRarity);
        if (recipe == null || !CanAfford(recipe)) return;

        SpendResources(recipe);
        WeaponRarity nextRarity = weapon.GetNextRarity(currentRarity);

        UpdateWeaponRarity(slotIndex, nextRarity);

        // Обновляем локальному игроку, если он есть
        PlayerWeaponInventory localInv = FindObjectOfType<PlayerWeaponInventory>();
        if (localInv != null) localInv.SetWeaponRarity(slotIndex, nextRarity);
    }

    // === Вспомогательные ===
    private bool HasWeaponOfType(WeaponType type)
    {
        for (int i = 0; i < savedWeapons.Length; i++)
            if (savedWeapons[i] != null && savedWeapons[i].weaponType == type) return true;
        return false;
    }

    private int FindWeaponSlot(WeaponType type)
    {
        for (int i = 0; i < savedWeapons.Length; i++)
            if (savedWeapons[i] != null && savedWeapons[i].weaponType == type) return i;
        return -1;
    }

    private void RefreshUI()
    {
        InventoryUI ui = FindObjectOfType<InventoryUI>();
        if (ui != null) ui.RefreshAll();
    }
}