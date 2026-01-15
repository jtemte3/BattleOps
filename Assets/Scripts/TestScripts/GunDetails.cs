using UnityEngine;
using UnityEngine.VFX;

public class GunDetails : MonoBehaviour
{
    [Header("Weapon Options")]
    public string weaponName;
    public enum WeaponType { Meele, SemiAuto, Burst, FullAuto, Throwable, Mounted, Handheld };
    public WeaponType weaponType;
    public int magazineSize;
    public float fireRate;
    public float reloadTime;
    public GameObject muzzleObj;
    public Light muzzleLight;
    public VisualEffect muzzleEffect;
    public float muzzleLightDuration;
    public GameObject bulletPrefab;
    public float bulletSpeed;

    [Header("IK Details")]
    public GameObject leftHandIKPos;
    public GameObject rightHandIKPos;
}
