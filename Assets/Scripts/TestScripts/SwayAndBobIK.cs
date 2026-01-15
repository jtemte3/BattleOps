using UnityEngine;
using UnityEngine.Animations.Rigging;

public class SwayAndBobIK : MonoBehaviour
{
    [Header("Dependancies")]
    public PlayerController playerController;
    public MultiAimConstraint aimConstraint;
    public MultiPositionConstraint positionConstraint;
    public GunProfile currentProfile;

    [Header("Sway Settings")]
    public float strength = 0.05f; //multiplied by the value of camera movement (mouse) for each frame
    public float maxDistance = 0.125f; //maximum distance allowed to sway
    public float maxRotation = 0.125f; //maximum distance allowed to sway
    Vector3 swayPosition; //The result of the sway calculation
    Vector3 swayEulerRotation; //The result of the sway calculation

    [Header("Bob Settings")]
    public float amplitude = .05f; // Wave amplitude (how high/low the object moves)
    public float frequency = 1f; // Wave frequency (speed of the oscillation)
    public float playerSpeedScale = .01f;
    public Vector3 directionalMultiplier = new Vector3(.25f, .25f, .25f);
    //public Vector3 offset = Vector3.zero; // Starting position offset
    public Vector3 bobPosition;

    [Header("General Settings")]
    public float smoothRate = 10f;
    public float smoothRotationRate = 12f;
    //public Vector3 idlePositionOffset;
    //public Vector3 adsPositionOffset;
    Vector3 positionOffset;

    // Start is called before the first frame update
    void Start()
    {
        positionOffset = currentProfile.idleCenter;
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
        Vector3 invertedLookInput = playerController.GetLookInput() * -strength;
        Vector3 invertedLookInputRotation = invertedLookInput;

        //Limit how far the gameonject can sway
        invertedLookInput.x = Mathf.Clamp(invertedLookInput.x, -maxDistance, maxDistance);
        invertedLookInput.y = Mathf.Clamp(invertedLookInput.y, -maxDistance, maxDistance);

        swayPosition = invertedLookInput;

        invertedLookInputRotation.x = Mathf.Clamp(invertedLookInputRotation.x, -maxRotation, maxRotation);
        invertedLookInputRotation.y = Mathf.Clamp(invertedLookInputRotation.y, -maxRotation, maxRotation);

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
                Mathf.Cos(Time.time * (2 * frequency) * (playerController.walkingSpeed * playerSpeedScale)) * (amplitude * directionalMultiplier.x * (playerController.walkingSpeed * playerSpeedScale)) / 2,
                Mathf.Sin(Time.time * -frequency * (playerController.walkingSpeed * playerSpeedScale)) * (amplitude * directionalMultiplier.y * (playerController.walkingSpeed * playerSpeedScale)),
                0);
                break;

            case ("sprinting"):
                bobPosition = new Vector3(
                Mathf.Cos(Time.time * (2 * frequency) * (playerController.runningSpeed * playerSpeedScale)) * (amplitude * directionalMultiplier.x * (playerController.runningSpeed * playerSpeedScale)) / 2,
                Mathf.Sin(Time.time * -frequency * (playerController.runningSpeed * playerSpeedScale)) * (amplitude * directionalMultiplier.y * (playerController.runningSpeed * playerSpeedScale)),
                0);
                break;
        }

    }

    void ApplyMotion()
    {
        positionConstraint.data.offset = Vector3.Lerp(positionConstraint.data.offset, swayPosition + bobPosition + positionOffset, Time.deltaTime * smoothRate);
        aimConstraint.data.offset = Vector3.Lerp(aimConstraint.data.offset, swayEulerRotation, Time.deltaTime * smoothRotationRate);
    }

    public void SetOffsetType(string type)
    {
        switch (type)
        {
            case "idleCenter":
                    positionOffset = currentProfile.idleCenter;
                    break;
            case "idleLeanRight":
                positionOffset = currentProfile.idleLeanRight;
                break;
            case "idleLeanLeft":
                positionOffset = currentProfile.idleLeanLeft;
                break;
            case "adsCenter":
                positionOffset = currentProfile.adsCenter;
                break;
            case "adsLeanRight":
                positionOffset = currentProfile.adsLeanRight;
                break;
            case "adsLeanLeft":
                positionOffset = currentProfile.adsLeanLeft;
                break;
            case "default":
                positionOffset = currentProfile.idleCenter;
                break;
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
