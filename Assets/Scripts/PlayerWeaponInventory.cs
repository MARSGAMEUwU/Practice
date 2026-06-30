using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(WeaponController))]
public class PlayerWeaponInventory : MonoBehaviour
{
    [SerializeField] private InputAction pickupAction;
    [SerializeField] private InputAction dropAction;

    private WeaponController weaponController;
    private WeaponPickup currentPickup;

    private void Awake() => weaponController = GetComponent<WeaponController>();

    private void OnEnable()
    {
        pickupAction.Enable();
        dropAction.Enable();
    }

    private void OnDisable()
    {
        pickupAction.Disable();
        dropAction.Disable();
    }

    private void Update()
    {
        if (pickupAction.WasPressedThisFrame() && currentPickup != null)
        {
            currentPickup.Pickup();
            currentPickup = null;
        }
        if (dropAction.WasPressedThisFrame()) DropCurrentWeapon();
    }

    public void SetCurrentPickup(WeaponPickup pickup) => currentPickup = pickup;
    public void ClearCurrentPickup(WeaponPickup pickup)
    {
        if (currentPickup == pickup) currentPickup = null;
    }

    public bool AddWeapon(WeaponData weapon, WeaponRarity rarity)
    {
        if (weapon == null) return false;

        // Ищем пустой слот
        for (int i = 0; i < 2; i++)
        {
            if (weaponController.GetWeaponInSlot(i) == null)
            {
                weaponController.SetWeapon(i, weapon, rarity);
                return true;
            }
        }

        // Все слоты заняты - можно заменить текущее оружие (опционально)
        // weaponController.SetWeapon(weaponController.GetCurrentWeaponIndex(), weapon, rarity);
        // return true;

        return false;
    }

    private void DropCurrentWeapon()
    {
        WeaponData droppedWeapon = weaponController.GetCurrentWeapon();
        WeaponRarity droppedRarity = weaponController.GetCurrentRarity();
        if (droppedWeapon == null) return;

        Vector3 dropPos = transform.position + transform.forward * 1.5f + Vector3.up * 0.5f;

        // Создаём объект подбора в мире
        GameObject droppedObj = new GameObject($"Pickup_{droppedWeapon.weaponName}");
        droppedObj.tag = "WeaponPickup";
        droppedObj.layer = LayerMask.NameToLayer("Default");

        // Добавляем коллайдер
        SphereCollider col = droppedObj.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 1f;

        // Добавляем визуальную модель (копию из WeaponData)
        if (droppedWeapon.weaponPrefab != null)
        {
            GameObject model = Instantiate(droppedWeapon.weaponPrefab, droppedObj.transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;

            // Удаляем лишние компоненты (если есть)
            foreach (var collider in model.GetComponentsInChildren<Collider>())
                Destroy(collider);
        }

        // Добавляем компонент подбора
        WeaponPickup pickup = droppedObj.AddComponent<WeaponPickup>();
        pickup.SetWeaponData(droppedWeapon);
        pickup.SetRarity(droppedRarity);
        pickup.SetPickUpUI();

        droppedObj.transform.position = dropPos;

        weaponController.ClearCurrentWeapon();
        Debug.Log($"Выкинуто: {droppedWeapon.weaponName}");
    }
}