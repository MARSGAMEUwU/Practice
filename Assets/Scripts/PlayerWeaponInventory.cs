using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(WeaponController))]
public class PlayerWeaponInventory : MonoBehaviour
{
    [SerializeField] private InputAction pickupAction;
    [SerializeField] private InputAction dropAction;

    protected WeaponController weaponController;
    private WeaponPickup currentPickup;

    private void Awake() => weaponController = GetComponent<WeaponController>();

    private void OnEnable() { pickupAction?.Enable(); dropAction?.Enable(); }
    private void OnDisable() { pickupAction?.Disable(); dropAction?.Disable(); }

    private void Update()
    {
        if (pickupAction.WasPressedThisFrame() && currentPickup != null)
        {
            currentPickup.Pickup();
            currentPickup = null;
        }

        if (dropAction.WasPressedThisFrame() && !weaponController.IsReloading)
        {
            DropCurrentWeapon();
        }
    }

    public void SetCurrentPickup(WeaponPickup pickup) => currentPickup = pickup;
    public void ClearCurrentPickup(WeaponPickup pickup) { if (currentPickup == pickup) currentPickup = null; }

    public bool AddWeapon(WeaponData weapon, WeaponRarity rarity)
    {
        if (weapon == null) return false;

        for (int i = 0; i < 2; i++)
        {
            if (weaponController.GetWeaponInSlot(i) == null)
            {
                weaponController.SetWeapon(i, weapon, rarity);
                InventoryManager.Instance?.SaveWeapon(i, weapon, rarity);
                return true;
            }
        }
        return false;
    }

    public void SetWeaponRarity(int slotIndex, WeaponRarity rarity)
    {
        weaponController.SetWeaponRarity(slotIndex, rarity);
    }
    public void SpawnWeaponPickup(WeaponData weapon, WeaponRarity rarity)
    {
        if (weapon == null) return;

        GameObject prefabToSpawn = weapon.GetPickupPrefab();
        if (prefabToSpawn == null)
        {
            Debug.LogError($"[WeaponPickup] У оружия {weapon.weaponName} не задан ни pickupPrefab, ни weaponPrefab!");
            return;
        }

        Vector3 dropPos = transform.position + transform.forward * 1.5f + Vector3.up * 0.5f;
        GameObject droppedObj = new GameObject($"Pickup_{weapon.weaponName}");

        try { droppedObj.tag = "WeaponPickup"; } catch { }

        SphereCollider col = droppedObj.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 1f;

        GameObject model = Instantiate(prefabToSpawn, droppedObj.transform);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        foreach (var collider in model.GetComponentsInChildren<Collider>()) Destroy(collider);

        WeaponPickup pickup = droppedObj.AddComponent<WeaponPickup>();
        pickup.SetWeaponData(weapon);
        pickup.SetRarity(rarity);
        pickup.SetPickUpUI();
        droppedObj.transform.position = dropPos;
    }

    protected void DropCurrentWeapon()
    {
        int slotIndex = weaponController.GetCurrentWeaponIndex();
        WeaponData droppedWeapon = weaponController.GetWeaponInSlot(slotIndex);
        WeaponRarity droppedRarity = weaponController.GetRarityInSlot(slotIndex);

        if (droppedWeapon == null) return;

        SpawnWeaponPickup(droppedWeapon, droppedRarity);

        weaponController.ClearCurrentWeapon();

        InventoryManager.Instance?.ClearWeapon(slotIndex);
    }

    public WeaponData GetWeaponInSlot(int i) => weaponController.GetWeaponInSlot(i);
    public WeaponRarity GetRarityInSlot(int i) => weaponController.GetRarityInSlot(i);
}