using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float damage;
    private float speed;
    private float lifetime;
    private Vector3 direction;
    private GameObject tracerPrefab;
    private GameObject owner;

    private Rigidbody rb;
    private GameObject tracerInstance;

    public void Initialize(float damage, float speed, float lifetime, Vector3 direction, GameObject tracerPrefab, GameObject owner)
    {
        this.damage = damage;
        this.speed = speed;
        this.lifetime = lifetime;
        this.direction = direction.normalized;
        this.tracerPrefab = tracerPrefab;
        this.owner = owner;

        Setup();
    }

    private void Setup()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.linearVelocity = direction * speed;
        }

        if (tracerPrefab != null)
        {
            tracerInstance = Instantiate(tracerPrefab, transform.position, transform.rotation);
            tracerInstance.transform.SetParent(transform);
        }

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;

        if (owner != null && other.gameObject == owner) return;
        if (owner != null && other.transform.IsChildOf(owner.transform)) return;

        Damageable damageable = other.GetComponent<Damageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (tracerInstance != null)
        {
            Destroy(tracerInstance);
        }
    }
}