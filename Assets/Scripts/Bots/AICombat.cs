using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

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
    public AIEntity entity;

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

    [Space]
    public int burstCountMin = 2;
    public int burstCountMax = 5;
    [Space]
    public float burstDelayMin = 0.08f;
    public float burstDelayMax = 0.25f;

    [Header("Accuracy")]
    [Tooltip("Lower = more accurate")]
    public float spreadRadius = 0.15f;

    [Tooltip("Extra spread while moving")]
    public float movingSpreadMultiplier = 2f;

    [Header("Projectile")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 100f;

    [Header("Search Fire")]
    public bool suppressLastKnownPosition = true;
    public float suppressionDurationMin;
    public float suppressionDurationMax;
    private float endSuppressionTime;

    [Tooltip("Chance to continue firing at last known position")]
    [Range(0f, 1f)]
    public float suppressionFireChance = 0.5f;

    [Header("Debug")]
    public bool debugDrawShots = true;

    public float nextFireTime;
    public float nextBurstTime;

    public int burstCount;
    public int currentBurstShots;
    public bool isBursting;

    public bool isSuppressing;

    private float lightOffTime = 0f;
    private float muzzleLightDuration = .1f;

    private Vector3 aimTargetRestPosition;

    public void Initialize(AIEntity entity)
    {
        this.entity = entity;

        burstCount = Random.Range(burstCountMin, burstCountMax);

        if (weaponAimTarget != null)
        {
            aimTargetRestPosition = weaponAimTarget.localPosition;
        }
    }

    private void Update()
    {
        if (entity.currentState == AIState.Dead)
        {
            return;
        }

        switch (entity.currentState)
        {
            case AIState.Combat:
                HandleCombat();
                break;

            case AIState.Suppress:
                if (isSuppressing == false)
                {
                    isSuppressing = true;
                    endSuppressionTime = Time.time + Random.Range(suppressionDurationMin, suppressionDurationMax);
                }
                
                if (isSuppressing == true)
                {
                    if (Time.time >= endSuppressionTime)
                    {
                        isSuppressing = false;
                    }
                    else
                    {
                        HandleSuppressionFire();
                    }
                }
                break;

            default:
                ResetAimToRest();
                break;
        }

        if (weaponMuzzle.GetComponent<Light>().enabled)
        {
            if (Time.time >= lightOffTime)
            {
                weaponMuzzle.GetComponent<Light>().enabled = false;
            }
        }
    }

    // --------------------------------------------------
    // COMBAT
    // --------------------------------------------------

    private void HandleCombat()
    {
        Transform target = entity.perception.currentTarget;

        if (target == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        UpdateAim(target.position);

        if (distance <= attackRange)
        {
            TryFire(target.position);
        }
    }

    // --------------------------------------------------
    // SUPPRESSION FIRE
    // --------------------------------------------------

    private void HandleSuppressionFire()
    {
        if (!suppressLastKnownPosition)
        {
            return;
        }

        if (Random.value > suppressionFireChance)
        {
            return;
        }

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
        {
            return;
        }

        Vector3 current = weaponAimTarget.position;

        Vector3 smoothed = Vector3.Lerp(current, targetPosition, Time.deltaTime * aimSpeed);

        weaponAimTarget.position = smoothed;
    }

    private void ResetAimToRest()
    {
        if (weaponAimTarget == null)
        {
            return;
        }

        Vector3 current = weaponAimTarget.localPosition;
        Vector3 smoothed = Vector3.Lerp(current, aimTargetRestPosition, Time.deltaTime * aimSpeed);

        weaponAimTarget.localPosition = smoothed;
    }

    // --------------------------------------------------
    // FIRE CONTROL
    // --------------------------------------------------

    private void TryFire(Vector3 targetPosition)
    {
        if (Time.time < nextFireTime)
        {
            return;
        }         

        if (isBursting == true && currentBurstShots >= burstCount)
        {
            isBursting = false;
            currentBurstShots = 0;
            burstCount = Random.Range(burstCountMin, burstCountMax);
            nextBurstTime = Time.time + Random.Range(burstDelayMin, burstDelayMax);
        }

        if (automatic)
        {
            FireShot(targetPosition);
            // SET NEXT FIRE TIME
            nextFireTime = Time.time + fireRate;
        }
        else
        {
            if (!isBursting && Time.time > nextBurstTime)
            {
                isBursting = true;
            }

            if (isBursting)
            {
                if (currentBurstShots < burstCount)
                {
                    FireShot(targetPosition);

                    currentBurstShots++;
                    // SET NEXT FIRE TIME
                    nextFireTime = Time.time + fireRate;
                }
            }
        }

        
    }

    // --------------------------------------------------
    // SHOOTING
    // --------------------------------------------------
    private void FireShot(Vector3 targetPosition)
    {
        Vector3 shotDirection = GetSpreadDirection(targetPosition);

        // SPAWN PHYSICAL PROJECTILE
        GameObject bullet = Instantiate(bulletPrefab, weaponMuzzle.position, Quaternion.identity);
        bullet.transform.parent = null;
        bullet.GetComponent<Rigidbody>().linearVelocity = shotDirection * bulletSpeed;
        bullet.GetComponent<BulletData>().team = AITeam.Enemy;

        if (debugDrawShots)
        {
            Debug.DrawRay(weaponMuzzle.position, shotDirection * attackRange, Color.yellow, 1f);
        }

        weaponMuzzle.GetComponent<VisualEffect>().Play();
        weaponMuzzle.GetComponent<Light>().enabled = true;
        lightOffTime = Time.time + muzzleLightDuration;

        // PROCEDURAL RECOIL
        if (weaponMotion != null)
        {
            weaponMotion.Fire(1f);
        }
    }

    private void FireRaycastShot(Vector3 targetPosition)
    {
        Vector3 shotDirection = GetSpreadDirection(targetPosition);

        // RAYCAST HITSCAN
        //This is okay for getting things setup, but I want to use a physical projectile instead of a raycast system
        //TODO: Redo this section and instantiate a prefab bullet
        if (Physics.Raycast(weaponMuzzle.position, shotDirection, out RaycastHit hit, attackRange))
        {
            AIHealth health = hit.collider.GetComponentInParent<AIHealth>();

            if (health != null)
            {
                if (health.entity.team != entity.team)
                {
                    health.TakeDamage(20f);
                }
            }

            if (debugDrawShots)
            {
                Debug.DrawLine(weaponMuzzle.position, hit.point, Color.red, 1f);
            }
        }
        else
        {
            if (debugDrawShots)
            {
                Debug.DrawRay(weaponMuzzle.position, shotDirection * attackRange, Color.yellow, 1f);
            }
        }

        weaponMuzzle.GetComponent<VisualEffect>().Play();
        weaponMuzzle.GetComponent<Light>().enabled = true;
        lightOffTime = Time.time + muzzleLightDuration;

        // PROCEDURAL RECOIL
        if (weaponMotion != null)
        {
            weaponMotion.Fire(1f);
        }
    }

    // --------------------------------------------------
    // SPREAD
    // --------------------------------------------------

    private Vector3 GetSpreadDirection(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - weaponMuzzle.position).normalized;

        float finalSpread = spreadRadius;

        // Moving inaccuracy
        if (entity.movement != null)
        {
            float speed = entity.movement.GetVelocityMagnitude();

            if (speed > 0.1f)
            {
                finalSpread *= movingSpreadMultiplier;
            }
        }

        Vector3 spreadOffset = Random.insideUnitSphere * finalSpread;

        direction += spreadOffset;

        return direction.normalized;
    }

    public void OverideSuppression()
    {
        isSuppressing = true;

        endSuppressionTime = Time.time + Random.Range(suppressionDurationMin, suppressionDurationMax);
    }
}
