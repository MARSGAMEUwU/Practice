using System;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Damageable
{
    
    [Header("Настройки ИИ")]
    public float attackRange = 2f;          // Дистанция атаки (насколько близко подойти)
    public float attackDamage = 10f;        // Урон, который враг наносит игроку
    public float attackRate = 1.5f;         // Задержка между атаками (в секундах)

    private Transform player;               // Ссылка на трансформ игрока
    private NavMeshAgent agent;             // Компонент "умного" движения
    private float nextAttackTime = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Находим игрока на сцене по тегу (помните про важность тегов?)
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Внимание! На сцене не найден объект с тегом 'Player'!");
        }
    }

    void Update()
    {
        Debug.Log(isDead);
        // Если враг мертв или игрок пропал — ничего не делаем
        if (isDead || player == null) return;

        // Приказываем ИИ бежать строго к координатам игрока
        agent.SetDestination(player.position);

        // Считаем расстояние до игрока
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Если подошли вплотную и пришло время кусать/бить
        if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackRate;
            Attack();
        }
    }

    void Attack()
    {
        Debug.Log("Враг нанес урон игроку!");

        // Сюда вы вставите скрипт урона вашего игрока, например:
        // if (player.TryGetComponent<PlayerHealth>(out var playerHealth)) {
        //     playerHealth.TakeDamage(attackDamage);
        // }
    }

    // Переопределяем метод смерти. 
    // (Я пишу 'public override void Die()', предполагая, что ваш коллега сделал этот метод виртуальным 'virtual' в классе Damageable)
    //public override void Die()
    //{
    //    if (isDead) return; // Чтобы код не сработал дважды
    //    isDead = true;

    //    Debug.Log($"Враг {gameObject.name} уничтожен!");

    //    // Отключаем ИИ-навигацию, чтобы труп не скользил за игроком
    //    if (agent != null)
    //    {
    //        agent.enabled = false;
    //    }

    //    // Выключаем коллайдер, чтобы мертвый враг не мешал ходить и в него нельзя было стрелять
    //    if (TryGetComponent<Collider>(out var col))
    //    {
    //        col.enabled = false;
    //    }

    //    // Уничтожаем объект врага через 1 секунду (чтобы успела проиграться анимация или эффект)
    //    Destroy(gameObject, 1f);
    //}
}



