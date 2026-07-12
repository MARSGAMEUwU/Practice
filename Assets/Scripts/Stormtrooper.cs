using UnityEngine;
using UnityEngine.AI;

public class Stormtrooper : Damageable
{
    [Header("Ссылки")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform firePoint;
    [SerializeField] private LineRenderer laserLine;
    [SerializeField] private ParticleSystem MuzzleEffect;
    [SerializeField] private NavMeshAgent agent;

    [Header("Настройки преследования")]
    [SerializeField] private float minDistance = 20f;
    [SerializeField] private float maxDistance = 50f;
    [SerializeField] private float moveSpeed = 4f;

    [Header("Параметры стрельбы")]
    [SerializeField] private float attackRate = 5f;
    [SerializeField] private float attackDamage = 5f;
    [SerializeField] private float aimSpeed = 5f;
    [SerializeField] private int shotsPerBurst = 5;
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private AudioClip shotSound;

    [Header("Маневрирование (Strafing)")]
    [SerializeField] private float strafeDistance = 3f;
    [SerializeField] private float strafeChangeTime = 2f;

    [Header("Анимации")]
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private string deathTrigger = "Die";

    [Header("Лут трупа")]
    [SerializeField] private GameObject corpsePrefab;
    [SerializeField] private float corpseSpawnHeight = 73f;

    private float nextStrafeTime;
    private int currentStrafeDirection = 1;
    private Adrenaline playerAdrenaline;
    private float nextFireTime;
    private float nextFireBurst;
    private bool isFiring;

    protected override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();

        agent.speed = moveSpeed;
        agent.acceleration = 8f;
        agent.angularSpeed = 120f;

        if (animator == null) animator = GetComponentInChildren<Animator>();

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

        if (laserLine != null) { laserLine = GetComponent<LineRenderer>(); laserLine.positionCount = 2; }
        agent.updateRotation = false;
    }

