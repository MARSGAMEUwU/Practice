using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction toggleAction;

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

    private CanvasGroup canvasGroup;
    private InventoryManager inventoryManager;
    private bool isOpen = false;
    private float previousTimeScale = 1f;

    [Header("Настройки")]

    [SerializeField] private GameObject settingsbutton;
    [SerializeField] private GameObject settingslayer;

    private void Awake()
    {
        if (toggleAction != null) toggleAction.Enable();

        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();

        if (playerInventory == null)
            playerInventory = FindObjectOfType<PlayerWeaponInventory>();

        if (rootPanel != null)
        {
            canvasGroup = rootPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = rootPanel.AddComponent<CanvasGroup>();
        }
    }

    private void Start()
    {
        if (inventoryManager == null)
            inventoryManager = FindObjectOfType<InventoryManager>();

        if (rootPanel != null) rootPanel.SetActive(true);
        RefreshAll();

        SetAlpha(false);
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
        if (isOpen) return;
        isOpen = true;

        previousTimeScale = 1f;

        if (playerController != null)
            playerController.LockControls();

        SetAlpha(true);
        RefreshAll();
        Canvas.ForceUpdateCanvases();

        Time.timeScale = 0f;
        settingsbutton.SetActive(true);
        if (settingslayer != null) settingslayer.SetActive(false);

        Debug.Log($"[UI] Инвентарь открыт. Время остановлено. Старая скорость была: {previousTimeScale}");


    }

    private void CloseInventory()
    {
        if (!isOpen) return;
        isOpen = false;

        SetAlpha(false);

        if (playerController != null)
            playerController.UnlockControls();

        Time.timeScale = 1f;

        Debug.Log($"[UI] Инвентарь закрыт. Время восстановлено: {Time.timeScale}");
        settingsbutton.SetActive(false);
        if (settingslayer != null) settingslayer.SetActive(false);
    }

    private void SetAlpha(bool visible)
    {
        if (canvasGroup == null) return;

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
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
        UpdateSlot(smgSlot, WeaponType.SniperRifle);
        UpdateSlot(shotgunSlot, WeaponType.Rifle);
        UpdateSlot(rifleSlot, WeaponType.GrenadeLauncher);
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
        string[] names = { "Pistol", "SniperRifle", "Rifle", "GrenadeLauncher" };
        int index = (int)type;
        if (index < 0 || index >= names.Length) return null;

        return Resources.Load<WeaponData>("Weapons/" + names[index]);
    }
}