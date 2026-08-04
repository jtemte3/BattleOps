using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[CreateAssetMenu(menuName = "Scriptable Objects/Profiles/NewGunProfile")]
public class GunProfile : WeaponProfile
{
    [Header("Gun Details")]
    public List<ShootingModes> supportedModes;

    public int maxMagazines;
    public int magazineSize;
    public float fireRate;
    public float reloadTime;
    public Sprite ammoIcon;

    public Vector3 muzzleLocalPosition;
    public Vector3 muzzleLocalRotation;
    public VisualEffectAsset muzzleEffectAsset;
    public float muzzleLightDuration;

    public GameObject bulletPrefab;
    public float bulletSpeed;

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
