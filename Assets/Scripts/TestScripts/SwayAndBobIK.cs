using UnityEngine;
using UnityEngine.Animations.Rigging;

public class SwayAndBobIK : MonoBehaviour
{
    [Header("Dependancies")]
    public PlayerController playerController;
    public MultiAimConstraint aimConstraint;
    public MultiPositionConstraint positionConstraint;

    [Header("Sway Settings")]
    public float swayStrength = 0.01f; //multiplied by the value of camera movement (mouse) for each frame
    public float swayMaxDistance = 0.06f; //maximum distance allowed to sway
    public float swayMaxRotation = 0.06f; //maximum distance allowed to sway
    Vector3 swayPosition; //The result of the sway calculation
    Vector3 swayEulerRotation; //The result of the sway calculation

    [Header("Bob Settings")]
    public float amplitude = 1f; // Wave amplitude (how high/low the object moves)
    public float frequency = 1f; // Wave frequency (speed of the oscillation)
    public float playerSpeedScale = .01f;
    public Vector3 directionalMultiplier = new Vector3(.25f, .25f, .25f);
    //public Vector3 offset = Vector3.zero; // Starting position offset
    public Vector3 bobPosition;

    [Header("General Settings")]
    public float smoothRate = 10f;
    public float smoothRotationRate = 12f;
    public Vector3 idlePositionOffset;
    public Vector3 adsPositionOffset;
    Vector3 positionOffset;

    // Start is called before the first frame update
    void Start()
    {
        positionOffset = idlePositionOffset;
        positionConstraint.data.offset = positionOffset;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //SetOffset();
        Sway();
        Bob();
        ApplyMotion();
    }

    void Sway() //The X,Y,Z position change of the gameobject depending on the mouse movement
    {
        //Invert the movement because your gameobject is lagging behind where your new camera direction is looking
        Vector3 invertedLookInput = playerController.GetLookInput() * -swayStrength;
        Vector3 invertedLookInputRotation = invertedLookInput;

        //Limit how far the gameonject can sway
        invertedLookInput.x = Mathf.Clamp(invertedLookInput.x, -swayMaxDistance, swayMaxDistance);
        invertedLookInput.y = Mathf.Clamp(invertedLookInput.y, -swayMaxDistance, swayMaxDistance);

        swayPosition = invertedLookInput;

        invertedLookInputRotation.x = Mathf.Clamp(invertedLookInputRotation.x, -swayMaxRotation, swayMaxRotation);
        invertedLookInputRotation.y = Mathf.Clamp(invertedLookInputRotation.y, -swayMaxRotation, swayMaxRotation);

        swayEulerRotation = new Vector3(invertedLookInputRotation.y, invertedLookInputRotation.x, invertedLookInputRotation.x);
    }

    void Bob()
    {
        switch (playerController.GetMovementState())
        {
            case ("idle"):
                bobPosition = new Vector3(
                Mathf.Cos(Time.time * (2 * frequency)) * (amplitude * directionalMultiplier.x) / 2,
                Mathf.Sin(Time.time * -frequency) * (amplitude * directionalMultiplier.y),
                0);
                break;

            case ("walking"):
                bobPosition = new Vector3(
                Mathf.Cos(Time.time * (2 * frequency) * (playerController.speedWalking * playerSpeedScale)) * (amplitude * directionalMultiplier.x * (playerController.speedWalking * playerSpeedScale)) / 2,
                Mathf.Sin(Time.time * -frequency * (playerController.speedWalking * playerSpeedScale)) * (amplitude * directionalMultiplier.y * (playerController.speedWalking * playerSpeedScale)),
                0);
                break;

            case ("sprinting"):
                bobPosition = new Vector3(
                Mathf.Cos(Time.time * (2 * frequency) * (playerController.speedRunning * playerSpeedScale)) * (amplitude * directionalMultiplier.x * (playerController.speedRunning * playerSpeedScale)) / 2,
                Mathf.Sin(Time.time * -frequency * (playerController.speedRunning * playerSpeedScale)) * (amplitude * directionalMultiplier.y * (playerController.speedRunning * playerSpeedScale)),
                0);
                break;
        }

    }

    void ApplyMotion()
    {
        positionConstraint.data.offset = Vector3.Lerp(positionConstraint.data.offset, swayPosition + bobPosition + positionOffset, Time.deltaTime * smoothRate);
        aimConstraint.data.offset = Vector3.Lerp(aimConstraint.data.offset, swayEulerRotation, Time.deltaTime * smoothRotationRate);
    }

    public void SetAdsOffset(bool adsState)
    {
        if (adsState == true)
        {
            positionOffset = adsPositionOffset;
        }
        else
        {
            positionOffset = idlePositionOffset;
        }
    }

    public Vector3 GetPositionOffset()
    {
        return positionOffset;
    }
    public Vector3 GetSwayPosition()
    {
        return swayPosition;
    }

    public Vector3 GetBobPosition()
    {
        return bobPosition;
    }

    public Vector3 GetSwayRotation()
    {
        return swayEulerRotation;
    }
}
