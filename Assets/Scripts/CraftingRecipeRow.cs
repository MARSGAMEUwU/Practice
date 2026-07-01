using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingRecipeRow : MonoBehaviour
{
    [Header("Настройки рецепта")]
    [SerializeField] private WeaponData weaponToCraft; // Какое оружие скрафтим
    [SerializeField] private WeaponRarity rarity = WeaponRarity.Common; // Какую редкость выдаем по умолчанию

    [Header("Требуемые ресурсы (Индексы: 0-Ствол, 1-Маг, 2-Ручка, 3-Прицел)")]
    [SerializeField] private int material1Index = 0;
    [SerializeField] private int material1Cost = 1;
    [SerializeField] private int material2Index = 2;
    [SerializeField] private int material2Cost = 2;

    [Header("Ссылки на компоненты UI строки")]
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private Image weaponIconImage;
    [SerializeField] private Button craftButton;
    [SerializeField] private TextMeshProUGUI costText; // Временный текст для отображения цены (например: "Ствол x1 | Ручка x2")

    private InventoryManager playerInventory;

    private void Start()
    {
        // Находим инвентарь игрока на сцене
        playerInventory = FindFirstObjectByType<InventoryManager>();

        // Настраиваем визуал строки из данных WeaponData
        if (weaponToCraft != null)
        {
            if (weaponNameText != null) weaponNameText.text = weaponToCraft.weaponName;
            if (weaponIconImage != null) weaponIconImage.sprite = weaponToCraft.weaponIcon;
        }

        // Выводим стоимость текстом (пока не настроены отдельные иконки ресурсов)
        if (costText != null)
        {
            string[] resourceNames = { "Ствол", "Магазин", "Рукоять", "Прицел" };
            costText.text = $"{resourceNames[material1Index]} x{material1Cost} \n {resourceNames[material2Index]} x{material2Cost}";
        }

        // Подписываем кнопку на метод крафта
        if (craftButton != null)
        {
            craftButton.onClick.AddListener(OnCraftButtonClicked);
        }
    }

    private void OnCraftButtonClicked()
    {
        if (playerInventory == null || weaponToCraft == null) return;

        // Передаем данные в твой метод крафта из InventoryManager
        playerInventory.Craft(material1Index, material1Cost, material2Index, material2Cost, weaponToCraft, rarity);
    }
}