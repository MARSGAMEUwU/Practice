using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(WeaponController))]
public class PlayerWeaponInventory : MonoBehaviour
{
    [SerializeField] private InputAction pickupAction, dropAction;
    protected WeaponController weaponController;
    private WeaponPickup currentPickup;

    private void Awake() => weaponController = GetComponent<WeaponController>();

    private void OnEnable() { pickupAction?.Enable(); dropAction?.Enable(); }
    private void OnDisable() { pickupAction?.Disable(); dropAction?.Disable(); }

    private void Update()
    {
        if (pickupAction.WasPressedThisFrame() && currentPickup != null) { currentPickup.Pickup(); currentPickup = null; }
        if (dropAction.WasPressedThisFrame()) DropCurrentWeapon();
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
                // === ÑÎÕÐÀÍßÅÌ Â ÃËÎÁÀËÜÍÎÅ ÕÐÀÍÈËÈÙÅ ===
                InventoryManager.Instance?.SaveWeapon(i, weapon, rarity);
                return true;
            }
        }
        return false;
    }

    public void SetWeaponFromGlobal(int slotIndex, WeaponData weapon, WeaponRarity rarity)
    {
        weaponController.SetWeapon(slotIndex, weapon, rarity);
    }

    public void SetWeaponRarity(int slotIndex, WeaponRarity rarity)
    {
        weaponController.SetWeaponRarity(slotIndex, rarity);
    }

    public void SpawnWeaponPickup(WeaponData weapon, WeaponRarity rarity)
    {
        if (weapon == null) return;

        Vector3 dropPos = transform.position + transform.forward * 1.5f + Vector3.up * 0.5f;

        GameObject droppedObj = new GameObject($"Pickup_{weapon.weaponName}");
        droppedObj.tag = "WeaponPickup";
        droppedObj.layer = LayerMask.NameToLayer("Default");

        SphereCollider col = droppedObj.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 1f;

        if (weapon.weaponPrefab != null)
        {
            GameObject model = Instantiate(weapon.pickupPrefab, droppedObj.transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;

            foreach (var collider in model.GetComponentsInChildren<Collider>())
                Destroy(collider);
        }

        WeaponPickup pickup = droppedObj.AddComponent<WeaponPickup>();
        pickup.SetWeaponData(weapon);
        pickup.SetRarity(rarity);
        pickup.SetPickUpUI();

        droppedObj.transform.position = dropPos;
    }

    protected void DropCurrentWeapon()
    {
        WeaponData droppedWeapon = weaponController.GetCurrentWeapon();
        WeaponRarity droppedRarity = weaponController.GetCurrentRarity();

        if (droppedWeapon == null)
        {
            Debug.Log("[Inventory] Íåò îðóæèÿ äëÿ âûáðàñûâàíèÿ");
            return;
        }

        SpawnWeaponPickup(droppedWeapon, droppedRarity);
        weaponController.ClearCurrentWeapon();
        Debug.Log($"Âûáðîøåíî: {droppedWeapon.weaponName}");
    }

    // Ìåòîäû äëÿ InventoryManager
    public WeaponData GetWeaponInSlot(int i) => weaponController.GetWeaponInSlot(i);
    public WeaponRarity GetRarityInSlot(int i) => weaponController.GetRarityInSlot(i);
}