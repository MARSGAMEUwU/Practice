using UnityEngine;

public class LevelSpawnPoint : MonoBehaviour
{
    [Header("Настройки спавна")]
    [Tooltip("Смещение по Y вверх, чтобы игрок не застрял в полу и не провалился под карту")]
    [SerializeField] private float spawnHeightOffset = 0.5f;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * spawnHeightOffset;
            Quaternion spawnRot = transform.rotation;

            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                player.transform.position = spawnPos;
                player.transform.rotation = spawnRot;
                cc.enabled = true;
            }
            else
            {
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