using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform weaponHolder;
    public GameObject bloodHitEffectPrefab;
    public GameObject holeHitEffectPrefab;
    public GameObject dustEffectPrefab;

    [Header("Input Actions")]
    [SerializeField] private InputAction shootAction;
    [SerializeField] private InputAction reloadAction;
    [SerializeField] private InputAction switchWeapon1Action;
    [SerializeField] private InputAction switchWeapon2Action;

    [Header("Оружие")]
    [SerializeField] private WeaponData[] weapons = new WeaponData[2];
    [SerializeField] private WeaponRarity[] weaponRarities = new WeaponRarity[2];
    [SerializeField] private int currentWeaponIndex = 0;

    [Header("Декали")]
    [SerializeField] private float decalSize = 0.5f;
    [SerializeField] private float decalLifetime = 10f;
    [SerializeField] private LayerMask impactLayers;

    [Header("Прицел")]
    [SerializeField] private CrosshairController crosshairController;

    // Состояние
    private RarityStats currentStats;
    private float nextFireTime;
    private float currentRecoil;
    private float currentSpread;
    private int currentAmmo;
    private bool isReloading;

    private GameObject[] weaponInstances = new GameObject[2];

    private void Awake()
    {
        if (cameraTransform == null) cameraTransform = Camera.main.transform;
    }

    private void OnEnable()
    {
        shootAction.Enable();
        reloadAction.Enable();
        switchWeapon1Action.Enable();
        switchWeapon2Action.Enable();
    }

    private void OnDisable()
    {
        shootAction.Disable();
        reloadAction.Disable();
        switchWeapon1Action.Disable();
        switchWeapon2Action.Disable();
    }

    private void Update()
    {
        HandleWeaponSwitch();
        HandleReload();
        HandleShooting();
        UpdateRecoilAndSpread();
    }

    private void HandleWeaponSwitch()
    {
        if (switchWeapon1Action.WasPressedThisFrame() && weapons[0] != null) SwitchWeapon(0);
        if (switchWeapon2Action.WasPressedThisFrame() && weapons[1] != null) SwitchWeapon(1);
    }

    private void SwitchWeapon(int index)
    {
        if (weapons[index] == null) return;

        currentWeaponIndex = index;
        currentStats = weapons[index].GetStatsForRarity(weaponRarities[index]);
        currentAmmo = currentStats.magazineSize;
        currentRecoil = 0f;
        currentSpread = 0f;
        isReloading = false;

        for (int i = 0; i < weaponInstances.Length; i++)
        {
            if (weaponInstances[i] != null)
                weaponInstances[i].SetActive(i == index);
        }

        WeaponData w = weapons[index];
        Color color = w.GetRarityColor(weaponRarities[index]);
        Debug.Log($"<color=#{ColorUtility.ToHtmlStringRGB(color)}>" +
                  $"[{w.GetRarityName(weaponRarities[index])}]</color> {w.weaponName} | " +
                  $"Урон: {currentStats.damage} | Магазин: {currentStats.magazineSize}");
    }

    private void HandleReload()
    {
        if (reloadAction.WasPressedThisFrame() && !isReloading &&
            currentAmmo < currentStats.magazineSize && weapons[currentWeaponIndex] != null)
            StartCoroutine(ReloadRoutine());
    }

    private System.Collections.IEnumerator ReloadRoutine()
    {
        isReloading = true;
        yield return new WaitForSeconds(currentStats.reloadTime);
        currentAmmo = currentStats.magazineSize;
        isReloading = false;
    }

    private void HandleShooting()
    {
        if (isReloading || weapons[currentWeaponIndex] == null) return;
        if (shootAction.IsPressed() && Time.time >= nextFireTime)
        {
            if (currentAmmo > 0)
            {
                Shoot();
                nextFireTime = Time.time + currentStats.fireRate;
                currentAmmo--;
                if (currentAmmo <= 0) StartCoroutine(ReloadRoutine());
            }
        }
    }

    private void Shoot()
    {
        if (crosshairController != null) crosshairController.OnShoot();

        Vector3 shootDir = GetSpreadDirection();
        Ray ray = new Ray(cameraTransform.position, shootDir);

        if (Physics.Raycast(ray, out RaycastHit hit, currentStats.range, impactLayers))
        {
            Damageable damageable = hit.collider.GetComponent<Damageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(currentStats.damage);

                if (damageable.IsDead())
                {
                    if (crosshairController != null) crosshairController.OnHitKill();
                }
                else
                {
                    if (crosshairController != null) crosshairController.OnHit();
                }
            }
            if (bloodHitEffectPrefab != null && hit.transform.name == "Capsule")
            {
                // Создаем эффект, разворачиваем его в сторону нормали поверхности (чтобы искры летели от стены)
                GameObject hitEffect = Instantiate(bloodHitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                // Уничтожаем эффект через 1 секунду, чтобы не засорять память
                Destroy(hitEffect, 1f);
            }
            if (holeHitEffectPrefab != null)
            {
                GameObject bulletHole = Instantiate(holeHitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                bulletHole.transform.position += hit.normal * 0.01f;
                // Уничтожаем эффект через 1 секунду, чтобы не засорять память
                Destroy(bulletHole, 10f);
            }
            if (dustEffectPrefab != null && hit.transform.name != "Capsule")
            {
                // Создаем эффект, разворачиваем его в сторону нормали поверхности (чтобы искры летели от стены)
                GameObject[] dustParticles = new GameObject[4];
                for (int i = 0; i < 4; i++)
                { dustParticles[i] = Instantiate(dustEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal)); }
                // Уничтожаем эффект через 1 секунду, чтобы не засорять память
                for (int i = 0; i < 4; i++) { Destroy(dustParticles[i], 1f); }
            }
        }

        SpawnMuzzleFlash();

        currentRecoil = Mathf.Min(currentRecoil + currentStats.recoilPerShot, currentStats.maxRecoil);
        currentSpread = Mathf.Min(currentSpread + currentStats.spreadPerShot, currentStats.maxSpread);
        ApplyRecoil();
    }

    private void SpawnMuzzleFlash()
    {
        if (weaponInstances[currentWeaponIndex] == null) return;
        if (weapons[currentWeaponIndex].muzzleFlashPrefab == null) return;

        Transform muzzlePoint = weaponInstances[currentWeaponIndex].transform.Find("MuzzlePoint");
        if (muzzlePoint != null)
        {
            Instantiate(weapons[currentWeaponIndex].muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation);
        }
    }

    private Vector3 GetSpreadDirection()
    {
        Vector3 dir = cameraTransform.forward;
        float totalSpread = currentStats.baseSpread + currentSpread;
        float sx = Random.Range(-totalSpread, totalSpread);
        float sy = Random.Range(-totalSpread, totalSpread);
        return Quaternion.Euler(sy, sx, 0) * dir;
    }

    private void ApplyRecoil()
    {
        float recoilAmount = currentStats.baseRecoil + currentRecoil;

        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            float verticalRecoil = -recoilAmount;
            float horizontalRecoil = Random.Range(-recoilAmount * 0.3f, recoilAmount * 0.3f);
            playerController.AddRecoil(verticalRecoil, horizontalRecoil);
        }
    }

    private void UpdateRecoilAndSpread()
    {
        if (weapons[currentWeaponIndex] == null) return;
        currentRecoil = Mathf.Lerp(currentRecoil, 0, currentStats.recoilRecovery * Time.deltaTime);
        currentSpread = Mathf.Lerp(currentSpread, 0, currentStats.spreadRecovery * Time.deltaTime);
    }

    private void CreateDecal(RaycastHit hit)
    {
        if (weapons[currentWeaponIndex].impactDecals == null ||
            weapons[currentWeaponIndex].impactDecals.Length == 0) return;

        Sprite decal = weapons[currentWeaponIndex].impactDecals[
            Random.Range(0, weapons[currentWeaponIndex].impactDecals.Length)];

        GameObject decalObj = new GameObject("Decal");
        decalObj.transform.position = hit.point + hit.normal * 0.01f;
        decalObj.transform.rotation = Quaternion.LookRotation(-hit.normal);

        SpriteRenderer sr = decalObj.AddComponent<SpriteRenderer>();
        sr.sprite = decal;
        sr.sortingOrder = 100;
        decalObj.transform.localScale = Vector3.one * decalSize;

        Destroy(decalObj, decalLifetime);
    }

    // === ПУБЛИЧНЫЕ МЕТОДЫ ===

    public float GetCurrentSpread() => currentSpread + (weapons[currentWeaponIndex] != null ? currentStats.baseSpread : 0f);
    public float GetMaxSpread() => weapons[currentWeaponIndex] != null ? currentStats.maxSpread : 1f;

    public void SetWeapon(int slotIndex, WeaponData weapon, WeaponRarity rarity)
    {
        if (slotIndex < 0 || slotIndex >= weapons.Length) return;

        if (weaponInstances[slotIndex] != null)
            Destroy(weaponInstances[slotIndex]);

        weapons[slotIndex] = weapon;
        weaponRarities[slotIndex] = rarity;

        if (weapon.weaponPrefab != null && weaponHolder != null)
        {
            // Создаём инстанс
            weaponInstances[slotIndex] = Instantiate(weapon.weaponPrefab);
            weaponInstances[slotIndex].transform.SetParent(weaponHolder);
            weaponInstances[slotIndex].transform.localPosition = Vector3.zero;
            weaponInstances[slotIndex].transform.localRotation = Quaternion.identity;
            weaponInstances[slotIndex].transform.localScale = Vector3.one;
            weaponInstances[slotIndex].SetActive(slotIndex == currentWeaponIndex);

            // === НОВОЕ: Принудительно устанавливаем слой WeaponLayer ===
            int weaponLayer = LayerMask.NameToLayer("WeaponLayer");
            if (weaponLayer != -1)
            {
                SetLayerRecursively(weaponInstances[slotIndex], weaponLayer);
                Debug.Log($"[WeaponController] Слой оружия установлен на WeaponLayer");
            }
            else
            {
                Debug.LogError("[WeaponController] Слой 'WeaponLayer' не найден! Создайте его в Edit → Project Settings → Tags and Layers");
            }
        }

        if (currentWeaponIndex == slotIndex) SwitchWeapon(slotIndex);
    }

    public WeaponData GetWeaponInSlot(int i)
    {
        if (i < 0 || i >= weapons.Length) return null;
        return weapons[i];
    }
    public WeaponData GetCurrentWeapon() => weapons[currentWeaponIndex];
    public WeaponRarity GetCurrentRarity() => weaponRarities[currentWeaponIndex];
    public int GetCurrentWeaponIndex() => currentWeaponIndex;

    // === НОВЫЕ МЕТОДЫ ДЛЯ INVENTORY MANAGER ===

    public WeaponRarity GetRarityInSlot(int i)
    {
        if (i < 0 || i >= weaponRarities.Length) return WeaponRarity.Common;
        return weaponRarities[i];
    }

    public void SetWeaponRarity(int slotIndex, WeaponRarity rarity)
    {
        if (slotIndex < 0 || slotIndex >= weaponRarities.Length) return;
        weaponRarities[slotIndex] = rarity;
        if (currentWeaponIndex == slotIndex) SwitchWeapon(slotIndex);
    }

    public void ClearCurrentWeapon()
    {
        if (weaponInstances[currentWeaponIndex] != null)
            Destroy(weaponInstances[currentWeaponIndex]);

        weapons[currentWeaponIndex] = null;
        weaponInstances[currentWeaponIndex] = null;

        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null) { SwitchWeapon(i); return; }
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < weaponInstances.Length; i++)
        {
            if (weaponInstances[i] != null)
                Destroy(weaponInstances[i]);
        }
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}