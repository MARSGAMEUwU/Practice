using TMPro;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private WeaponRarity rarity = WeaponRarity.Common;

    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float floatAmplitude = 0.2f;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private GameObject pickupPromptUI;
    [SerializeField] private Transform canvasTransform;

    public void SpawnPickupUI()
    {
        GameObject PickUpText = Instantiate(pickupPromptUI, canvasTransform);
    }

    private Vector3 startPosition;
    private PlayerWeaponInventory playerInventory;

    private void Start()
    {
        startPosition = transform.position;
        if (pickupPromptUI != null) pickupPromptUI.SetActive(false);
        ApplyRarityVisuals();
    }

    private void ApplyRarityVisuals()
    {
        if (weaponData == null) return;

        Renderer modelRenderer = GetComponentInChildren<Renderer>();
        if (modelRenderer != null)
        {
            Material mat = new Material(modelRenderer.material);
            mat.color = weaponData.GetRarityColor(rarity);
            modelRenderer.material = mat;
        }
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        playerInventory = other.GetComponent<PlayerWeaponInventory>();
        if (playerInventory != null)
        {
            playerInventory.SetCurrentPickup(this);
            if (pickupPromptUI != null) pickupPromptUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        if (playerInventory != null)
        {
            playerInventory.ClearCurrentPickup(this);
        }
        
        playerInventory = null;
        if (pickupPromptUI != null) pickupPromptUI.SetActive(false);
    }

    public void Pickup()
    {
        if (playerInventory == null || weaponData == null) return;
        
        if (playerInventory.AddWeapon(weaponData, rarity))
        {
            Debug.Log($"Подобрано: {weaponData.weaponName} ({weaponData.GetRarityName(rarity)})");
            
            // === ИСПРАВЛЕНИЕ: очищаем ссылку и скрываем подсказку ===
            if (playerInventory != null)
            {
                playerInventory.ClearCurrentPickup(this);
            }
            
            if (pickupPromptUI != null)
            {
                pickupPromptUI.SetActive(false);
            }
            
            Destroy(gameObject);
        }
    }

    public void SetWeaponData(WeaponData data) => weaponData = data;
    public void SetRarity(WeaponRarity r) => rarity = r;
    public void SetPickUpUI() => pickupPromptUI = GameObject.FindGameObjectWithTag("PickUpPrompt");
}