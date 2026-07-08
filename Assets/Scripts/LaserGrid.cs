using UnityEngine;

public class LaserGrid : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private BOSS bossController;
    [SerializeField] private Transform[] laserOrigins; // Перетащи сюда все пустышки LaserPoint_1, 2, 3...
    [SerializeField] private LineRenderer[] lineRenderers; // Перетащи сюда их LineRenderer'ы

    [Header("Настройки урона")]
    [SerializeField] private float maxLaserDistance = 50f;
    [SerializeField] private float laserDamage = 15f;
    [SerializeField] private float damageCooldown = 0.5f; // Как часто лазер может бить игрока (в секундах)

    private float nextDamageTime;
    private Adrenaline playerAdrenaline;

    private void Update()
    {
        // Если босс мертв или лазеры выключены — прячем лучи
        if (bossController == null || !bossController.areLasersActive)
        {
            ToggleLasers(false);
            return;
        }

        ToggleLasers(true);
        bool playerHitThisFrame = false;

        // Просчитываем каждый лазер на стойке
        for (int i = 0; i < laserOrigins.Length; i++)
        {
            Vector3 origin = laserOrigins[i].position;
            Vector3 direction = laserOrigins[i].forward; // Луч бьет прямо от точки

            lineRenderers[i].SetPosition(0, origin);

            if (Physics.Raycast(origin, direction, out RaycastHit hit, maxLaserDistance))
            {
                // Луч во что-то врезался (в стену или игрока)
                lineRenderers[i].SetPosition(1, hit.point);

                if (hit.collider.CompareTag("Player"))
                {
                    playerHitThisFrame = true;
                    // Кешируем скрипт игрока, чтобы не искать его каждый кадр
                    if (playerAdrenaline == null)
                        playerAdrenaline = hit.collider.GetComponent<Adrenaline>();
                }
            }
            else
            {
                // Луч улетел в пустоту
                lineRenderers[i].SetPosition(1, origin + direction * maxLaserDistance);
            }
        }

        // === СИСТЕМА НЕСТАКАЮЩЕГОСЯ УРОНА ===
        // Если хотя бы один луч коснулся игрока, и кулдаун прошел
        if (playerHitThisFrame && Time.time >= nextDamageTime && playerAdrenaline != null)
        {
            playerAdrenaline.TakeDamage(laserDamage);
            nextDamageTime = Time.time + damageCooldown; // Игрок получает "иммунитет" к лазерам на 0.5 сек
        }
    }

    private void ToggleLasers(bool state)
    {
        foreach (var lr in lineRenderers)
        {
            if (lr.enabled != state) lr.enabled = state;
        }
    }
}
