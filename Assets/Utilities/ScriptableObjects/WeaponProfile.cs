using UnityEngine;


public class WeaponProfile : ScriptableObject
{
    public string weaponName;

    [Header("IK Details")]
    public Vector3 rightHandPosition;
    public Vector3 rightHandRotation;
    public Vector3 leftHandPosition;
    public Vector3 leftHandRotation;

    [Header("Position Offsets")]
    public Vector3 idleCenter;
    public Vector3 idleLeanRight;
    public Vector3 idleLeanLeft;
    public Vector3 adsCenter;
    public Vector3 adsLeanRight;
    public Vector3 adsLeanLeft;
}
