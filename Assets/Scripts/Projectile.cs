using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 100f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private GameObject impactEffectPrefab;
    private Adrenaline playerAdrenaline;

    void Start()
    {
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifetime);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            Transform player = playerObj.transform;
            playerAdrenaline = playerObj.GetComponent<Adrenaline>();
            
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 1. Игнорируем другие триггеры (чтобы пуля не взрывалась в воздухе от триггеров зоны видимости врагов)
        if (other.isTrigger) return;

        // 2. Проверяем, попали ли в игрока
        if (other.CompareTag("Player"))
        {
            if (playerAdrenaline != null)
            {
                playerAdrenaline.TakeDamage(damage);
            }
        }
        // Если в твоем будущем оружии пуля будет лететь от ИГРОКА ВО ВРАГА, то добавь проверку:
        // else if (other.TryGetComponent<Damageable>(out Damageable enemy)) { enemy.TakeDamage(damage); }

        // 3. Эффект попадания должен создаваться ВСЕГДА (и об стену, и об игрока)
        if (impactEffectPrefab != null)
        {
            Instantiate(impactEffectPrefab, transform.position, transform.rotation);
        }

        // 4. Уничтожаем пулю при любом физическом столкновении
        Destroy(gameObject);
    }
}
