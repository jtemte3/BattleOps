using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Handles:
/// - Vision cone checks
/// - Line of sight
/// - Target acquisition
/// - Last known positions
/// - Suspicion
/// 
/// This system does NOT make combat decisions.
/// It only gathers information.
/// </summary>
public class AIPerception : MonoBehaviour, IAIBehaviour
{
    public AIEntity entity;

    [Header("Vision")]
    public Transform eyePoint;

    [Tooltip("Maximum vision range")]
    public float viewDistance = 30f;

    [Tooltip("Total vision cone angle")]
    [Range(1f, 180f)]
    public float viewAngle = 90f;

    [Tooltip("How often perception updates")]
    public float perceptionInterval = 0.2f;

    [Tooltip("Layers considered obstacles")]
    public LayerMask obstacleMask;

    [Tooltip("Layers considered AI targets")]
    public LayerMask targetMask;

    [Header("Detection")]
    public float suspicionIncreaseRate = 2f;
    public float suspicionDecayRate = 1f;

    [Tooltip("Suspicion needed for full detection")]
    public float detectionThreshold = 3f;

    [Header("Debug")]
    public bool drawDebug = true;

    // TARGET DATA
     public Transform currentTarget;
     public Vector3 lastKnownPosition;

    
    public DetectionState detectionState = DetectionState.None;

    public float suspicionLevel;

    private float nextPerceptionTime;

    private Collider[] results;

    public void Initialize(AIEntity entity)
    {
        this.entity = entity;
    }

    private void Update()
    {
        if (Time.time >= nextPerceptionTime)
        {
            nextPerceptionTime = Time.time + perceptionInterval;

            PerformPerceptionCheck();
        }

        UpdateSuspicion();
    }

    // --------------------------------------------------
    // MAIN PERCEPTION
    // --------------------------------------------------

    private void PerformPerceptionCheck()
    {
        results = Physics.OverlapSphere(eyePoint.position, viewDistance, targetMask);

        Transform bestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach(Collider col in results)
        {
            if (col == null)
            {
                continue;
            }

            AIEntity otherEntity =
                col.GetComponentInParent<AIEntity>();

            if (otherEntity == null)
            {
                continue;
            }

            // Ignore same team
            if (otherEntity.team == entity.team && otherEntity.team == AITeam.Neutral)
            {
                continue;
            }

            Transform target = otherEntity.transform;

            float distance = Vector3.Distance(eyePoint.position, target.position);

            if (!IsTargetVisable(target))
            {
                continue;
            }
            /*if (!CanSeeTarget(target))
            {
                continue;
            }*/

            if (distance < closestDistance)
            {
                closestDistance = distance;
                bestTarget = otherEntity.target;
            }
        }

        if (bestTarget != null)
        {
            currentTarget = bestTarget;
            lastKnownPosition = bestTarget.position;

            suspicionLevel += suspicionIncreaseRate;
            suspicionLevel = Mathf.Min(suspicionLevel, detectionThreshold);
        }
        else
        {
            currentTarget = null;
        }
    }

    // --------------------------------------------------
    // VISION CHECK
    // --------------------------------------------------

    private bool CanSeeTarget(Transform target)
    {
        Vector3 dirToTarget = (eyePoint.position - target.position).normalized;

        // ANGLE CHECK
        float angle = Vector3.Angle(-entity.transform.forward, dirToTarget);

        if (angle > viewAngle * 0.5f)
        {
            return false;
        }
            

        // DISTANCE CHECK
        float distance = Vector3.Distance( eyePoint.position, target.position);

        if (distance > viewDistance)
        {
            return false;
        }

        // LINE OF SIGHT
        if (Physics.Raycast(eyePoint.position, -dirToTarget, out RaycastHit hit, distance, obstacleMask | targetMask))
        {
            if (hit.transform != target && !hit.transform.IsChildOf(target))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsTargetVisable(Transform target)
    {
        Vector3 targetDirection = (entity.transform.position - target.position).normalized;
        float targetAngle = Vector3.Angle(-entity.transform.forward, targetDirection);
        if (targetAngle < (viewAngle / 2))
        {
            float distanceToTarget = Vector3.Distance(entity.transform.position, target.position);

            Debug.DrawRay(entity.transform.position, -targetDirection * distanceToTarget, Color.green, 1);
            if (Physics.Raycast(eyePoint.position, -targetDirection, distanceToTarget, obstacleMask))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else
        {
            return false;
        }
    }

    // --------------------------------------------------
    // SUSPICION
    // --------------------------------------------------

    private void UpdateSuspicion()
    {
        if (currentTarget != null)
        {
            if (suspicionLevel >= detectionThreshold)
            {
                detectionState = DetectionState.Detected;
            }
            else
            {
                detectionState = DetectionState.Suspicious;
            }
        }
        else
        {
            suspicionLevel -= suspicionDecayRate * Time.deltaTime;

            suspicionLevel = Mathf.Max(0f, suspicionLevel);

            if (suspicionLevel <= 0f)
            {
                detectionState = DetectionState.None;
            }
            else
            {
                detectionState = DetectionState.LostTarget;
            }
        }
    }

    // --------------------------------------------------
    // DEBUG
    // --------------------------------------------------

    private void OnDrawGizmosSelected()
    {
        if (!drawDebug || eyePoint == null)
        {
            return;
        }

        //Gizmos.DrawWireSphere(eyePoint.position, viewDistance);
        Gizmos.color = Color.yellow;
        Handles.color = Color.yellow;
        Handles.DrawWireArc(eyePoint.position, Vector3.up, Quaternion.Euler(0, -viewAngle * 0.5f, 0) * eyePoint.forward, viewAngle, viewDistance);

        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle * 0.5f, 0) * eyePoint.forward;

        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle * 0.5f, 0) * eyePoint.forward;

        Gizmos.color = Color.cyan;

        Gizmos.DrawRay(eyePoint.position, leftBoundary * viewDistance);

        Gizmos.DrawRay(eyePoint.position, rightBoundary * viewDistance);

        if (entity.combat != null)
        {
            Handles.color = Color.red;
            Handles.DrawWireArc(eyePoint.position, Vector3.up, Quaternion.Euler(0, -viewAngle * 0.5f, 0) * eyePoint.forward, viewAngle, entity.combat.attackRange);
        }

        if (currentTarget != null)
        {
            Gizmos.color = Color.orange;

            Gizmos.DrawLine(eyePoint.position, currentTarget.position);
        }
    }
}
