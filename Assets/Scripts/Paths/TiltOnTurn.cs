using UnityEngine;

public class TiltOnTurn : MonoBehaviour
{
    [Header("Settings")]
    public float maxTiltAngle = 30f;    // Maximum tilt in degrees
    public float tiltSpeed = 5f;        // How fast tilt adjusts
    public float tiltSensitivity = 1f;  // Scale tilt strength vs speed

    private float currentTilt = 0f;

    /// <summary>
    /// Call this every frame with current forward speed and a world-space target position.
    /// </summary>
    public void ApplyTilt(float speed, Vector3 targetPosition)
    {
        Vector3 forward = transform.forward;
        Vector3 toTarget = (targetPosition - transform.position).normalized;

        // Signed angle around Y (horizontal turn direction)
        float angle = Vector3.SignedAngle(forward, toTarget, Vector3.up);

        // Desired tilt proportional to turn angle and speed
        float desiredTilt = Mathf.Clamp(angle * speed * tiltSensitivity / 100f, -maxTiltAngle, maxTiltAngle);

        // Smoothly move current tilt toward desired tilt
        currentTilt = Mathf.Lerp(currentTilt, desiredTilt, Time.deltaTime * tiltSpeed);

        // Apply tilt about local Z (roll axis)
        Quaternion uprightRot = Quaternion.LookRotation(forward, Vector3.up);
        transform.rotation = uprightRot * Quaternion.Euler(0f, 0f, -currentTilt);
    }
}
