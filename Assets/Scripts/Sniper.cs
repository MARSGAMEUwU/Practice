using System.Collections;
using UnityEngine;

public class Sniper : Damageable
{
    [Header("Ссылки")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform firePoint;
    [SerializeField] private LineRenderer laserLine;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private ParticleSystem MuzzleEffect;

    [Header("Параметры стрельбы")]
    [SerializeField] private float attackRate = 3f;
    [SerializeField] private float attackDamage = 30f;
    [SerializeField] private float aimSpeed = 5f;
    [SerializeField] private AudioClip shotSound;
    [SerializeField] private float lockDelay = 1.0f;
    [SerializeField] private float maxSightDistance = 100f;

    [Header("Настройки прицеливания")]
    [SerializeField] private float aimTargetHeight = 0.2f;

    private Adrenaline playerAdrenaline;
    private float nextFireTime;
    private bool wasSeeingPlayer = false;

    // Новая логика таймера (без плавности)
    private float lockTimer = 0f;
    private bool isLockedOn = false;

    protected override void Awake()
    {
        base.Awake();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerAdrenaline = playerObj.GetComponent<Adrenaline>();
            }
        }
        else if (playerAdrenaline == null)
        {
            playerAdrenaline = player.GetComponent<Adrenaline>();
        }

        if (laserLine == null) laserLine = GetComponent<LineRenderer>();
        if (laserLine != null) laserLine.positionCount = 2;
    }

    private void UpdateLaser()
    {
        if (laserLine == null || player == null) return;

        Vector3 origin = firePoint != null ? firePoint.position : transform.position;
        Vector3 targetPos = player.position + Vector3.up * aimTargetHeight;
        Vector3 direction = (targetPos - origin).normalized;

        laserLine.SetPosition(0, origin);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, Mathf.Infinity))
        {
            laserLine.SetPosition(1, hit.point);
        }
        else
        {
            laserLine.SetPosition(1, origin + direction * 100f);
        }
    }

    private void Aim()
    {
        if (player == null) return;
        Vector3 targetPos = player.position + Vector3.up * aimTargetHeight;
        Vector3 direction = (targetPos - transform.position).normalized;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * aimSpeed);
        }
        UpdateLaser();
    }

    public void Attack()
    {
        nextFireTime = Time.time + attackRate;

        if (animator != null) animator.SetTrigger("Attack");

        if (audioSource != null && shotSound != null)
        {
            audioSource.PlayOneShot(shotSound);
        }

        if (laserLine != null && laserLine.material != null)
        {
            laserLine.material.color = Color.yellow;
        }

        Vector3 origin = firePoint != null ? firePoint.position : transform.position + Vector3.up;
        Vector3 targetPos = player.position + Vector3.up * aimTargetHeight;
        Vector3 direction = (targetPos - origin).normalized;

        Debug.DrawRay(origin, direction * 100f, Color.red, 2f);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, Mathf.Infinity))
        {
            if (hit.collider.CompareTag("Player") || hit.collider.GetComponentInParent<Adrenaline>() != null)
            {
                if (playerAdrenaline != null)
                {
                    playerAdrenaline.TakeDamage(attackDamage);
                }
            }
        }

        StartCoroutine(ResetLaserColorRoutine());
    }

    private IEnumerator ResetLaserColorRoutine()
    {
        yield return new WaitForSeconds(0.15f);
        if (laserLine != null && laserLine.material != null)
        {
            laserLine.material.color = Color.red;
        }
    }

    private void Update()
    {
        if (isDead) return;
        if (player == null) return;

        Aim();

        bool canSeePlayer = false;

        Vector3 rayOrigin = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.5f;
        Vector3 rayTarget = player.position + Vector3.up * aimTargetHeight;
        Vector3 rayDirection = (rayTarget - rayOrigin).normalized;
        float distanceToPlayer = Vector3.Distance(rayOrigin, rayTarget);

        float rayDistance = Mathf.Min(distanceToPlayer, maxSightDistance);

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, rayDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.CompareTag("Player") || hit.collider.GetComponentInParent<Adrenaline>() != null)
            {
                canSeePlayer = true;
            }
        }

        Debug.DrawRay(rayOrigin, rayDirection * distanceToPlayer, canSeePlayer ? Color.green : Color.red);

        // === ЛОГИКА ЖЕСТКОГО СБРОСА ТАЙМЕРА ===
        if (canSeePlayer)
        {
            // Если игрок только что появился в зоне видимости (был скрыт, стал виден)
            if (!wasSeeingPlayer)
            {
                lockTimer = 0f;       // Обнуляем таймер
                isLockedOn = false;   // Сбрасываем флаг захвата
                if (laserLine != null && laserLine.material != null) laserLine.material.color = new Color(1f, 0.5f, 0f); // Оранжевый
            }

            // Если еще не захвачены, считаем время
            if (!isLockedOn)
            {
                lockTimer += Time.deltaTime;
                if (lockTimer >= lockDelay)
                {
                    isLockedOn = true;
                    Debug.Log($"<color=yellow>[Sniper Debug]</color> Цель ЗАХВАЧЕНА!");
                }
            }
            else
            {
                // Если захвачены, проверяем кулдаун атаки (attackRate)
                if (Time.time >= nextFireTime)
                {
                    Attack();
                }
                else
                {
                    // Во время перезарядки лазер оранжевый
                    if (laserLine != null && laserLine.material != null) laserLine.material.color = new Color(1f, 0.5f, 0f);
                }
            }
        }
        else
        {
            // Если игрок скрылся, жестко обнуляем таймер и сброс
            if (wasSeeingPlayer)
            {
                Debug.Log($"<color=yellow>[Sniper Debug]</color> Игрок скрылся. Захват СБРОШЕН.");
            }

            lockTimer = 0f;
            isLockedOn = false;

            if (laserLine != null && laserLine.material != null)
            {
                laserLine.material.color = Color.red;
            }
        }

        wasSeeingPlayer = canSeePlayer;
    }

    protected override void Die()
    {
        if (rb != null) rb.useGravity = true;
        if (laserLine != null) laserLine.enabled = false;
    }
}