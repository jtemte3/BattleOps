using UnityEngine;
using UnityEngine.UIElements;

public class Grenade : MonoBehaviour
{
    [Header("Team Data")]
    public AITeam team;
    [Header("Explosion")]
    public float explosionRadius = 6f;
    public float explosionForce = 800f;
    public float damage = 100f;
    public bool deleteOnEffect = true;

    public GameObject effectObject;

    float fuseRemaining = 5f;
    bool armed;
    bool effectEnabled = false;

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
            if (effectEnabled == false)
            {
                Explode();
                effectEnabled = true;
            }
            
        }
    }

    void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hit in hits)
        {
            //Rigidbody r = hit.attachedRigidbody;
            //if (r != null)
            //{
            //    r.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            //}

            // If the collider is a hitbox, apply damage
            if (hit.GetComponent<HitBox>() != null)
            {
                float distance = Vector3.Distance(transform.position, hit.attachedRigidbody.position);
                float damageMultiplier = (distance / explosionRadius) * 100;
                hit.GetComponent<HitBox>().TakeHit(this, damage * damageMultiplier);
            }
        }

        Common.AlertViaSound(transform.position, explosionRadius * 5, team);

        // TODO: spawn VFX / SFX here

        GameObject effectObj = Instantiate(effectObject, transform.position, transform.rotation);

        effectObj.transform.rotation = Quaternion.Euler(Vector3.zero);

        if (effectObj.GetComponent<TimedApplyForce>())
        {
            effectObj.GetComponent<TimedApplyForce>().SetupExplosion(explosionRadius, explosionForce);
        }

        if (deleteOnEffect)
        {
            Destroy(gameObject);
        }
    }
}
