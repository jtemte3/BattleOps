using UnityEngine;
using UnityEngine.Animations.Rigging;

/// <summary>
/// Procedural weapon motion using Animation Rigging constraints.
/// 
/// Features:
/// - Constraint-based bobbing
/// - Aim sway
/// - Additive recoil
/// - Automatic recoil recovery
/// - Min/max recoil scaling
/// </summary>
public class ProceduralWeaponMotion : MonoBehaviour
{
    [Header("Constraints")]
    public MultiPositionConstraint positionConstraint;
    public MultiAimConstraint aimConstraint;

    [Header("Movement")]
    public float moveSpeed;
    public float movementThreshold = 0.05f;

    [Header("Timing")]
    public float walkCycleSpeed = 5f;

    [Header("Position Bob")]
    public float verticalBob = 0.015f;
    public float horizontalBob = 0.01f;
    public float forwardBob = 0.01f;

    [Header("Rotation Sway")]
    public float pitchAmount = 2f;
    public float yawAmount = 1f;
    public float rollAmount = 3f;

    [Header("Recoil")]
    [Tooltip("Minimum recoil strength")]
    [Range(0f, 5f)]
    public float minRecoilScale = 0.5f;

    [Tooltip("Maximum recoil strength")]
    [Range(0f, 10f)]
    public float maxRecoilScale = 2f;

    [Tooltip("How fast recoil kicks backward")]
    public float recoilKickSpeed = 25f;

    [Tooltip("How fast recoil returns")]
    public float recoilRecoverySpeed = 10f;

    [Tooltip("Positional recoil amount")]
    public Vector3 recoilPosition = new Vector3(0f, 0f, -0.08f);

    [Header("Smoothing")]
    public float positionSmoothness = 8f;
    public float rotationSmoothness = 8f;

    [Header("Idle")]
    public float idleReturnSpeed = 5f;

    private float cycle;

    private Vector3 basePositionOffset;
    private Vector3 baseRotationOffset;

    private Vector3 currentPositionOffset;
    private Vector3 currentRotationOffset;

    // RECOIL
    private Vector3 recoilPositionCurrent;

    private Vector3 recoilPositionTarget;

    private void Start()
    {
        basePositionOffset = positionConstraint.data.offset;
        baseRotationOffset = aimConstraint.data.offset;
    }

    private void Update()
    {
        UpdateCycle();

        AnimatePositionOffset();
        AnimateRotationOffset();

        UpdateRecoil();

        ApplyOffsets();

        positionConstraint.gameObject.transform.localRotation.eulerAngles.Set(0,0,0);
    }

    // --------------------------------------------------
    // PUBLIC RECOIL CALL
    // --------------------------------------------------

    /// <summary>
    /// Call this whenever the gun fires.
    /// </summary>
    /// <param name="recoilScale">
    /// 0 = min recoil
    /// 1 = max recoil
    /// </param>
    public void Fire(float recoilScale = 1f)
    {
        float recoilAmount =
            Mathf.Lerp(
                minRecoilScale,
                maxRecoilScale,
                recoilScale
            );

        // ADDITIVE RECOIL
        recoilPositionTarget += recoilPosition * recoilAmount;
    }

    // --------------------------------------------------
    // WALK CYCLE
    // --------------------------------------------------

    private void UpdateCycle()
    {
        if (moveSpeed > movementThreshold)
        {
            cycle += Time.deltaTime * walkCycleSpeed * moveSpeed;
        }
    }

    // --------------------------------------------------
    // POSITION BOB
    // --------------------------------------------------

    private void AnimatePositionOffset()
    {
        Vector3 targetOffset = Vector3.zero;

        if (moveSpeed > movementThreshold)
        {
            float speedFactor = Mathf.Clamp01(moveSpeed);

            float vertical =
                Mathf.Sin(cycle * 2f) *
                verticalBob *
                speedFactor;

            float horizontal =
                Mathf.Sin(cycle) *
                horizontalBob *
                speedFactor;

            float forward =
                Mathf.Cos(cycle * 2f) *
                forwardBob *
                speedFactor;

            targetOffset = new Vector3(
                horizontal,
                vertical,
                forward
            );
        }

        currentPositionOffset =
            Vector3.Lerp(
                currentPositionOffset,
                targetOffset,
                Time.deltaTime *
                (moveSpeed > movementThreshold
                    ? positionSmoothness
                    : idleReturnSpeed)
            );
    }

    // --------------------------------------------------
    // AIM SWAY
    // --------------------------------------------------

    private void AnimateRotationOffset()
    {
        Vector3 targetRot = Vector3.zero;

        if (moveSpeed > movementThreshold)
        {
            float speedFactor = Mathf.Clamp01(moveSpeed);

            float pitch =
                Mathf.Sin(cycle * 2f) *
                pitchAmount *
                speedFactor;

            float yaw =
                Mathf.Sin(cycle) *
                yawAmount *
                speedFactor;

            float roll =
                Mathf.Sin(cycle) *
                rollAmount *
                speedFactor;

            targetRot = new Vector3(
                pitch,
                yaw,
                roll
            );
        }

        currentRotationOffset =
            Vector3.Lerp(
                currentRotationOffset,
                targetRot,
                Time.deltaTime *
                (moveSpeed > movementThreshold
                    ? rotationSmoothness
                    : idleReturnSpeed)
            );
    }

    // --------------------------------------------------
    // RECOIL
    // --------------------------------------------------

    private void UpdateRecoil()
    {
        // SNAP toward target quickly
        recoilPositionCurrent =
            Vector3.Lerp(
                recoilPositionCurrent,
                recoilPositionTarget,
                Time.deltaTime * recoilKickSpeed
            );

        // RECOVER target back to zero
        recoilPositionTarget =
            Vector3.Lerp(
                recoilPositionTarget,
                Vector3.zero,
                Time.deltaTime * recoilRecoverySpeed
            );
    }

    // --------------------------------------------------
    // APPLY TO CONSTRAINTS
    // --------------------------------------------------

    private void ApplyOffsets()
    {
        // POSITION
        var posData = positionConstraint.data;

        posData.offset =
            basePositionOffset +
            currentPositionOffset +
            recoilPositionCurrent;

        positionConstraint.data = posData;

        // ROTATION
        var aimData = aimConstraint.data;

        aimData.offset =
            baseRotationOffset +
            currentRotationOffset;

        aimConstraint.data = aimData;
    }
}