using UnityEngine;

public class InventoryManager : PlayerWeaponInventory
{
    // Будем использовать твой массив, так гораздо удобнее управлять ресурсами по индексам:
    // 0 = Ствол, 1 = Магазин, 2 = Рукоять, 3 = Прицел
    private int[] materialsAmount = new int[4];

    [Header("Иконки для UI (пригодятся для интерфейса)")]
    public Sprite barrelIcon;
    public Sprite magazineIcon;
    public Sprite handleIcon;
    public Sprite scopeIcon;

    // Публичный метод, который будут вызывать трупы при лутании
    public void LootEnemy(int resourceIndex, int amount)
    {
        if (resourceIndex >= 0 && resourceIndex < materialsAmount.Length)
        {
            materialsAmount[resourceIndex] += amount;

            // Массив строк исключительно для красивого вывода в консоль
            string[] resourceNames = { "Ствол", "Магазин", "Рукоять", "Прицел" };
            Debug.Log($"[Инвентарь] Получено: {resourceNames[resourceIndex]} x{amount}. " +
                      $"Всего в наличии: {materialsAmount[resourceIndex]}");
        }
    }

    // Вспомогательные публичные методы, чтобы другие скрипты (например, крафт) могли узнать количество
    public int GetBarrels() => materialsAmount[0];
    public int GetMagazines() => materialsAmount[1];
    public int GetHandles() => materialsAmount[2];
    public int GetScopes() => materialsAmount[3];

    public void Craft(int slot1, int amount1, int slot2, int amount2, WeaponData weaponToCraft, WeaponRarity rarity)
    {
        // Сразу списываем материалы, так как крафт в любом случае состоится (в руки, в карман или на землю)
        materialsAmount[slot1] -= amount1;
        materialsAmount[slot2] -= amount2;

        bool upgradedExisting = false;
        int emptySlotIndex = -1;

        // 2. Идем по слотам оружия (всего 2 слота: 0 и 1)
        for (int i = 0; i < 2; i++)
        {
            WeaponData weaponInSlot = weaponController.GetWeaponInSlot(i);

            if (weaponInSlot != null)
            {
                // ПРАВИЛО 1: Если такое оружие уже есть — повышаем его редкость
                if (weaponInSlot == weaponToCraft)
                {
                    // Получаем текущую редкость из контроллера коллеги
                    WeaponRarity currentRarity = weaponController.GetRarityInSlot(i);

                    if (currentRarity < WeaponRarity.Legendary)
                    {
                        WeaponRarity nextRarity = (WeaponRarity)((int)currentRarity + 1);
                        weaponController.SetWeapon(i, weaponToCraft, nextRarity);

                        Debug.Log($"[Крафт] {weaponToCraft.weaponName} уже был у игрока! Качество повышено до {nextRarity}");
                        upgradedExisting = true;
                        break;
                    }
                    else
                    {
                        Debug.Log($"[Крафт] {weaponToCraft.weaponName} уже имеет максимальный уровень (Legendary)!");
                        // В таком случае логика пойдет дальше и скрафтит дубликат Common в свободный слот/на землю
                    }
                }
            }
            else
            {
                // Запоминаем индекс первого пустого слота, если он есть
                if (emptySlotIndex == -1)
                {
                    emptySlotIndex = i;
                }
            }
        }

        // Если мы успешно улучшили существующее оружие, завершаем метод
        if (upgradedExisting) return;

        // ПРАВИЛО 2: Если пушки не было, но есть свободное место (в руках или кармане)
        if (emptySlotIndex != -1)
        {
            // Добавляем новое оружие базовой редкости (Common) в свободный слот
            weaponController.SetWeapon(emptySlotIndex, weaponToCraft, WeaponRarity.Common);
            Debug.Log($"[Крафт] Скрафчен новый {weaponToCraft.weaponName} и добавлен в слот {emptySlotIndex}");
        }
        else
        {
            // ПРАВИЛО 3: Свободных мест нет — пушка падает на землю
            SpawnWeaponPickup(weaponToCraft, WeaponRarity.Common);
            Debug.Log($"[Крафт] Нет свободных мест! Скрафченный {weaponToCraft.weaponName} упал на землю");
        }
    }
}