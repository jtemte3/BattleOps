using UnityEngine;

/// <summary>
/// Handles patrol-specific movement logic.
/// Separated from AIMovement to allow external managers (like AISquad) 
/// to control patrol destinations and arrival states.
/// </summary>
public class AIPatrol : MonoBehaviour, IAIBehaviour
{
    public AIEntity entity;

    [Header("Patrol Settings")]
    [Tooltip("Distance to stop when reaching a patrol destination")]
    public float stoppingDistance = 1.5f;

    private Vector3 currentDestination;
    private bool hasDestination = false;

    public void Initialize(AIEntity entity)
    {
        this.entity = entity;
    }

    /// <summary>
    /// Called by an external manager (e.g., AISquad) to set where this bot should move.
    /// </summary>
    public void SetDestination(Vector3 destination)
    {
        currentDestination = destination;
        hasDestination = true;
    }

    /// <summary>
    /// Called by AIMovement when the entity is in Patrol state.
    /// </summary>
    public void HandlePatrol()
    {
        if (!hasDestination) return;

        // FIX: Don't set patrol destination if entity is in Combat state
        // This prevents patrol logic from overriding combat movement
        if (entity.currentState == AIState.Combat)
        {
            return;
        }

        UnityEngine.AI.NavMeshAgent agent = entity.movement.GetNavMeshAgent();
        agent.stoppingDistance = stoppingDistance;

        // Only set destination if we are far enough to prevent micro-adjustments
        if (Vector3.Distance(transform.position, currentDestination) > stoppingDistance)
        {
            agent.SetDestination(currentDestination);
        }
    }

    /// <summary>
    /// Checks if the bot has arrived at its assigned patrol destination.
    /// </summary>
    public bool IsAtDestination()
    {
        if (!hasDestination) return false;
        return Vector3.Distance(transform.position, currentDestination) <= stoppingDistance;
    }

    /// <summary>
    /// Clears the current patrol destination and resets the agent path.
    /// </summary>
    public void ClearDestination()
    {
        hasDestination = false;
        if (entity.movement != null)
        {
            entity.movement.GetNavMeshAgent().ResetPath();
        }
    }
}
