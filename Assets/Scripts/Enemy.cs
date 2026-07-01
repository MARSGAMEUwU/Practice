using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : Damageable
{
    [Header("Ссылки")]
    [SerializeField] private Transform player; // Ссылка на игрока
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;

    [Header("Настройки преследования")]
    [SerializeField] private float detectionRange = 20f; // Дальность обнаружения
    [SerializeField] private float attackRange = 1.5f; // Дистанция атаки
    [SerializeField] private float stopDistance = 1.2f; // Останавливаться на этом расстоянии
    [SerializeField] private float attackCooldown = 1.5f; // Пауза между атаками

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

    protected override void Awake()
    {
        base.Awake();

        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = stopDistance;
        agent.speed = moveSpeed;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        // Автоматический поиск игрока по тегу
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

        // Проверяем, в зоне ли обнаружения игрок
        if (distanceToPlayer <= detectionRange)
        {
            // Если достаточно близко для атаки
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

            // Запускаем анимацию атаки
            if (animator != null)
            {
                animator.SetTrigger(attackTrigger);
            }

            // Здесь будет урон игроку (пока заглушка)
            Debug.Log($"<color=red>{gameObject.name} атакует игрока!</color>");

            // Сбрасываем флаг атаки через время (должно совпадать с длиной анимации)
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
            // Получаем направление движения в локальных координатах
            Vector3 localVelocity = transform.InverseTransformDirection(agent.velocity);

            // Нормализуем
            float inputX = Mathf.Clamp(localVelocity.x, -1f, 1f);
            float inputY = Mathf.Clamp(localVelocity.z, -1f, 1f);

            // Устанавливаем параметры
            animator.SetFloat("InputX", inputX);
            animator.SetFloat("InputY", inputY);
            animator.SetFloat("Speed", 1f);

            Debug.Log($"InputX: {inputX:F2}, InputY: {inputY:F2}");
        }
        else
        {
            animator.SetFloat("Speed", 0f);
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
            animator.SetTrigger(deathTrigger);
        }

        // Отключаем коллайдеры чтобы не мешал
        foreach (var col in GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        // Уничтожаем после завершения анимации смерти
        Destroy(gameObject, 3f);
    }

    // === Метод для события анимации (Animation Event) ===
    // Можно вызвать из анимации атаки в момент удара
    public void OnAttackHit()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= attackRange)
        {
            // Здесь будет урон игроку
            Debug.Log($"Удар достиг игрока! Урон: {damage}");

            // Когда добавите TakeDamage игроку:
            // var playerDamageable = player.GetComponent<Damageable>();
            // if (playerDamageable != null) playerDamageable.TakeDamage(damage);
        }
    }

    // Визуализация в редакторе
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}