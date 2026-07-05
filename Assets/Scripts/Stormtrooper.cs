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

    private Adrenaline playerAdrenaline;
    private float nextFireTime;

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
        nextFireTime = Time.time + attackRate;
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        laserLine.enabled = true;
        //Vector3 origin = firePoint != null ? firePoint.position : transform.position + Vector3.up;
        //Vector3 targetPos = player.position + Vector3.up * 0.5f;
        //Vector3 direction = (targetPos - origin).normalized;
        //Debug.DrawRay(origin, direction * 50f, Color.red, 2f);
        //if (Physics.Raycast(origin, direction, out RaycastHit hit, Mathf.Infinity))
        //{
        //    Debug.Log($"Луч попал в: {hit.collider.name}");
        //    if (hit.collider.CompareTag("Player"))
        //    {
        //        playerAdrenaline.TakeDamage(attackDamage);
        //        Debug.Log($"stormtrooper hit {attackDamage} hp");
        //    }
        //}

        Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        StartCoroutine(ResetLaserColorRoutine());
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

        Aim(); // Поворачиваемся всегда

        if (Time.time >= nextFireTime)
            Attack();

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Проверяем дистанцию и двигаемся
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
            // === НОВОЕ: Мы в идеальной зоне (20-50м). Стоим на месте! ===
            if (agent.isOnNavMesh)
            {
                agent.ResetPath(); // Сбрасываем маршрут
            }
        }

        // ЕСЛИ У ТЕБЯ APPLY ROOT MOTION = TRUE:
        // Сюда обязательно нужно вернуть метод UpdateAnimator(distanceToPlayer), 
        // как это было сделано в Enemy.cs, чтобы штурмовик перебирал ногами!
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
