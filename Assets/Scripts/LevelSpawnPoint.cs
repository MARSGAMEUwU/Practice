using UnityEngine;

public class LevelSpawnPoint : MonoBehaviour
{
    private void Start()
    {
        // Ищем нашего "бессмертного" игрока на сцене
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            // Телепортируем игрока в точку спавна
            player.transform.position = transform.position;
            player.transform.rotation = transform.rotation;

            // Сбрасываем скорость, если у игрока есть физика (чтобы он не инерциально летел дальше)
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = Vector3.zero;

            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.SimpleMove(Vector3.zero); // Сброс для CharacterController
        }
        else
        {
            Debug.LogError("Игрок не найден при загрузке уровня! Убедитесь, что тег Player задан.");
        }
    }
}