    private void Aim()
    {
        if (player == null || firePoint == null) return;

        Vector3 aimAxis = firePoint.right;

        aimAxis.y = 0f;
        if (aimAxis.sqrMagnitude < 0.001f) return;
        aimAxis.Normalize();

        Vector3 targetDir = player.position - transform.position;
        targetDir.y = 0f;
        if (targetDir.sqrMagnitude < 0.001f) return;
        targetDir.Normalize();

        Quaternion deltaRotation = Quaternion.FromToRotation(aimAxis, targetDir);

        Quaternion targetRotation = deltaRotation * transform.rotation;

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * aimSpeed);
    }

    public void Attack()
    {
        if (Time.time >= nextFireTime && !isFiring)
        {
            StartCoroutine(BurstFireCoroutine());
        }
    }

    private System.Collections.IEnumerator BurstFireCoroutine()
    {
        isFiring = true;
        for (int i = 0; i < shotsPerBurst; i++)
        {
            if (isDead) yield break;

            if (animator != null)
            {
                animator.SetTrigger(attackTrigger);
            }

            if (laserLine != null) laserLine.enabled = true;
            if (projectilePrefab != null && firePoint != null)
            {
                GameObject projObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
                Projectile proj = projObj.GetComponent<Projectile>();
                if (proj != null)
                {
                    if (audioSource != null && shotSound != null) audioSource.PlayOneShot(shotSound);
                    proj.Initialize(
                        damage: attackDamage,
                        speed: 25f,
                        lifetime: 10f,
                        direction: (player.position - firePoint.position).normalized,
                        tracerPrefab: null,
                        owner: gameObject
                    );
                }
            }

            StartCoroutine(ResetLaserColorRoutine());
            yield return new WaitForSeconds(attackRate);
        }
        nextFireTime = Time.time + cooldown;
        isFiring = false;
    }

    private System.Collections.IEnumerator ResetLaserColorRoutine()
    {
        yield return new WaitForSeconds(0.15f);
        if (laserLine != null)
        {
            laserLine.enabled = false;
        }
    }

    private void Update()
    {
        if (isDead) return;
        if (player == null) return;

        Aim();
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        bool canSeePlayer = false;
        Vector3 rayOrigin = firePoint != null ? firePoint.position : transform.position + Vector3.up * 1f;
        Vector3 rayTarget = player.position + Vector3.up * 1f;
        Vector3 rayDirection = (rayTarget - rayOrigin).normalized;

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, maxDistance))
        {
            if (hit.collider.CompareTag("Player"))
            {
                canSeePlayer = true;
            }
        }

        if (canSeePlayer)
        {
            if (Time.time >= nextFireBurst) Attack();

            if (distanceToPlayer > maxDistance)
            {
                ChasePlayer();
            }
            else if (distanceToPlayer < minDistance)
            {
                RunFromPlayer();
            }
            else
            {
                Strafe();
            }
        }
        else
        {
            ChasePlayer();
        }

        UpdateAnimator(distanceToPlayer);
    }

    private void UpdateAnimator(float distanceToPlayer)
    {
        if (animator == null) return;

        bool isMoving = distanceToPlayer <= maxDistance && agent.velocity.magnitude > 0.1f;

        if (isMoving)
        {
            Vector3 localVelocity = transform.InverseTransformDirection(agent.velocity);
            float inputX = Mathf.Clamp(localVelocity.x, -1f, 1f);
            float inputY = Mathf.Clamp(localVelocity.z, -1f, 1f);

            animator.SetFloat("InputX", inputX);
            animator.SetFloat("InputY", inputY);
            animator.SetFloat(speedParam, 1f);
        }
        else
        {
            animator.SetFloat(speedParam, 0f);
            animator.SetFloat("InputX", 0f);
            animator.SetFloat("InputY", 0f);
        }
    }

    private void ChasePlayer()
    {
        if (agent.isOnNavMesh) agent.SetDestination(player.position);
    }

    private void RunFromPlayer()
    {
        if (!agent.isOnNavMesh) { return; }
        Vector3 directionAwayFromPlayer = transform.position - player.position;
        Vector3 runToPosition = transform.position + directionAwayFromPlayer.normalized * minDistance;

        if (NavMesh.SamplePosition(runToPosition, out NavMeshHit hit, minDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        Strafe();
    }

    private void Strafe()
    {
        if (!agent.isOnNavMesh) return;

        if (Time.time >= nextStrafeTime)
        {
            currentStrafeDirection = Random.value > 0.5f ? 1 : -1;
            nextStrafeTime = Time.time + strafeChangeTime + Random.Range(-0.5f, 0.5f);
        }

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Vector3 rightDirection = Vector3.Cross(directionToPlayer, Vector3.up);
        Vector3 strafeVector = rightDirection * currentStrafeDirection;
        Vector3 targetPosition = transform.position + strafeVector * strafeDistance;

        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, strafeDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    protected override void Die()
    {
        if (WaveManager.Instance != null) WaveManager.Instance.OnEnemyDeath();
        Debug.Log($"<color=red>{gameObject.name} убит!</color>");
        if (agent != null && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.enabled = false;
        }
        foreach (var col in GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }
        if (animator != null)
        {
            animator.applyRootMotion = true;
            animator.SetLayerWeight(1, 0f);
            animator.SetTrigger(deathTrigger);
        }

        if (playerAdrenaline != null) playerAdrenaline.KillReward();

        Invoke(nameof(SpawnCorpse), 2f);
        Destroy(gameObject, 2f);
    }

    private void SpawnCorpse()
    {
        if (corpsePrefab == null)
        {
            Debug.LogWarning("Corpse prefab не назначен!");
            return;
        }
        float yOffset = 0.8f;
        Vector3 spawnPos = transform.position + Vector3.up * yOffset;
        Quaternion spawnRot = transform.rotation;
        GameObject corpse = Instantiate(corpsePrefab, spawnPos, spawnRot);
        Debug.Log($"Труп заспавнен на месте смерти: {corpse.name} на позиции {spawnPos}");
    }
}