using UnityEngine;

/// <summary>
/// Handles:
/// - Target aiming
/// - Fire timing
/// - Burst fire
/// - Accuracy/spread
/// - Weapon integration
/// - Procedural recoil
/// 
/// Does NOT:
/// - Control movement
/// - Decide AI states
/// - Handle perception
/// </summary>
public class AICombat : MonoBehaviour, IAIBehaviour
{
    private AIEntity entity;

    [Header("References")]
    public Transform weaponMuzzle;

    [Tooltip("Transform the rig aims toward")]
    public Transform weaponAimTarget;

    public ProceduralWeaponMotion weaponMotion;

    [Header("Combat")]
    public float attackRange = 25f;

    [Tooltip("How quickly AI rotates aim target")]
    public float aimSpeed = 12f;

    [Header("Fire Settings")]
    public bool automatic = true;

    public float fireRate = 0.1f;

    public int burstCount = 3;

    public float burstDelay = 0.08f;

    [Header("Accuracy")]
    [Tooltip("Lower = more accurate")]
    public float spreadRadius = 0.15f;

    [Tooltip("Extra spread while moving")]
    public float movingSpreadMultiplier = 2f;

    [Header("Search Fire")]
    public bool suppressLastKnownPosition = true;

    [Tooltip("Chance to continue firing at last known position")]
    [Range(0f, 1f)]
    public float suppressionFireChance = 0.5f;

    [Header("Debug")]
    public bool debugDrawShots = true;

    private float nextFireTime;

    private int currentBurstShots;

    private bool isBursting;

    public void Initialize(AIEntity entity)
    {
        this.entity = entity;
    }

    private void Update()
    {
        if (entity.currentState == AIState.Dead)
            return;

        switch (entity.currentState)
        {
            case AIState.Combat:
                HandleCombat();
                break;

            case AIState.Search:
                HandleSuppressionFire();
                break;
        }
    }

    // --------------------------------------------------
    // COMBAT
    // --------------------------------------------------

    private void HandleCombat()
    {
        Transform target =
            entity.perception.currentTarget;

        if (target == null)
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                target.position
            );

        UpdateAim(target.position);

        if (distance <= attackRange)
        {
            TryFire(target.position);
        }
    }

    // --------------------------------------------------
    // SEARCH / SUPPRESSION FIRE
    // --------------------------------------------------

    private void HandleSuppressionFire()
    {
        if (!suppressLastKnownPosition)
            return;

        if (Random.value > suppressionFireChance)
            return;

        UpdateAim(
            entity.perception.lastKnownPosition
        );

        TryFire(
            entity.perception.lastKnownPosition
        );
    }

    // --------------------------------------------------
    // AIMING
    // --------------------------------------------------

    private void UpdateAim(Vector3 targetPosition)
    {
        if (weaponAimTarget == null)
            return;

        Vector3 current =
            weaponAimTarget.position;

        Vector3 smoothed =
            Vector3.Lerp(
                current,
                targetPosition,
                Time.deltaTime * aimSpeed
            );

        weaponAimTarget.position = smoothed;
    }

    // --------------------------------------------------
    // FIRE CONTROL
    // --------------------------------------------------

    private void TryFire(Vector3 targetPosition)
    {
        if (Time.time < nextFireTime)
            return;

        nextFireTime =
            Time.time + fireRate;

        if (automatic)
        {
            FireShot(targetPosition);
        }
        else
        {
            if (!isBursting)
            {
                StartCoroutine(
                    BurstFire(targetPosition)
                );
            }
        }
    }

    // --------------------------------------------------
    // BURST FIRE
    // --------------------------------------------------

    private System.Collections.IEnumerator BurstFire(
        Vector3 targetPosition)
    {
        isBursting = true;

        currentBurstShots = 0;

        while (currentBurstShots < burstCount)
        {
            FireShot(targetPosition);

            currentBurstShots++;

            yield return new WaitForSeconds(
                burstDelay
            );
        }

        isBursting = false;
    }

    // --------------------------------------------------
    // SHOOTING
    // --------------------------------------------------

    private void FireShot(Vector3 targetPosition)
    {
        Vector3 shotDirection =
            GetSpreadDirection(targetPosition);

        // RAYCAST HITSCAN
        if (Physics.Raycast(
                weaponMuzzle.position,
                shotDirection,
                out RaycastHit hit,
                attackRange))
        {
            AIHealth health =
                hit.collider.GetComponentInParent<AIHealth>();

            if (health != null)
            {
                health.TakeDamage(20f);
            }

            if (debugDrawShots)
            {
                Debug.DrawLine(
                    weaponMuzzle.position,
                    hit.point,
                    Color.red,
                    1f
                );
            }
        }
        else
        {
            if (debugDrawShots)
            {
                Debug.DrawRay(
                    weaponMuzzle.position,
                    shotDirection * attackRange,
                    Color.yellow,
                    1f
                );
            }
        }

        // PROCEDURAL RECOIL
        if (weaponMotion != null)
        {
            weaponMotion.Fire(1f);
        }
    }

    // --------------------------------------------------
    // SPREAD
    // --------------------------------------------------

    private Vector3 GetSpreadDirection(
        Vector3 targetPosition)
    {
        Vector3 direction =
            (targetPosition - weaponMuzzle.position)
            .normalized;

        float finalSpread = spreadRadius;

        // Moving inaccuracy
        if (entity.movement != null)
        {
            float speed =
                entity.movement.GetVelocityMagnitude();

            if (speed > 0.1f)
            {
                finalSpread *=
                    movingSpreadMultiplier;
            }
        }

        Vector3 spreadOffset =
            Random.insideUnitSphere *
            finalSpread;

        direction += spreadOffset;

        return direction.normalized;
    }
}