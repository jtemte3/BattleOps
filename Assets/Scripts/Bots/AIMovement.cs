using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class AIMovement : MonoBehaviour, IAIBehaviour
{
    public AIEntity entity;
    private NavMeshAgent agent;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    private int patrolIndex;

    [Header("Search")]
    public bool isSearching;
    public int searchPositionCount;
    public List<Vector3> searchPositions = new();
    public int searchIndex;
    public float searchRadius;

    [Header("Animation")]
    public ProceduralFootIK footIK;
    public ProceduralWeaponMotion weaponMotion;

    [Header("Stopping Distances")]
    public float patrolStoppingDistance = 0;
    public float combatStoppingDistance = 3;
    public float searchStoppingDistance = 5;

    public void Initialize(AIEntity entity)
    {
        this.entity = entity;

        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (entity.currentState == AIState.Dead)
        {
            return;
        }

        UpdateAnimation();

        switch (entity.currentState)
        {
            case AIState.Idle:
                agent.stoppingDistance = patrolStoppingDistance;
                HandleIdle();
                break;
            case AIState.Patrol:
                agent.stoppingDistance = patrolStoppingDistance;
                HandlePatrol();
                break;

            case AIState.Combat:
                //Eventually I want to update this state.
                /*
                 *This state should:
                 *  1. Find cover with its target still in range, and shoot from it
                 *  2. Move into shooting range with the target, shoot a bit, and then re-position and repeat
                 *  3. Move to shooting range and croutch, shoot a bit and then re-position and repeat
                 *  
                 *  If the bot is low on health it should dis-engage and run away and heal before re-engaging
                 */
                agent.stoppingDistance = combatStoppingDistance;
                HandleCombatMovement();
                
                break;

            case AIState.Search:
                //Eventually this should consist of finding 3-5 spots on the navmesh to investigate and move to those areas before returning to a patrol
                /*
                 * Like a patrol, but it selects random locations in a radius of the bots location
                 * Once all of the locations have been searched, it will resume its patrol
                 * 
                 * Maybe a squad version of this will have a the bots look at 3-4 locations before they all rally at the next patrol point and continue as a group
                 */
                agent.stoppingDistance = 0;
                HandleSearchMovement();
                break;
        }
    }

    private void UpdateAnimation()
    {
        float speed = agent.velocity.magnitude;

        if (footIK != null)
        {
            footIK.moveSpeed = speed;
        }

        if (weaponMotion != null)
        {
            weaponMotion.moveSpeed = speed;
        }
    }

    private void HandleIdle()
    {
        if (patrolPoints.Length == 1)
        {
            if (Vector3.Distance(agent.transform.position, patrolPoints[0].transform.position) > 1f)
            {
                agent.SetDestination(patrolPoints[0].position);
            }
        }
        if (patrolPoints.Length >= 1)
        {
            entity.currentState = AIState.Patrol;
        }
    }

    private void HandlePatrol()
    {
        if (patrolPoints.Length == 0)
        {
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 1f)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;

            agent.SetDestination(patrolPoints[patrolIndex].position);
        }
    }

    private void HandleCombatMovement()
    {
        if (entity.perception.currentTarget == null)
        {
            return;
        }

        agent.SetDestination(entity.perception.currentTarget.position);
    }

    public float GetVelocityMagnitude()
    {
        return agent.velocity.magnitude;
    }

    public void SetupSearchPath()
    {
        isSearching = true;
        searchPositions.Clear();
        searchIndex = 0;

        for (int i = 0; i < searchPositionCount; i++)
        {
            Vector3 newSearchPos = agent.transform.position + new Vector3(Random.Range(0, searchRadius), 0, Random.Range(0, searchRadius));

            if (IsPositionOnNavMesh(newSearchPos))
            {
                searchPositions.Add(newSearchPos);
            }
            else
            {
                i = Mathf.Max(0, i--);
            }
        }

        if (searchPositions.Count > 0)
        {
            agent.SetDestination(searchPositions[0]);
        }
    }

    private void HandleSearchMovement()
    {
        if(searchPositions.Count > 0)
        {
            float dist = Vector3.Distance(agent.transform.position, searchPositions[searchIndex]);

            if (dist < searchStoppingDistance)
            {
                if (searchIndex == searchPositions.Count-1)
                {
                    isSearching = false;
                }
                else
                {
                    searchIndex++;
                }
            }

            agent.SetDestination(searchPositions[searchIndex]);
        }
    }

    public bool IsPositionOnNavMesh(Vector3 targetPosition)
    {
        NavMeshHit hit;

        if (NavMesh.SamplePosition(targetPosition, out hit, .1f, NavMesh.AllAreas)){
            return true;
        }
        else
        {
            return false;
        }
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