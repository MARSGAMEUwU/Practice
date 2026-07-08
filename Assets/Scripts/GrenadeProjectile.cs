using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;

public class GrenadeProjectile : MonoBehaviour
{
    private float damage;
    private float throwForce;
    private float explosionRadius;
    private float fuseTime;
    private GameObject explosionEffectPrefab;
    private Vector3 direction;
    private GameObject owner;

    private Rigidbody rb;
    private bool hasExploded = false;

    [SerializeField] private GameObject explosionSoundSource;

    public void Initialize(float damage, float throwForce, float explosionRadius,
                          float fuseTime, GameObject explosionEffectPrefab, Vector3 direction, GameObject owner)
    {
        this.damage = damage;
        this.throwForce = throwForce;
        this.explosionRadius = explosionRadius;
        this.fuseTime = fuseTime;
        this.explosionEffectPrefab = explosionEffectPrefab;
        this.direction = direction;
        this.owner = owner;

        Setup();
    }

    private void Setup()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.linearVelocity = direction * throwForce;
        }

        StartCoroutine(FuseTimer());
    }

    private IEnumerator FuseTimer()
    {
        yield return new WaitForSeconds(fuseTime);
        if (!hasExploded)
        {
            Explode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (owner != null && collision.gameObject == owner) return;
        if (owner != null && collision.transform.IsChildOf(owner.transform)) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Instantiate(explosionSoundSource, transform.position, Quaternion.identity);
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            if (owner != null && hit.gameObject == owner) continue;
            if (owner != null && hit.transform.IsChildOf(owner.transform)) continue;

            Damageable damageable = hit.GetComponent<Damageable>();
            if (damageable != null)
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                float damageFalloff = 1f - (distance / explosionRadius);
                float finalDamage = damage * Mathf.Max(0f, damageFalloff);

                damageable.TakeDamage(finalDamage);
            }
        }

        Destroy(gameObject);
    }
}