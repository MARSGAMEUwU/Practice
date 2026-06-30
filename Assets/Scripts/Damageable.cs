using UnityEngine;

public class Damageable : MonoBehaviour
{
    [SerializeField] protected float maxHealth = 100f;
    public float currentHealth;
    public bool isDead = false;

    protected virtual void Awake() => currentHealth = maxHealth;

    public virtual void TakeDamage(float damage)
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

    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} уничтожен!");
        Destroy(gameObject);
    }
}