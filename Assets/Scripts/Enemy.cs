using System;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Damageable
{
    [Header("Настройки ИИ")]
    public float attackRange = 2f;
    public float attackDamage = 10f;
    public float attackRate = 1.5f;

    [Header("Трупы")]
    public GameObject upperCorpsePrefab;   // префаб верхнего трупа (с LootEnemy)
    public GameObject lowerCorpsePrefab;   // префаб нижнего трупа (просто модель)

    private Transform player;
    private NavMeshAgent agent;
    private float nextAttackTime = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null) player = playerObject.transform;
        else Debug.LogError("Внимание! На сцене не найден объект с тегом 'Player'!");
    }

    void Update()
    {
        if (isDead || player == null) return;

        agent.SetDestination(player.position);
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackRate;
            Attack();
        }
    }

    void Attack()
    {
        Debug.Log("Враг нанес урон игроку!");
        // Здесь будет вызов урона игроку
    }

    // Переопределяем смерть
    protected override void Die()
    {
        // 1. Спавним верхний труп (на 73 выше)
        Vector3 upperPos = transform.position + Vector3.up * 72.9f;
        GameObject upper = Instantiate(upperCorpsePrefab, upperPos, transform.rotation);

        // Добавляем LootEnemy (если ещё нет)
        LootEnemy loot = upper.GetComponent<LootEnemy>();
        if (loot == null) loot = upper.AddComponent<LootEnemy>();

        // Настраиваем MeshCollider как триггер
        MeshCollider mc = upper.GetComponent<MeshCollider>();
        if (mc == null) mc = upper.AddComponent<MeshCollider>();
        mc.isTrigger = true;

        // 2. Спавним нижний труп (на месте врага)
        Vector3 lowerPos = transform.position;
        GameObject lower = Instantiate(lowerCorpsePrefab, lowerPos, transform.rotation);

        // 3. Отключаем врага (не уничтожаем, чтобы не сломать ссылки)
        gameObject.SetActive(false);
    }
}