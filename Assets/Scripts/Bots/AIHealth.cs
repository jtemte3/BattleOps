using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;

/// <summary>
/// Handles:
/// - Health
/// - Damage
/// - Death
/// - Ragdoll activation
/// - Cleanup
/// 
/// Does NOT:
/// - Handle combat
/// - Handle perception
/// - Handle AI decisions
/// </summary>
public class AIHealth : MonoBehaviour, IAIBehaviour
{
    public AIEntity entity;

    [Header("Health")]
    public float maxHealth = 100f;

    [SerializeField]
    private float currentHealth;

    [Header("Death")]
    public bool destroyOnDeath = false;

    public float destroyDelay = 20f;

    [Header("Ragdoll")]
    public Rigidbody[] ragdollBodies;
    public Collider[] ragdollColliders;

    [Header("Disable On Death")]
    public MonoBehaviour[] scriptsToDisable;

    public Animator animator;
    public RigBuilder rigBuilder;

    private NavMeshAgent agent;

    private Collider mainCollider;

    private Rigidbody mainRigidbody;

    private bool isDead;

    public void Initialize(AIEntity entity)
    {
        this.entity = entity;

        currentHealth = maxHealth;

        agent = GetComponent<NavMeshAgent>();

        mainCollider = GetComponent<Collider>();
        mainRigidbody = GetComponent<Rigidbody>();

        SetupRagdoll();
    }

    // --------------------------------------------------
    // INITIALIZATION
    // --------------------------------------------------

    private void SetupRagdoll()
    {
        foreach (Rigidbody rb in ragdollBodies)
        {
            if (rb == null)
                continue;

            rb.isKinematic = true;
        }

        foreach (Collider col in ragdollColliders)
        {
            if (col == null)
                continue;

            col.enabled = false;
        }
    }

    // --------------------------------------------------
    // DAMAGE
    // --------------------------------------------------

    public void TakeDamage(float damage)
    {
        if (isDead)
        {
            return;
        }

        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    // --------------------------------------------------
    // DEATH
    // --------------------------------------------------

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        currentHealth = 0f;

        // ENTITY STATE
        if (entity != null)
        {
            entity.Die();
        }

        // STOP NAVIGATION
        if (agent != null)
        {
            agent.enabled = false;
        }

        // DISABLE ANIMATOR
        if (animator != null)
        {
            animator.enabled = false;
        }

        // DISABLE RIGGING
        if (rigBuilder != null)
        {
            rigBuilder.enabled = false;
        }

        // DISABLE CUSTOM SCRIPTS
        foreach (MonoBehaviour script in scriptsToDisable)
        {
            if (script == null)
            {
                continue;
            }

            script.enabled = false;
        }

        // MAIN COLLIDER
        if (mainCollider != null)
        {
            mainCollider.enabled = false;
        }

        // MAIN RB
        if (mainRigidbody != null)
        {
            mainRigidbody.isKinematic = true;
        }

        EnableRagdoll();

        // OPTIONAL CLEANUP
        if (destroyOnDeath)
        {
            Destroy(gameObject, destroyDelay);
        }
    }

    // --------------------------------------------------
    // RAGDOLL
    // --------------------------------------------------

    private void EnableRagdoll()
    {
        foreach (Rigidbody rb in ragdollBodies)
        {
            if (rb == null)
            {
                continue; 
            }

            rb.isKinematic = false;
        }

        foreach (Collider col in ragdollColliders)
        {
            if (col == null)
            {
                continue;
            }

            col.enabled = true;
        }
    }

    // --------------------------------------------------
    // OPTIONAL FORCE APPLICATION
    // --------------------------------------------------

    public void ApplyImpactForce(Vector3 force, Vector3 hitPoint, float radius = 0.25f)
    {
        foreach (Rigidbody rb in ragdollBodies)
        {
            if (rb == null)
            {
                continue;
            }

            rb.AddExplosionForce(force.magnitude, hitPoint, radius);
        }
    }

    // --------------------------------------------------
    // HELPERS
    // --------------------------------------------------
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
    // --------------------------------------------------
    // DEBUG
    // --------------------------------------------------

    public float GetHealthPercent()
    {
        return currentHealth / maxHealth;
    }

    public bool IsDead()
    {
        return isDead;
    }
}