using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryUI : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction toggleAction; // Tab

    [Header("Ссылки")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerWeaponInventory playerInventory;

    [Header("Ссылки на плашки")]
    [SerializeField] private WeaponSlotUI pistolSlot;
    [SerializeField] private WeaponSlotUI smgSlot;
    [SerializeField] private WeaponSlotUI shotgunSlot;
    [SerializeField] private WeaponSlotUI rifleSlot;

    [Header("Отображение ресурсов")]
    [SerializeField] private ResourceDisplay resourceDisplay;

    [Header("Корневой объект UI")]
    [SerializeField] private GameObject rootPanel;

    private InventoryManager inventoryManager;
    private bool isOpen = false;
    private float previousTimeScale = 1f;

    private void Awake()
    {
        if (rootPanel != null) rootPanel.SetActive(false);
        if (toggleAction != null) toggleAction.Enable();

        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();

        if (playerInventory == null)
            playerInventory = FindObjectOfType<PlayerWeaponInventory>();
    }

    private void Start()
    {
        if (inventoryManager == null)
            inventoryManager = FindObjectOfType<InventoryManager>();

        RefreshAll();
    }

    private void Update()
    {
        if (toggleAction.WasPressedThisFrame())
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        if (isOpen) CloseInventory();
        else OpenInventory();
    }

    private void OpenInventory()
    {
        isOpen = true;

        if (playerController != null)
            playerController.LockControls();

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        if (rootPanel != null) rootPanel.SetActive(true);

        RefreshAll();
        Debug.Log("[UI] Инвентарь открыт");
    }

    private void CloseInventory()
    {
        isOpen = false;

        if (playerController != null)
            playerController.UnlockControls();

        Time.timeScale = previousTimeScale;

        if (rootPanel != null) rootPanel.SetActive(false);

        Debug.Log("[UI] Инвентарь закрыт");
    }

    public void RefreshAll()
    {
        if (inventoryManager == null) return;
        RefreshResources();
        RefreshSlots();
    }

    public void RefreshResources()
    {
        if (resourceDisplay != null && inventoryManager != null)
        {
            resourceDisplay.UpdateDisplay(inventoryManager.GetAllResources(), inventoryManager.resourceIcons);
        }
    }

    public void RefreshSlots()
    {
        if (inventoryManager == null) return;

        UpdateSlot(pistolSlot, WeaponType.Pistol);
        UpdateSlot(smgSlot, WeaponType.SMG);
        UpdateSlot(shotgunSlot, WeaponType.Shotgun);
        UpdateSlot(rifleSlot, WeaponType.Rifle);
    }

    private void UpdateSlot(WeaponSlotUI slot, WeaponType type)
    {
        if (slot == null) return;

        WeaponData weaponData = FindWeaponDataByType(type);

        bool hasWeapon = inventoryManager.HasWeapon(type);
        WeaponRarity currentRarity = inventoryManager.GetCurrentRarity(type);
        bool isMaxRarity = currentRarity >= WeaponRarity.Legendary && hasWeapon;

        slot.Setup(weaponData, hasWeapon, currentRarity, isMaxRarity, inventoryManager);
    }

    private WeaponData FindWeaponDataByType(WeaponType type)
    {
        string[] names = { "Pistol", "SMG", "Shotgun", "Rifle" };
        int index = (int)type;
        if (index < 0 || index >= names.Length) return null;

        return Resources.Load<WeaponData>("Weapons/" + names[index]);
    }
}