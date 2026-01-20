using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.VFX;

[CreateAssetMenu(menuName = "Scriptable Objects/Profiles/NewWeaponProfile")]
public class GunProfile : ScriptableObject
{
    [Header("Gun Details")]
    public string weaponName;
    public List<ShootingModes> supportedModes;

    public int magazineSize;
    public float fireRate;
    public float reloadTime;

    public Vector3 muzzleLocalPosition;
    public Vector3 muzzleLocalRotation;
    public VisualEffectAsset muzzleEffectAsset;
    public float muzzleLightDuration;

    public GameObject bulletPrefab;
    public float bulletSpeed;

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

    [Header("Camera Recoil Settings")]
    public Vector2 cameraRecoilKick;
    public float cameraReturnSpeed;
    public float cameraSnappiness;
    public float cameraRecoilScale;
    public float adsCameraRecoilScale;

    [Header("Weapon Recoil Settings")]
    public Vector3 weaponRecoilKick;
    public float weaponReturnSpeed;
    public float weaponSnappiness;
    [Range(.001f, 1)]
    public float minRecoilScale;
    [Range(.001f, 1)]
    public float maxRecoilScale;
    public float adsAcumulationSpeed;
    public float acumulationSpeed;
    public float cooldownSpeed;
}
