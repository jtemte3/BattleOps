using UnityEngine;

public class Grenade : MonoBehaviour
{
    [Header("Explosion")]
    public float explosionRadius = 6f;
    public float explosionForce = 800f;
    public float damage = 100f;

    float fuseRemaining = 5f;
    bool armed;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Arm(float fuseTime)
    {
        fuseRemaining = fuseTime;
        armed = true;
    }

    void Update()
    {
        if (!armed) return;

        fuseRemaining -= Time.deltaTime;

        if (fuseRemaining <= 0f)
        {
            Explode();
        }
    }

    void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hit in hits)
        {
            Rigidbody r = hit.attachedRigidbody;
            if (r != null)
            {
                r.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }

            // Optional:
            // hit.GetComponent<IDamageable>()?.TakeDamage(damage);
        }

        // TODO: spawn VFX / SFX here

        Destroy(gameObject);
    }
}
