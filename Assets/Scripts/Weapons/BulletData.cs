using UnityEngine;


public class BulletData : MonoBehaviour
{
    public AITeam team;
    private Rigidbody bulletRigidbody;
    public float alertRadius = 5;
    public bool hasImpacted = false;

    private void Start()
    {
        bulletRigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        CheckForImpact();

        if (!hasImpacted)
        {
            Common.AlertViaSound(transform.position,alertRadius, team);
        }
    }

    private void CheckForImpact()
    {
        if (bulletRigidbody == null)
        {
            return;
        }

        // Calculate speed and distance to travel this frame
        float speed = bulletRigidbody.linearVelocity.magnitude;

        if (speed < 0.01f)
        {
            return; // Bullet has stopped or hasn't started moving yet
        }

        float moveDistance = speed * Time.fixedDeltaTime;

        // Raycast forward along the velocity vector
        if (Physics.Raycast(transform.position, bulletRigidbody.linearVelocity.normalized, out RaycastHit hit, moveDistance))
        {
            // Check if we hit a HitBox component
            HitBox hitBox = hit.collider.GetComponent<HitBox>();

            if (hitBox != null)
            {
                // Call the hitbox script to handle the damage logic
                hitBox.TakeHit(this);
            }

            // Disable the bullet rendering/physics upon impact
            GetComponent<SphereCollider>().enabled = false;
            bulletRigidbody.linearVelocity = Vector3.zero;
            GetComponent<Renderer>().enabled = false;

            //Destroy(gameObject);
            hasImpacted = true;
        }
    }
}