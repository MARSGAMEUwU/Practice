using System.Threading.Tasks;
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
    [Header("параметры стрельбы")]
    [SerializeField] private float attackRate = 3f;
    [SerializeField] private float attackDamage = 30f;
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

    private void UpdateLaser()
    {
        if (laserLine == null) return;

        Vector3 origin = firePoint != null ? firePoint.position : transform.position;
        Vector3 targetPos = player.position + Vector3.up * 0.7f;
        Vector3 direction = (targetPos - origin).normalized;

        // Первая точка лазера (на стволе снайпера)
        laserLine.SetPosition(0, origin);

        // Пускаем луч, чтобы понять, где лазер должен оборваться
        if (Physics.Raycast(origin, direction, out RaycastHit hit, Mathf.Infinity))
        {
            // Если луч во что-то уперся (в стену или игрока), обрываем лазер в точке касания
            laserLine.SetPosition(1, hit.point);
            //if (hit.collider.CompareTag("Player"))
            //{
            //    nextFireTime += attackRate;
            //}
        }
        else
        {
            // Если луч улетел в небо, рисуем его очень длинным
            laserLine.SetPosition(1, origin + direction * 100f);
        }
    }

    private void Aim()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * aimSpeed);
        }
        UpdateLaser();
    }

    public void Attack()
    {
        nextFireTime = Time.time + attackRate;
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        laserLine.material.color = Color.yellow;
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
                Debug.Log($"sniper hit {attackDamage} hp");
            }
            //else Debug.Log("sniper hit wall");
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
            laserLine.material.color = Color.red;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (isDead) return;
        if (player == null) return;
        Aim();
        if (Time.time >= nextFireTime)
            Attack();
    }

    protected override void Die()
    {
        Debug.Log($"<color=orange>{gameObject.name} убит!</color>");
        //if (animator != null) animator.SetTrigger("Die");
        if (rb != null) rb.useGravity = true;
        laserLine.SetPosition(0, Vector3.zero);
    }
}
