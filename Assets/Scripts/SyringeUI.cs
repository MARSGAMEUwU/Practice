using UnityEngine;
using UnityEngine.UI;

public class SyringeUI : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private Adrenaline adrenaline;

    [Header("Иконки шприцев (максимум 4)")]
    [SerializeField] private Image[] syringeImages = new Image[4];

    private void Update()
    {
        // ПРОВЕРКА 1: Назначена ли ссылка на игрока?
        if (adrenaline == null)
        {
            Debug.LogError("[SyringeUI] ? ОШИБКА: Ссылка на Adrenaline НЕ назначена в инспекторе!");
            return;
        }

        // ПРОВЕРКА 2: Заполнен ли массив иконок?
        if (syringeImages == null || syringeImages.Length == 0)
        {
            Debug.LogError("[SyringeUI] ? ОШИБКА: Массив иконок пуст! Перетащите 4 Image в инспекторе.");
            return;
        }

        // Читаем значение
        int currentAmount = adrenaline.GetSyringeAmount();

        // ДЕБАГ: Пишем в консоль каждый кадр (потом можно будет убрать)
        Debug.Log($"[SyringeUI] ?? Обновление! Текущее кол-во шприцев: {currentAmount}");

        // Включаем/выключаем иконки
        for (int i = 0; i < syringeImages.Length; i++)
        {
            if (syringeImages[i] == null)
            {
                Debug.LogWarning($"[SyringeUI] ?? Иконка в слоте [{i}] не назначена в массиве!");
                continue;
            }

            if (i < currentAmount)
            {
                syringeImages[i].gameObject.SetActive(true);
            }
            else
            {
                syringeImages[i].gameObject.SetActive(false);
            }
        }
    }
}