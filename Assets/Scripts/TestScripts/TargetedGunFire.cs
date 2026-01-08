using UnityEngine;
using UnityEngine.VFX;

public class TargetedGunFire : MonoBehaviour
{
    public ControlSchemeManager controlScheme;
    public PlayerController playerController;
    public Transform target;
    //public ClipManager clipManager;
    public RecoilControllerIK recoilController;
    public float fireRate = 10f; // bullets per second
    public GameObject gunMuzzle;
    public VisualEffect muzzleEffect;
    public Light muzzleLight;
    public float muzzleLightDuration;
    public GameObject bulletPrefab;
    public float bulletSpeed;

    public bool canShoot = true;


    private float nextFireTime = 0f;
    private float lightOffTime = 0f;

    private void Start()
    {
        nextFireTime = Time.time;
    }

    void Update()
    {
        if (canShoot)
        {
            bool isFiring = Input.GetKey(controlScheme.weaponFire);

            if (playerController.GetMovementState() != "sprinting" /*&& clipManager.isClipped != true*/)
            {
                // Update recoil scaling
                recoilController.isFiring = isFiring;

                if (isFiring && Time.time >= nextFireTime)
                {
                    FireWeapon();
                    nextFireTime = Time.time + 1f / fireRate;
                }
            }

            if (isFiring && Time.time >= nextFireTime)
            {
                FireWeapon();
                nextFireTime = Time.time + 1f / fireRate;
            }
        }

        if (muzzleLight.enabled)
        {
            if (Time.time >= lightOffTime)
            {
                muzzleLight.enabled = false;
            }
        }
    }

    void FireWeapon()
    {
        recoilController.ApplyRecoil();

        GameObject bullet = Instantiate(bulletPrefab, gunMuzzle.transform.position, gunMuzzle.transform.rotation);
        bullet.transform.parent = null;
        bullet.GetComponent<Rigidbody>().linearVelocity = (target.transform.position - gunMuzzle.transform.position).normalized * bulletSpeed;

        muzzleEffect.Play();
        muzzleLight.enabled = true;
        lightOffTime = Time.time + muzzleLightDuration;
    }
}
