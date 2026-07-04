using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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

    // Хранение патронов для каждого оружия
    private int[] currentAmmoPerWeapon = new int[2];
    private bool[] weaponInitialized = new bool[2];
    private GameObject[] weaponInstances = new GameObject[2];

    // === НОВОЕ: Animator для каждого слота ===
    private Animator[] weaponAnimators = new Animator[2];

    // Singleton
    private static WeaponController instance;
    private bool isInitialized = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        if (cameraTransform == null) cameraTransform = Camera.main.transform;

        currentStats = new RarityStats();
        currentAmmo = 0;
        currentRecoil = 0f;
        currentSpread = 0f;
        isReloading = false;

        SceneManager.sceneLoaded += OnSceneLoaded;
        isInitialized = true;
        Debug.Log("[WeaponController] Инициализирован");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RestoreWeapons();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (instance == this) instance = null;
        for (int i = 0; i < weaponInstances.Length; i++)
        {
            if (weaponInstances[i] != null)
                Destroy(weaponInstances[i]);
        }
    }

    private void OnEnable()
    {
        if (shootAction != null) shootAction.Enable();
        if (reloadAction != null) reloadAction.Enable();
        if (switchWeapon1Action != null) switchWeapon1Action.Enable();
        if (switchWeapon2Action != null) switchWeapon2Action.Enable();
    }

    private void OnDisable()
    {
        if (shootAction != null) shootAction.Disable();
        if (reloadAction != null) reloadAction.Disable();
        if (switchWeapon1Action != null) switchWeapon1Action.Disable();
        if (switchWeapon2Action != null) switchWeapon2Action.Disable();
    }

    private void Update()
    {
        if (!isInitialized) return;
        HandleWeaponSwitch();
        HandleReload();
        HandleShooting();
        UpdateRecoilAndSpread();
    }

    private void HandleWeaponSwitch()
    {
        if (isReloading) return;
        if (switchWeapon1Action.WasPressedThisFrame() && weapons[0] != null)
            SwitchWeapon(0);
        if (switchWeapon2Action.WasPressedThisFrame() && weapons[1] != null)
            SwitchWeapon(1);
    }

    private void SwitchWeapon(int index)
    {
        if (weapons[index] == null) return;
        if (currentWeaponIndex == index) return;

        if (weaponInitialized[currentWeaponIndex])
            currentAmmoPerWeapon[currentWeaponIndex] = currentAmmo;

        currentWeaponIndex = index;
        currentStats = weapons[index].GetStatsForRarity(weaponRarities[index]);

        if (weaponInitialized[index])
        {
            currentAmmo = currentAmmoPerWeapon[index];
        }
        else
        {
            currentAmmo = currentStats.magazineSize;
            currentAmmoPerWeapon[index] = currentAmmo;
            weaponInitialized[index] = true;
        }

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
                  $"Урон: {currentStats.damage} | Магазин: {currentAmmo}/{currentStats.magazineSize}");
    }

    private void HandleReload()
    {
        if (isReloading) return;
        if (currentStats == null || weapons[currentWeaponIndex] == null) return;
        if (reloadAction.WasPressedThisFrame() && currentAmmo < currentStats.magazineSize)
        {
            StartCoroutine(ReloadRoutine());
        }
    }

    private System.Collections.IEnumerator ReloadRoutine()
    {
        if (currentStats == null) yield break;
        isReloading = true;
        yield return new WaitForSeconds(currentStats.reloadTime);
        currentAmmo = currentStats.magazineSize;
        currentAmmoPerWeapon[currentWeaponIndex] = currentAmmo;
        isReloading = false;
    }

    private void HandleShooting()
    {
        if (isReloading || weapons[currentWeaponIndex] == null || currentStats == null) return;
        if (shootAction.IsPressed() && Time.time >= nextFireTime)
        {
            if (currentAmmo > 0)
            {
                Shoot();
                nextFireTime = Time.time + currentStats.fireRate;
                currentAmmo--;
                currentAmmoPerWeapon[currentWeaponIndex] = currentAmmo;
                if (currentAmmo <= 0)
                {
                    StartCoroutine(ReloadRoutine());
                }
            }
            else
            {
                if (!isReloading)
                {
                    StartCoroutine(ReloadRoutine());
                }
            }
        }
    }

    private void Shoot()
    {
        if (crosshairController != null) crosshairController.OnShoot();

        // === НОВОЕ: Запускаем анимацию выстрела ===
        PlayShootAnimation();

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
            if (bloodHitEffectPrefab != null && hit.transform.TryGetComponent<Damageable>(out _))
            {
                GameObject hitEffect = Instantiate(bloodHitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(hitEffect, 1f);
            }
            if (holeHitEffectPrefab != null && !hit.transform.TryGetComponent<Damageable>(out _))
            {
                GameObject bulletHole = Instantiate(holeHitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                bulletHole.transform.position += hit.normal * 0.1f;
                Destroy(bulletHole, 10f);
            }
            if (dustEffectPrefab != null && !hit.transform.TryGetComponent<Damageable>(out _))
            {
                GameObject[] dustParticles = new GameObject[4];
                for (int i = 0; i < 4; i++)
                    dustParticles[i] = Instantiate(dustEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                for (int i = 0; i < 4; i++)
                    Destroy(dustParticles[i], 1f);
            }
        }

        SpawnMuzzleFlash();
        currentRecoil = Mathf.Min(currentRecoil + currentStats.recoilPerShot, currentStats.maxRecoil);
        currentSpread = Mathf.Min(currentSpread + currentStats.spreadPerShot, currentStats.maxSpread);
        ApplyRecoil();
    }

    // ======================================================
    // === НОВЫЕ МЕТОДЫ: АНИМАЦИЯ ЧЕРЕЗ ANIMATOR ===
    // ======================================================

    /// <summary>
    /// Проигрывает анимацию выстрела с умной скоростью
    /// </summary>
    private void PlayShootAnimation()
    {
        Animator animator = weaponAnimators[currentWeaponIndex];
        WeaponData weapon = weapons[currentWeaponIndex];

        if (animator == null)
        {
            Debug.LogWarning($"[WeaponController] Animator не найден для слота {currentWeaponIndex}");
            return;
        }

        if (weapon == null)
        {
            Debug.LogWarning($"[WeaponController] WeaponData не назначен для слота {currentWeaponIndex}");
            return;
        }

        if (weapon.shootAnimatorController == null)
        {
            Debug.LogWarning($"[WeaponController] Shoot Animator Controller не назначен в WeaponData для {weapon.weaponName}");
            return;
        }


        // === УМНАЯ СКОРОСТЬ АНИМАЦИИ ===
        // Получаем длительность анимации выстрела
        float animationDuration = GetShootAnimationDuration(animator, weapon);
        float fireRate = currentStats.fireRate;

        // Если fireRate меньше длительности анимации → ускоряем
        // Если fireRate больше или равен → нормальная скорость
        if (fireRate < animationDuration && fireRate > 0f)
        {
            animator.speed = animationDuration / fireRate;
            Debug.Log($"[WeaponController] Ускорение анимации: speed={animator.speed:F2} (fireRate={fireRate:F2}с, animDuration={animationDuration:F2}с)");
        }
        else
        {
            animator.speed = 1f;
            Debug.Log($"[WeaponController] Нормальная скорость анимации: speed=1 (fireRate={fireRate:F2}с, animDuration={animationDuration:F2}с)");
        }

        // Запускаем триггер
        animator.SetTrigger(weapon.shootTriggerName);
    }

    /// <summary>
    /// Получает длительность анимации выстрела из Animator
    /// </summary>
    private float GetShootAnimationDuration(Animator animator, WeaponData weapon)
    {
        // Ищем состояние с анимацией выстрела
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // Пробуем найти состояние по имени триггера
        foreach (AnimatorStateInfo info in new AnimatorStateInfo[] { stateInfo })
        {
            // Если текущее состояние - это анимация выстрела
            if (info.IsName(weapon.shootTriggerName))
            {
                return info.length;
            }
        }

        // Если не нашли, возвращаем длительность по умолчанию
        // Можно настроить это значение в WeaponData
        return 0.3f; // Значение по умолчанию
    }

    /// <summary>
    /// Настраивает Animator на инстансе оружия
    /// </summary>
    private void SetupWeaponAnimator(int slotIndex, WeaponData weapon)
    {
        if (weaponInstances[slotIndex] == null || weapon == null) return;

        // Ищем Animator на инстансе (может быть на дочернем объекте)
        Animator animator = weaponInstances[slotIndex].GetComponentInChildren<Animator>();

        if (animator == null)
        {
            Debug.LogWarning($"[WeaponController] Animator не найден на префабе {weapon.weaponName}");
            return;
        }

        // Применяем Animator Controller из WeaponData
        if (weapon.shootAnimatorController != null)
        {
            animator.runtimeAnimatorController = weapon.shootAnimatorController;
            Debug.Log($"[WeaponController] Animator Controller назначен для {weapon.weaponName}");
        }
        else
        {
            Debug.LogWarning($"[WeaponController] Shoot Animator Controller не назначен в WeaponData для {weapon.weaponName}");
        }

        weaponAnimators[slotIndex] = animator;
    }

    // ======================================================

    private void SpawnMuzzleFlash()
    {
        if (weaponInstances[currentWeaponIndex] == null) return;
        if (weapons[currentWeaponIndex].muzzleFlashPrefab == null) return;
        Transform muzzlePoint = weaponInstances[currentWeaponIndex].transform.Find("MuzzlePoint");
        if (muzzlePoint != null)
            Instantiate(weapons[currentWeaponIndex].muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation);
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
        if (weapons[currentWeaponIndex] == null || currentStats == null) return;
        currentRecoil = Mathf.Lerp(currentRecoil, 0, currentStats.recoilRecovery * Time.deltaTime);
        currentSpread = Mathf.Lerp(currentSpread, 0, currentStats.spreadRecovery * Time.deltaTime);
    }

    // === ПУБЛИЧНЫЕ МЕТОДЫ ===
    public float GetCurrentSpread()
    {
        if (currentStats == null || weapons[currentWeaponIndex] == null) return 0f;
        return currentSpread + currentStats.baseSpread;
    }

    public float GetMaxSpread()
    {
        if (currentStats == null || weapons[currentWeaponIndex] == null) return 1f;
        return currentStats.maxSpread;
    }

    public void SetWeapon(int slotIndex, WeaponData weapon, WeaponRarity rarity)
    {
        if (slotIndex < 0 || slotIndex >= weapons.Length) return;

        if (weaponInstances[slotIndex] != null)
            Destroy(weaponInstances[slotIndex]);

        weapons[slotIndex] = weapon;
        weaponRarities[slotIndex] = rarity;

        RarityStats stats = weapon.GetStatsForRarity(rarity);
        currentAmmoPerWeapon[slotIndex] = stats.magazineSize;
        weaponInitialized[slotIndex] = true;

        if (slotIndex == currentWeaponIndex)
        {
            currentStats = stats;
            currentAmmo = stats.magazineSize;
            currentRecoil = 0f;
            currentSpread = 0f;
            isReloading = false;
        }

        if (weapon.weaponPrefab != null && weaponHolder != null)
        {
            weaponInstances[slotIndex] = Instantiate(weapon.weaponPrefab);
            weaponInstances[slotIndex].transform.SetParent(weaponHolder);
            weaponInstances[slotIndex].transform.localPosition = Vector3.zero;
            weaponInstances[slotIndex].transform.localRotation = Quaternion.identity;
            weaponInstances[slotIndex].transform.localScale = Vector3.one;
            weaponInstances[slotIndex].SetActive(slotIndex == currentWeaponIndex);

            int weaponLayer = LayerMask.NameToLayer("WeaponLayer");
            if (weaponLayer != -1)
                SetLayerRecursively(weaponInstances[slotIndex], weaponLayer);

            // Настраиваем Animator
            SetupWeaponAnimator(slotIndex, weapon);
        }
    }

    public WeaponData GetWeaponInSlot(int i)
    {
        if (i < 0 || i >= weapons.Length) return null;
        return weapons[i];
    }

    public WeaponData GetCurrentWeapon() => weapons[currentWeaponIndex];
    public WeaponRarity GetCurrentRarity() => weaponRarities[currentWeaponIndex];
    public int GetCurrentWeaponIndex() => currentWeaponIndex;
    public int GetCurrentAmmo() => currentAmmo;
    public int GetMaxAmmo() => currentStats != null ? currentStats.magazineSize : 0;

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
        weaponAnimators[currentWeaponIndex] = null;

        currentAmmoPerWeapon[currentWeaponIndex] = 0;
        weaponInitialized[currentWeaponIndex] = false;

        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null)
            {
                SwitchWeapon(i);
                return;
            }
        }

        currentWeaponIndex = 0;
        currentAmmo = 0;
        currentStats = new RarityStats();
    }

    private void RestoreWeapons()
    {
        for (int i = 0; i < weaponInstances.Length; i++)
        {
            if (weaponInstances[i] != null) Destroy(weaponInstances[i]);
            weaponInstances[i] = null;
            weaponAnimators[i] = null;
        }

        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null && weaponHolder != null)
            {
                weaponInstances[i] = Instantiate(weapons[i].weaponPrefab);
                weaponInstances[i].transform.SetParent(weaponHolder);
                weaponInstances[i].transform.localPosition = Vector3.zero;
                weaponInstances[i].transform.localRotation = Quaternion.identity;
                weaponInstances[i].transform.localScale = Vector3.one;
                weaponInstances[i].SetActive(i == currentWeaponIndex);

                int weaponLayer = LayerMask.NameToLayer("WeaponLayer");
                if (weaponLayer != -1)
                    SetLayerRecursively(weaponInstances[i], weaponLayer);

                SetupWeaponAnimator(i, weapons[i]);
            }
        }

        if (weapons[currentWeaponIndex] != null)
        {
            currentStats = weapons[currentWeaponIndex].GetStatsForRarity(weaponRarities[currentWeaponIndex]);
            if (weaponInitialized[currentWeaponIndex])
                currentAmmo = currentAmmoPerWeapon[currentWeaponIndex];
            else
            {
                currentAmmo = currentStats.magazineSize;
                currentAmmoPerWeapon[currentWeaponIndex] = currentAmmo;
                weaponInitialized[currentWeaponIndex] = true;
            }
        }
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}