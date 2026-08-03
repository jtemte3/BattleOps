using UnityEngine;

/// <summary>
/// Procedural in-place foot IK walker.
/// 
/// Features:
/// - Procedural looping footsteps
/// - Adjustable foot spacing
/// - Pelvis oscillation synced to footsteps
/// - Speed-scaled animation intensity
/// - IK target + hint support
/// 
/// Designed for Unity Animation Rigging.
/// </summary>
public class ProceduralFootIK : MonoBehaviour
{
    [Header("References")]
    public Transform body;
    public Transform pelvis;

    [Header("Feet")]
    public Transform leftFootTarget;
    public Transform rightFootTarget;

    private Vector3 leftRestLocalPos;
    private Vector3 rightRestLocalPos;

    private Quaternion leftRestLocalRot;
    private Quaternion rightRestLocalRot;

    [Header("Hints")]
    public Transform leftHint;
    public Transform rightHint;

    [Header("Movement")]
    [Tooltip("Current AI movement speed")]
    public float moveSpeed = 0f;

    [Tooltip("Minimum speed before movement begins")]
    public float movementThreshold = 0.05f;

    [Tooltip("Speed to transition to idle state")]
    public float idleResetSpeed = 6;

    [Header("Foot Settings")]
    [Tooltip("Horizontal distance between feet")]
    public float footSpacing = 0.15f;

    [Tooltip("Forward/back stride length")]
    public float strideLength = 0.35f;

    [Tooltip("Vertical foot lift")]
    public float stepHeight = 0.15f;

    [Tooltip("Walk cycle speed multiplier")]
    public float walkCycleSpeed = 5f;

    [Header("Pelvis Motion")]
    [Tooltip("Base vertical offset for pelvis height")]
    public float pelvisHeightOffset = -0.1f;

    [Tooltip("Vertical pelvis bounce amount")]
    public float pelvisBounceStrength = 0.05f;

    [Tooltip("Pelvis side-to-side sway")]
    public float pelvisSideStrength = 0.03f;

    [Tooltip("How much movement speed affects pelvis motion")]
    public float pelvisSpeedMultiplier = 1f;

    [Header("Hint Settings")]
    public float hintHeight = 0.5f;
    public float hintForwardOffset = 0.25f;

    private float cycle;

    private Vector3 pelvisStartLocalPos;

    private void Start()
    {
        leftRestLocalPos = leftFootTarget.localPosition;
        rightRestLocalPos = rightFootTarget.localPosition;

        leftRestLocalRot = leftFootTarget.localRotation;
        rightRestLocalRot = rightFootTarget.localRotation;

        if (pelvis != null)
        {
            pelvisStartLocalPos = pelvis.localPosition;
        }
    }

    private void Update()
    {
        UpdateCycle();

        AnimateFeet();
        AnimatePelvis();
        UpdateHints();
    }

    private void UpdateCycle()
    {
        if (moveSpeed > movementThreshold)
        {
            cycle += Time.deltaTime * walkCycleSpeed * moveSpeed;
        }
    }

    private void AnimateFeet()
    {
        // RETURN TO IDLE
        if (moveSpeed <= movementThreshold)
        {
            leftFootTarget.localPosition =
                Vector3.Lerp(
                    leftFootTarget.localPosition,
                    leftRestLocalPos,
                    Time.deltaTime * idleResetSpeed
                );

            rightFootTarget.localPosition =
                Vector3.Lerp(
                    rightFootTarget.localPosition,
                    rightRestLocalPos,
                    Time.deltaTime * idleResetSpeed
                );

            leftFootTarget.localRotation =
                Quaternion.Slerp(
                    leftFootTarget.localRotation,
                    leftRestLocalRot,
                    Time.deltaTime * idleResetSpeed
                );

            rightFootTarget.localRotation =
                Quaternion.Slerp(
                    rightFootTarget.localRotation,
                    rightRestLocalRot,
                    Time.deltaTime * idleResetSpeed
                );

            return;
        }

        float leftPhase = Mathf.Sin(cycle);
        float rightPhase = Mathf.Sin(cycle + Mathf.PI);

        Vector3 leftOffset =
            (-body.right * footSpacing) +
            (body.forward * (leftPhase * strideLength));

        Vector3 rightOffset =
            (body.right * footSpacing) +
            (body.forward * (rightPhase * strideLength));

        Vector3 leftPos = body.position + leftOffset;
        Vector3 rightPos = body.position + rightOffset;

        // Foot lift
        leftPos.y += Mathf.Max(0f, leftPhase) * stepHeight;
        rightPos.y += Mathf.Max(0f, rightPhase) * stepHeight;

        leftFootTarget.position = leftPos;
        rightFootTarget.position = rightPos;

        // Optional subtle foot pitch
        float footPitchAmount = 12f;

        Quaternion leftRot =
            Quaternion.Euler(
                leftPhase * footPitchAmount,
                0f,
                0f
            ) * leftRestLocalRot;

        Quaternion rightRot =
            Quaternion.Euler(
                rightPhase * footPitchAmount,
                0f,
                0f
            ) * rightRestLocalRot;

        leftFootTarget.localRotation = leftRot;
        rightFootTarget.localRotation = rightRot;
    }

    private void AnimatePelvis()
    {
        if (pelvis == null)
            return;

        float speedFactor =
            Mathf.Clamp01(moveSpeed * pelvisSpeedMultiplier);

        // Vertical bounce
        float bounce =
            Mathf.Sin(cycle * 2f) *
            pelvisBounceStrength *
            speedFactor;

        // Side-to-side sway
        float sway =
            Mathf.Sin(cycle) *
            pelvisSideStrength *
            speedFactor;

        Vector3 offset =
            Vector3.up * (pelvisHeightOffset + bounce) +
            body.right * sway;

        pelvis.localPosition =
            pelvisStartLocalPos + offset;
    }

    private void UpdateHints()
    {
        if (leftHint != null)
        {
            leftHint.position =
                leftFootTarget.position +
                Vector3.up * hintHeight +
                body.forward * hintForwardOffset;
        }

        if (rightHint != null)
        {
            rightHint.position =
                rightFootTarget.position +
                Vector3.up * hintHeight +
                body.forward * hintForwardOffset;
        }
    }
}