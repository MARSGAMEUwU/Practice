using UnityEngine;

public class LevelSpawnPoint : MonoBehaviour
{
    [Header("Настройки спавна")]
    [Tooltip("Смещение по Y вверх, чтобы игрок не застрял в полу и не провалился под карту")]
    [SerializeField] private float spawnHeightOffset = 0.5f;

    private void Start()
    {
        // Ищем нашего "бессмертного" игрока на сцене
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            // 1. Вычисляем глобальную позицию с небольшим смещением вверх
            Vector3 spawnPos = transform.position + Vector3.up * spawnHeightOffset;
            Quaternion spawnRot = transform.rotation;

            // 2. Полностью сбрасываем физику (Rigidbody), чтобы инерция не унесла игрока
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // 3. Правильно телепортируем CharacterController
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null)
            {
                // CharacterController глючит, если просто менять transform.position при коллизии.
                // Стандартный фикс Unity: отключить контроллер, переместить объект, включить обратно.
                cc.enabled = false;
                player.transform.position = spawnPos;
                player.transform.rotation = spawnRot;
                cc.enabled = true;
            }
            else
            {
                // Если CharacterController нет, просто меняем позицию
                player.transform.position = spawnPos;
                player.transform.rotation = spawnRot;
            }

            Debug.Log($"<color=green>Игрок телепортирован в точку спавна: {spawnPos}</color>");
        }
        else
        {
            Debug.LogError("Игрок не найден при загрузке уровня! Убедитесь, что тег Player задан.");
        }
    }
}