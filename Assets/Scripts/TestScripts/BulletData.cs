using UnityEngine;

public class BulletData : MonoBehaviour
{
    public AITeam team;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        // Calculate speed and distance to travel this frame
        float speed = rb.linearVelocity.magnitude;

        if (speed < 0.01f)
        {
            return; // Bullet has stopped or hasn't started moving yet
        }

        float moveDistance = speed * Time.fixedDeltaTime;

        // Raycast forward along the velocity vector
        if (Physics.Raycast(transform.position, rb.linearVelocity.normalized, out RaycastHit hit, moveDistance))
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
            rb.linearVelocity = Vector3.zero;
            GetComponent<Renderer>().enabled = false;

            //Destroy(gameObject);
        }
    }
}