using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : Damageable
{
    [Header("Ссылки")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;

    [Header("Префаб верхнего трупа")]
    [SerializeField] private GameObject upperCorpsePrefab; // Префаб трупа для лутинга
    [SerializeField] private float corpseSpawnOffset = 0.73f; // Смещение по Y (73 пункта = 0.73 метра)

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

    // Состояние
    private float nextAttackTime;
    private bool isAttacking;
    private GameObject spawnedCorpse; // Ссылка на спавненный труп

    protected override void Awake()
    {
        base.Awake();

        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = stopDistance;
        agent.speed = moveSpeed;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
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
        {
            agent.SetDestination(player.position);
        }
    }

    private void StopMovement()
    {
        if (agent.isOnNavMesh)
        {
            agent.ResetPath();
        }
    }

    private void Attack()
    {
        if (Time.time >= nextAttackTime && !isAttacking)
        {
            isAttacking = true;
            nextAttackTime = Time.time + attackCooldown;

            if (animator != null)
            {
                animator.SetTrigger(attackTrigger);
            }

            Debug.Log($"<color=red>{gameObject.name} атакует игрока!</color>");

            Invoke(nameof(ResetAttack), 0.8f);
        }
    }

    private void ResetAttack()
    {
        isAttacking = false;
    }

    private void UpdateAnimator(float distanceToPlayer)
    {
        if (animator == null) return;

        bool isMoving = distanceToPlayer > attackRange &&
                        distanceToPlayer <= detectionRange &&
                        !isAttacking;

        if (isMoving && agent.velocity.magnitude > 0.1f)
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

    protected override void Die()
    {
        Debug.Log($"<color=red>{gameObject.name} убит!</color>");

        // Останавливаем агента
        if (agent != null && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.enabled = false;
        }

        // Запускаем анимацию смерти
        if (animator != null)
        {
            animator.applyRootMotion = true;
            animator.SetTrigger(deathTrigger);
        }

        // Отключаем коллайдеры чтобы не мешал
        foreach (var col in GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        // === НОВОЕ: спавним верхний труп для лутинга ===
        SpawnCorpseForLooting();

        // === ИЗМЕНЕНО: НЕ уничтожаем врага, он остаётся как нижний труп ===
        // Destroy(gameObject, 3f); // УДАЛЕНО

        // Отключаем скрипт чтобы не обновлялся
        enabled = false;
    }

    private void SpawnCorpseForLooting()
    {
        if (upperCorpsePrefab == null)
        {
            Debug.LogWarning("upperCorpsePrefab не назначен!");
            return;
        }

        // Спавним труп на +0.73 по Y
        Vector3 spawnPosition = transform.position + Vector3.up * corpseSpawnOffset;
        spawnedCorpse = Instantiate(upperCorpsePrefab, spawnPosition, transform.rotation);

        Debug.Log($"Спавнен верхний труп на позиции {spawnPosition}");
    }

    public void OnAttackHit()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= attackRange)
        {
            Debug.Log($" Удар достиг игрока! Урон: {damage}");
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