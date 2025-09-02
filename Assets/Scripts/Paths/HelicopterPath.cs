using UnityEngine;
using System.Collections.Generic;

public class HelicopterPath : MonoBehaviour
{
    public Transform helicopter;
    public Vector3 startPos;
    public Vector3 landingPos;
    public float approachHeight = 40f;
    public float hoverHeight = 15f;
    public float approachSpeed = 40f;
    public float circleSpeed = 30f;
    public float descentSpeed = 10f;
    public float curveOffset = 100f;
    public bool curveInward = true; // If false, curves outward

    public float turnTiltAmount = 15f; // Max tilt degrees
    public float tiltSmooth = 5f;

    private List<Vector3> pathPoints;
    //private int currentPointIndex;
    private Vector3 velocity;
    private Quaternion targetRotation;

    private enum FlightStage { Approach, Circle, Descent }
    private FlightStage stage;
    private float circleAngle;
    private Vector3 circleCenter;
    private float circleRadius;
    private int circleDirection;

    void Start()
    {
        GeneratePath();
    }

    void GeneratePath()
    {
        pathPoints = new List<Vector3>();

        // Direction from start to landing
        Vector3 flatDir = (landingPos - startPos);
        flatDir.y = 0;
        flatDir.Normalize();

        // Curve direction (left or right)
        Vector3 sideOffset = curveInward ? Vector3.Cross(Vector3.up, flatDir) : Vector3.Cross(flatDir, Vector3.up);

        // Entry point offset for approach arc
        Vector3 offsetPos = landingPos + sideOffset * curveOffset;
        offsetPos.y = approachHeight;

        // Tangent entry point for circle
        circleRadius = Vector3.Distance(offsetPos, landingPos);
        circleCenter = new Vector3(landingPos.x, hoverHeight, landingPos.z);
        circleDirection = curveInward ? 1 : -1;

        // Start state
        helicopter.position = startPos;
        helicopter.position = new Vector3(helicopter.position.x, approachHeight, helicopter.position.z);

        stage = FlightStage.Approach;
        //currentPointIndex = 0;
        velocity = Vector3.zero;
    }

    void Update()
    {
        float dt = Time.deltaTime;

        switch (stage)
        {
            case FlightStage.Approach:
                Vector3 approachTarget = new Vector3(circleCenter.x + Mathf.Cos(Mathf.PI / 2 * -circleDirection) * circleRadius,
                                                     hoverHeight,
                                                     circleCenter.z + Mathf.Sin(Mathf.PI / 2 * -circleDirection) * circleRadius);
                MoveTowards(approachTarget, approachSpeed, dt);

                if (Vector3.Distance(helicopter.position, approachTarget) < 5f && velocity.magnitude < 5f)
                {
                    stage = FlightStage.Circle;
                    circleAngle = Mathf.Atan2(helicopter.position.z - circleCenter.z,
                                              helicopter.position.x - circleCenter.x);
                }
                break;

            case FlightStage.Circle:
                circleAngle += circleDirection * (circleSpeed / circleRadius) * dt;
                Vector3 circlePos = new Vector3(
                    circleCenter.x + Mathf.Cos(circleAngle) * circleRadius,
                    hoverHeight,
                    circleCenter.z + Mathf.Sin(circleAngle) * circleRadius
                );

                MoveTowards(circlePos, circleSpeed, dt);

                // Check if we made a full revolution
                if (Mathf.Abs(circleAngle) >= 2 * Mathf.PI)
                {
                    stage = FlightStage.Descent;
                }
                break;

            case FlightStage.Descent:
                Vector3 descentPos = new Vector3(landingPos.x, landingPos.y, landingPos.z);
                MoveTowards(descentPos, descentSpeed, dt);

                if (Vector3.Distance(helicopter.position, landingPos) < 0.5f)
                {
                    velocity = Vector3.zero; // Landed
                }
                break;
        }

        // Tilt during turns
        Vector3 forward = helicopter.forward;
        Vector3 horizontalVel = velocity;
        horizontalVel.y = 0;
        if (horizontalVel.magnitude > 0.1f)
        {
            Vector3 turnDir = Vector3.Cross(forward, horizontalVel.normalized);
            float tilt = Mathf.Clamp(-turnDir.y * turnTiltAmount, -turnTiltAmount, turnTiltAmount);
            targetRotation = Quaternion.LookRotation(horizontalVel.normalized) * Quaternion.Euler(tilt, 0, 0);
        }

        helicopter.rotation = Quaternion.Slerp(helicopter.rotation, targetRotation, tiltSmooth * dt);
    }

    void MoveTowards(Vector3 target, float speed, float dt)
    {
        Vector3 desiredVelocity = (target - helicopter.position).normalized * speed;
        velocity = Vector3.Lerp(velocity, desiredVelocity, dt * 2f); // Smooth acceleration
        helicopter.position += velocity * dt;
    }
}
