using UnityEngine;

public class Damageable : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false;

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

    private void Die()
    {
        Debug.Log($"{gameObject.name} уничтожен!");
        Destroy(gameObject);
    }
}