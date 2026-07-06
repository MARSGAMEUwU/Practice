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
    [Header("параметры стрельбы")]
    [SerializeField] private float attackRate = 5f;
    [SerializeField] private float attackDamage = 5f;
    [SerializeField] private float aimSpeed = 5f;
    [SerializeField] private int shotsPerBurst = 5;
    [SerializeField] private float cooldown = 1f;
    [Header("Мансование (Strafing)")]
    [SerializeField] private float strafeDistance = 3f; // На какое расстояние вбок пытаемся шагнуть
    [SerializeField] private float strafeChangeTime = 2f; // Как часто враг меняет направление (влево/вправо)

    private float nextStrafeTime;
    private int currentStrafeDirection = 1; // 1 = вправо, -1 = влево

    private Adrenaline playerAdrenaline;
    private float nextFireTime;
    private float nextFireBurst;
    private bool isFiring;

    protected override void Awake()
    {
        base.Awake(); // Обязательно вызываем Awake из Damageable, чтобы здоровье установилось
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        // Ищем игрока по тегу, как это сделано в Enemy.cs
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
        Vector3 direction = (player.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * aimSpeed);
        }
    }

    public void Attack()
    {
        // Стреляем только если: вышло время кулдауна И мы сейчас НЕ стреляем другую очередь
        if (Time.time >= nextFireTime && !isFiring)
        {
            StartCoroutine(BurstFireCoroutine());
        }
    }

    // Корутина для поочередного выпускания пуль
    private System.Collections.IEnumerator BurstFireCoroutine()
    {
        isFiring = true; // Занято, штурмовик начал стрелять очередь

        for (int i = 0; i < shotsPerBurst; i++)
        {
            // Проверяем на всякий случай, не умер ли штурмовик посреди очереди
            if (isDead) yield break;

            // 1. Включаем анимацию (если нужно на каждый выстрел)
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }

            // 2. Включаем лазер и спавним пулю
            if (laserLine != null) laserLine.enabled = true;

            if (projectilePrefab != null && firePoint != null)
            {
                GameObject projObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
                Projectile proj = projObj.GetComponent<Projectile>();
                if (proj != null)
                {
                    proj.Initialize(
                        damage: attackDamage,
                        speed: 50f,
                        lifetime: 5f,
                        direction: (player.position - firePoint.position).normalized,
                        tracerPrefab: null,
                        owner: gameObject  // Владелец = Stormtrooper
                    );
                }
            }

            // 3. Выключаем лазер чуть позже
            StartCoroutine(ResetLaserColorRoutine());

            // 4. Ждем микро-паузу перед следующей пулей в этой же очереди
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

        // Поворачиваемся к игроку всегда, чтобы знать, куда пускать луч
        Aim();

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // === НАЧАЛО БЛОКА ПРОВЕРКИ ВИДИМОСТИ ===
        bool canSeePlayer = false;

        // Пускаем луч от уровня "глаз" штурмовика к уровню "груди" игрока (чуть выше пола)
        Vector3 rayOrigin = firePoint != null ? firePoint.position : transform.position + Vector3.up * 1f;
        Vector3 rayTarget = player.position + Vector3.up * 1f;
        Vector3 rayDirection = (rayTarget - rayOrigin).normalized;

        // Стреляем лучом на расстояние maxDistance
        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, maxDistance))
        {
            // Если первый объект, в который врезался луч — это игрок
            if (hit.collider.CompareTag("Player"))
            {
                canSeePlayer = true;
            }
        }
        // === КОНЕЦ БЛОКА ПРОВЕРКИ ВИДИМОСТИ ===

        // Логика поведения на основе видимости
        if (canSeePlayer)
        {
            Strafe();
            // 1. ИГРОКА ВИДНО: Стреляем и контролируем дистанцию
            if (Time.time >= nextFireBurst)
                Attack();

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
                // Идеальная дистанция: стоим и стреляем
                if (agent.isOnNavMesh) agent.ResetPath();
            }
        }
        else
        {
            // 2. ИГРОК ЗА СТЕНОЙ: Не стреляем, а просто бежим за ним, огибая углы
            ChasePlayer();
        }
    }

    private void ChasePlayer()
    {
        if (agent.isOnNavMesh)
            agent.SetDestination(player.position);
    }

    private void RunFromPlayer()
    {
        if (!agent.isOnNavMesh) { return; }
            Vector3 directionAwayFromPlayer = transform.position - player.position;
        Vector3 runToPosition = transform.position + directionAwayFromPlayer.normalized * minDistance;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(runToPosition, out hit, minDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        Strafe();
    }

    private void Strafe()
    {
        if (!agent.isOnNavMesh) return;

        // 1. Проверяем, не пора ли сменить направление
        if (Time.time >= nextStrafeTime)
        {
            // Случайно выбираем: 1 (вправо) или -1 (влево)
            currentStrafeDirection = Random.value > 0.5f ? 1 : -1;

            // Задаем время следующей смены направления (можно добавить легкий рандом, чтобы враг был менее предсказуемым)
            nextStrafeTime = Time.time + strafeChangeTime + Random.Range(-0.5f, 0.5f);
        }

        // 2. Узнаем, где у нас "вперед" и "вверх"
        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        // 3. Магия Cross Product: получаем вектор, смотрящий ровно вправо от игрока
        Vector3 rightDirection = Vector3.Cross(directionToPlayer, Vector3.up);

        // 4. Умножаем на наш currentStrafeDirection (чтобы идти вправо или влево)
        Vector3 strafeVector = rightDirection * currentStrafeDirection;

        // 5. Вычисляем точку назначения в паре метров сбоку от штурмовика
        Vector3 targetPosition = transform.position + strafeVector * strafeDistance;

        // 6. Проверяем, есть ли в этой точке пол (NavMesh), чтобы не приказать ему идти в стену
        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, strafeDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    protected override void Die()
    {
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
            // === НОВОЕ: Отключаем слой атаки (индекс 1), чтобы труп падал естественно ===
            animator.SetLayerWeight(1, 0f);
            //animator.SetTrigger(deathTrigger);
        }

        playerAdrenaline.KillReward();

        //Invoke(nameof(SpawnCorpse), 2f);
        //Destroy(gameObject, 2f);
    }
}
