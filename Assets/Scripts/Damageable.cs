using UnityEngine;

public class Damageable : MonoBehaviour
{
    [SerializeField] protected float maxHealth = 100f;
    public float currentHealth;
    public bool isDead = false;

    [Header("Звуки")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField][Range(0f, 1f)] private float hitVolume = 0.7f;
    [SerializeField][Range(0f, 1f)] private float deathVolume = 1f;

    protected AudioSource audioSource;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
        }
    }

    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} получил {damage:F1} урона. HP: {currentHealth:F1}/{maxHealth}");

        PlayHitSound();

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

        PlayDeathSound();

        Destroy(gameObject, 2f);
    }

    private void PlayHitSound()
    {
        if (audioSource == null || hitSound == null) return;
        audioSource.PlayOneShot(hitSound);
    }

    private void PlayDeathSound()
    {
        if (audioSource == null || deathSound == null) return;
        audioSource.PlayOneShot(deathSound);
    }
}