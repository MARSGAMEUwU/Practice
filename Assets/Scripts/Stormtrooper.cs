using UnityEngine;
using UnityEngine.AI;

public class Stormtrooper : Damageable
{
    [Header("Ссылки")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform firePoint;
    [SerializeField] private LineRenderer laserLine;
    [SerializeField] private ParticleSystem MuzzleEffect;
    [SerializeField] private NavMeshAgent agent;
    [Header("Настройки преследования")]
    [SerializeField] private float minDistance = 20f;
    [SerializeField] private float maxDistance = 50f;
    [Header("параметры стрельбы")]
    [SerializeField] private float attackRate = 0.5f;
    [SerializeField] private float attackDamage = 5f;
    [SerializeField] private float aimSpeed = 5f;

    private Adrenaline playerAdrenaline;
    private float nextFireTime;

    protected override void Awake()
    {
        base.Awake(); // Обязательно вызываем Awake из Damageable, чтобы здоровье установилось

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
        Vector3 origin = firePoint != null ? firePoint.position : transform.position + Vector3.up;
        Vector3 targetPos = player.position + Vector3.up * 0.5f;
        Vector3 direction = (targetPos - origin).normalized;
        Debug.DrawRay(origin, direction * 50f, Color.red, 2f);
        if (Physics.Raycast(origin, direction, out RaycastHit hit, Mathf.Infinity))
        {
            Debug.Log($"Луч попал в: {hit.collider.name}");
            if (hit.collider.CompareTag("Player"))
            {
                playerAdrenaline.TakeDamage(attackDamage);
                Debug.Log($"stormtrooper hit {attackDamage} hp");
            }
        }

        StartCoroutine(ResetLaserColorRoutine());
    }

    private System.Collections.IEnumerator ResetLaserColorRoutine()
    {
        // Ждём 0.15 секунд (можешь поменять время, чтобы выстрел казался длиннее или короче)
        yield return new WaitForSeconds(0.15f);

        // Возвращаем лазеру стандартный красный прицел
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
        if (Time.time >= nextFireTime)
            Attack();
    }
}
