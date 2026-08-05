using UnityEngine;

public class TimedApplyForce : MonoBehaviour
{
    public float timer;
    float endTime;
    public float explosionRadius;
    public float explosionForce;

    void Start()
    {
        endTime = Time.time + timer;
    }

    private void Update()
    {
        if (Time.time >= endTime)
        {
            ApplyExplosionForce();
        }
    }

    public void ApplyExplosionForce()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hit in hits)
        {
            Rigidbody r = hit.attachedRigidbody;
            if (r != null)
            {
                r.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }
    }

    public void SetupExplosion(float radius, float force)
    {
        explosionForce = force;
        explosionRadius = radius;
    }
}
