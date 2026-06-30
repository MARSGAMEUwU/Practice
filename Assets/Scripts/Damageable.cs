using UnityEngine;

public class Damageable : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    public float currentHealth;
    public bool isDead = false;
    public GameObject prefab;

    private void Awake() => currentHealth = maxHealth;

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} получил {damage:F1} урона. HP: {currentHealth:F1}/{maxHealth}");

        if (currentHealth <= 0)
        {
            isDead = true;
            Die();
        }
    }

    public bool IsDead() => isDead;

    // “еперь виртуальный, чтобы Enemy мог переопределить
    protected virtual void Die()
    {
        Vector3 spawnPosition = transform.position + new Vector3(0f, 72.95f, 0f);
        Debug.Log($"{gameObject.name} уничтожен!");
        if (prefab != null)
        {
            Instantiate(prefab, spawnPosition, transform.rotation);
        }
    }
}