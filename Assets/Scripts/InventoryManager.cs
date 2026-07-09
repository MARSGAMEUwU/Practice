using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Глобальное хранилище оружия")]
    [SerializeField] private WeaponData[] savedWeapons = new WeaponData[2];
    [SerializeField] private WeaponRarity[] savedRarities = new WeaponRarity[2];

    [Header("Сохранение состояния игрока")]
    public float savedAdrenaline = 0f;
    public int savedSyringes = 0;

    [Header("Ресурсы")]
    private int[] materialsAmount = new int[4];

    [Header("Иконки ресурсов для UI")]
    public Sprite barrelIcon;
    public Sprite magazineIcon;
    public Sprite handleIcon;
    public Sprite scopeIcon;
    public Sprite[] resourceIcons;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Живет вечно
        resourceIcons = new Sprite[] { barrelIcon, magazineIcon, handleIcon, scopeIcon };
    }

    // === ГЕТТЕРЫ И СЕТТЕРЫ ДЛЯ ГЛОБАЛЬНОГО ХРАНИЛИЩА ===
    public WeaponData[] GetSavedWeapons() => savedWeapons;
    public WeaponRarity[] GetSavedRarities() => savedRarities;

    public void SaveWeapon(int slotIndex, WeaponData weapon, WeaponRarity rarity)
    {
        if (slotIndex < 0 || slotIndex >= savedWeapons.Length) return;
        savedWeapons[slotIndex] = weapon;
        savedRarities[slotIndex] = rarity;
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

    // === РЕСУРСЫ ===
    public void AddResource(int resourceIndex, int amount)
    {
        if (resourceIndex < 0 || resourceIndex >= materialsAmount.Length) return;
        materialsAmount[resourceIndex] += amount;
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

    // === ПРОВЕРКИ (Теперь смотрят в глобальные массивы, а не в игрока!) ===
    public bool HasWeapon(WeaponType type)
    {
        for (int i = 0; i < savedWeapons.Length; i++)
            if (savedWeapons[i] != null && savedWeapons[i].weaponType == type) return true;
        return false;
    }

    private bool HasWeaponOfType(WeaponType type) => HasWeapon(type);

    public WeaponRarity GetCurrentRarity(WeaponType type)
    {
        for (int i = 0; i < savedWeapons.Length; i++)
            if (savedWeapons[i] != null && savedWeapons[i].weaponType == type) return savedRarities[i];
        return WeaponRarity.Common;
    }

    private int FindWeaponSlot(WeaponType type)
    {
        for (int i = 0; i < savedWeapons.Length; i++)
            if (savedWeapons[i] != null && savedWeapons[i].weaponType == type) return i;
        return -1;
    }

    private int FindEmptySlot()
    {
        for (int i = 0; i < savedWeapons.Length; i++)
            if (savedWeapons[i] == null) return i;
        return -1;
    }

    // === КРАФТ (Поиск игрока "на лету") ===
    public void CraftPurchase(WeaponData weapon)
    {
        if (weapon == null || !CanAfford(weapon.purchaseRecipe) || HasWeaponOfType(weapon.weaponType)) return;

        SpendResources(weapon.purchaseRecipe);

        // Ищем локального игрока на сцене динамически
        PlayerWeaponInventory localPlayer = FindObjectOfType<PlayerWeaponInventory>();
        bool addedToHands = false;

        if (localPlayer != null)
        {
            addedToHands = localPlayer.AddWeapon(weapon, WeaponRarity.Common);
        }

        // Если в руки не взяли (слоты заняты) — спавним на землю
        if (!addedToHands)
        {
            if (localPlayer != null)
            {
                localPlayer.SpawnWeaponPickup(weapon, WeaponRarity.Common);
                Debug.Log($"<color=yellow>[Крафт] Слоты заняты! {weapon.weaponName} упал на землю.</color>");
            }
            else
            {
                Debug.LogError("<color=red>[Крафт] Игрок не найден, а слоты заняты! Оружие потеряно.</color>");
            }
        }

        // Сохраняем факт покупки в глобальное хранилище
        int slot = FindEmptySlot();
        if (slot != -1) SaveWeapon(slot, weapon, WeaponRarity.Common);

        RefreshUI();
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

        // Обновляем локальному игроку, если он есть на сцене
        PlayerWeaponInventory localPlayer = FindObjectOfType<PlayerWeaponInventory>();
        if (localPlayer != null) localPlayer.SetWeaponRarity(slotIndex, nextRarity);

        RefreshUI();
    }

    // === СБРОС ЗАБЕГА (При смерти или победе) ===
    public void ResetRunData()
    {
        for (int i = 0; i < savedWeapons.Length; i++) { savedWeapons[i] = null; savedRarities[i] = WeaponRarity.Common; }
        for (int i = 0; i < materialsAmount.Length; i++) materialsAmount[i] = 0;
        savedAdrenaline = 0f;
        savedSyringes = 0;
        Debug.Log("<color=yellow>[InventoryManager] ✅ Прогресс забега сброшен.</color>");
    }

    private void RefreshUI()
    {
        InventoryUI ui = FindObjectOfType<InventoryUI>();
        if (ui != null) { ui.RefreshResources(); ui.RefreshSlots(); }
    }
}