using UnityEngine;
using UnityEngine.AI;

public class AIMovement : MonoBehaviour, IAIBehaviour
{
    private AIEntity entity;
    private NavMeshAgent agent;

    [Header("Patrol")]
    public Transform[] patrolPoints;

    private int patrolIndex;

    [Header("Animation")]
    public ProceduralFootIK footIK;
    public ProceduralWeaponMotion weaponMotion;

    public void Initialize(AIEntity entity)
    {
        this.entity = entity;

        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (entity.currentState == AIState.Dead)
            return;

        UpdateAnimation();

        switch (entity.currentState)
        {
            case AIState.Patrol:
                HandlePatrol();
                break;

            case AIState.Combat:
                HandleCombatMovement();
                break;

            case AIState.Search:
                HandleSearchMovement();
                break;
        }
    }

    private void UpdateAnimation()
    {
        float speed = agent.velocity.magnitude;

        if (footIK != null)
            footIK.moveSpeed = speed;

        if (weaponMotion != null)
            weaponMotion.moveSpeed = speed;
    }

    private void HandlePatrol()
    {
        if (patrolPoints.Length == 0)
            return;

        if (!agent.pathPending &&
            agent.remainingDistance < 1f)
        {
            patrolIndex =
                (patrolIndex + 1) % patrolPoints.Length;

            agent.SetDestination(
                patrolPoints[patrolIndex].position
            );
        }
    }

    private void HandleCombatMovement()
    {
        if (entity.perception.currentTarget == null)
            return;

        agent.SetDestination(
            entity.perception.currentTarget.position
        );
    }

    public float GetVelocityMagnitude()
    {
        return agent.velocity.magnitude;
    }

    private void HandleSearchMovement()
    {
        agent.SetDestination(
            entity.perception.lastKnownPosition
        );
    }

    public void MoveTo(Vector3 position)
    {
        agent.SetDestination(position);
    }

    public void Stop()
    {
        agent.ResetPath();
    }
}