using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : Damageable
{
    [Header("Ссылки")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;

    [Header("Настройки преследования")]
    [SerializeField] private float detectionRange = 20f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float stopDistance = 1.2f;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Характеристики")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float moveSpeed = 3.5f;

    [Header("Анимации")]
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private string deathTrigger = "Die";

    [Header("Лут трупа")]
    [SerializeField] private GameObject corpsePrefab; // Префаб трупа
    [SerializeField] private float corpseSpawnHeight = 73f; // Смещение по Y

    private float nextAttackTime;
    private bool isAttacking;
    Adrenaline playerAdrenaline;

    protected override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = stopDistance;
        agent.speed = moveSpeed;
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            playerAdrenaline = playerObj.GetComponent<Adrenaline>();
            if (playerObj != null) player = playerObj.transform;
        }
    }

    private void Update()
    {
        if (isDead) return;
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            if (distanceToPlayer <= attackRange)
            {
                Attack();
                StopMovement();
            }
            else
            {
                ChasePlayer();
            }
        }
        else
        {
            StopMovement();
        }

        UpdateAnimator(distanceToPlayer);
    }

    private void ChasePlayer()
    {
        if (agent.isOnNavMesh && !isAttacking)
            agent.SetDestination(player.position);
    }

    private void StopMovement()
    {
        if (agent.isOnNavMesh) agent.ResetPath();
    }

    private void Attack()
    {
        if (Time.time >= nextAttackTime && !isAttacking)
        {
            isAttacking = true;
            nextAttackTime = Time.time + attackCooldown;
            if (animator != null) animator.SetTrigger(attackTrigger);
            Debug.Log($"<color=red>{gameObject.name} замахивается!</color>");

            Invoke(nameof(ResetAttack), 0.8f);

            // ВМЕСТО УРОНА ЗАПУСКАЕМ КОРУТИНУ:
            // Передаем туда время замаха (например, 0.4 секунды)
            StartCoroutine(DealDamageCoroutine(0.8f));
        }
    }

    // А ВОТ И САМА КОРУТИНА С УСЛОВИЕМ:
    private System.Collections.IEnumerator DealDamageCoroutine(float delay)
    {
        // 1. Ставим выполнение на паузу, пока идет анимация замаха
        yield return new WaitForSeconds(delay);

        // 2. ВРЕМЯ ВЫШЛО. Проверяем условие (не убежал ли игрок?)
        // Предполагается, что переменные player и attackRange у тебя уже есть в скрипте Enemy
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // Если игрок все еще находится в зоне поражения:
            if (isDead == false)
            {
                playerAdrenaline.TakeDamage(damage);
                Debug.Log($"<color=red>Удар достиг цели!</color>");
            }
            else
            {
                Debug.Log($"<color=yellow>Игрок успел увернуться!</color>");
            }
        }
    }

    private void ResetAttack() => isAttacking = false;

    private void UpdateAnimator(float distanceToPlayer)
    {
        if (animator == null) return;

        bool isMoving = distanceToPlayer > attackRange &&
                        distanceToPlayer <= detectionRange &&
                        !isAttacking;

        if (isMoving && agent.velocity.magnitude > 0.1f)
        {
            // Получаем направление движения в локальных координатах
            Vector3 localVelocity = transform.InverseTransformDirection(agent.velocity);

            // Нормализуем
            float inputX = Mathf.Clamp(localVelocity.x, -1f, 1f);
            float inputY = Mathf.Clamp(localVelocity.z, -1f, 1f);

            // Устанавливаем параметры
            animator.SetFloat("InputX", inputX);
            animator.SetFloat("InputY", inputY);
            animator.SetFloat("Speed", 1f);

            //Debug.Log($"InputX: {inputX:F2}, InputY: {inputY:F2}");
        }
        else
        {
            animator.SetFloat("Speed", 0f);
            animator.SetFloat("InputX", 0f);
            animator.SetFloat("InputY", 0f);
        }
    }

    // === ИЗМЕНЁННЫЙ МЕТОД СМЕРТИ ===
    protected override void Die()
    {
        Debug.Log($"<color=red>{gameObject.name} убит!</color>");

        // Останавливаем агента
        if (agent != null && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.enabled = false;
        }

        // Отключаем коллайдеры чтобы не мешал стрельбе
        foreach (var col in GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        // Запускаем анимацию смерти
        if (animator != null)
        {
            animator.applyRootMotion = true;
            animator.SetTrigger(deathTrigger);
        }

        // Награда за убийство
        playerAdrenaline.KillReward();

        // === НОВОЕ: спавним труп на месте смерти через время анимации ===
        Invoke(nameof(SpawnCorpse), 2f); // Через 2 секунды (длина анимации смерти)

        // === НОВОЕ: уничтожаем врага после анимации смерти ===
        Destroy(gameObject, 2f);
    }


    private void SpawnCorpse()
    {
        if (corpsePrefab == null)
        {
            Debug.LogWarning("Corpse prefab не назначен!");
            return;
        }

        // Смещение по Y (например, -0.5 чтобы труп лежал на земле)
        float yOffset = -0.5f;
        Vector3 spawnPos = transform.position + Vector3.up * yOffset;
        Quaternion spawnRot = transform.rotation;

        GameObject corpse = Instantiate(corpsePrefab, spawnPos, spawnRot);

        Debug.Log($"Труп заспавнен на месте смерти: {corpse.name} на позиции {spawnPos}");
    }

    public void OnAttackHit()
    {
        if (player == null) return;
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= attackRange)
        {
            Debug.Log($"💥 Удар достиг игрока! Урон: {damage}");
            playerAdrenaline.TakeDamage(damage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}