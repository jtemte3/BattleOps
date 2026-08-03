using UnityEngine;
using UnityEngine.VFX;

public class TargetedGunFire : MonoBehaviour
{
    public ControlSchemeManager controlScheme;
    public PlayerController playerController;
    public Transform target;

    public RecoilControllerIK recoilController;
    public GunProfile gunProfile;
    public GameObject muzzleObj;

    public bool canShoot = true;
    private float nextFireTime = 0f;
    private float lightOffTime = 0f;

    public ShootingModes mode = ShootingModes.fullAuto;

    private void Start()
    {
        nextFireTime = Time.time;
    }

    void Update()
    {
        if (playerController.GetMountStatus())
        {
            canShoot = false;
        }

        if (Input.GetKeyDown(controlScheme.weaponMode))
        {
            int currentMode = gunProfile.supportedModes.IndexOf(mode);

            int nextMode = currentMode + 1;

            if (nextMode > gunProfile.supportedModes.Count - 1)
            {
                nextMode = 0;
            }

            mode = gunProfile.supportedModes[nextMode];

            if(mode == ShootingModes.semiAuto)
            {
                recoilController.isFullAuto = false;
            }
            else
            {
                recoilController.isFullAuto = true;
            }
        }

        if (canShoot)
        {
            if (mode == ShootingModes.semiAuto)
            {
                SemiAutoMode();
            }
            if (mode == ShootingModes.burst)
            {
                BurstMode();
            }
            if (mode == ShootingModes.fullAuto)
            {
                FullAutoMode();
            }
        }

        if (muzzleObj.GetComponent<Light>().enabled)
        {
            if (Time.time >= lightOffTime)
            {
                muzzleObj.GetComponent<Light>().enabled = false;
            }
        }
    }

    private void FullAutoMode()
    {
        bool isFiring = Input.GetKey(controlScheme.weaponFire);

        /*if (playerController.GetMovementState() != "sprinting" *//*&& clipManager.isClipped != true*//*)
        {
            // Update recoil scaling
            recoilController.isFiring = isFiring;

            if (isFiring && Time.time >= nextFireTime)
            {
                FireWeapon();
                nextFireTime = Time.time + 1f / currentProfile.fireRate;
            }
        }*/

        if (isFiring && Time.time >= nextFireTime)
        {
            // Update recoil scaling
            recoilController.isFiring = isFiring;

            FireWeapon();
            nextFireTime = Time.time + 1f / gunProfile.fireRate;
        }
    }

    private void BurstMode()
    {
        bool isFiring = Input.GetKeyDown(controlScheme.weaponFire);

/*        if (playerController.GetMovementState() != "sprinting" *//*&& clipManager.isClipped != true*//*)
        {
            // Update recoil scaling
            recoilController.isFiring = isFiring;

            if (isFiring && Time.time >= nextFireTime)
            {
                FireWeapon();
                nextFireTime = Time.time + 1f / currentProfile.fireRate;
            }
        }*/

        if (isFiring && Time.time >= nextFireTime)
        {
            // Update recoil scaling
            recoilController.isFiring = isFiring;

            FireWeapon();
            nextFireTime = Time.time + 1f / gunProfile.fireRate;
        }
    }

    private void SemiAutoMode()
    {
        bool isFiring = Input.GetKeyDown(controlScheme.weaponFire);

/*        if (playerController.GetMovementState() != "sprinting" *//*&& clipManager.isClipped != true*//*)
        {
            // Update recoil scaling
            recoilController.isFiring = isFiring;

            if (isFiring && Time.time >= nextFireTime)
            {
                FireWeapon();
                nextFireTime = Time.time + 1f / currentProfile.fireRate;
            }
        }*/

        if (isFiring && Time.time >= nextFireTime)
        {
            // Update recoil scaling
            recoilController.isFiring = isFiring;

            FireWeapon();
            nextFireTime = Time.time + 1f / gunProfile.fireRate;
        }
    }

    void FireWeapon()
    {
        recoilController.ApplyRecoil();

        GameObject bullet = Instantiate(gunProfile.bulletPrefab, muzzleObj.transform.position, muzzleObj.transform.rotation);
        bullet.transform.parent = null;
        bullet.GetComponent<Rigidbody>().linearVelocity = (target.transform.position - muzzleObj.transform.position).normalized * gunProfile.bulletSpeed;

        bullet.GetComponent<BulletData>().team = AITeam.Player;

        muzzleObj.GetComponent<VisualEffect>().Play();
        muzzleObj.GetComponent<Light>().enabled = true;
        lightOffTime = Time.time + gunProfile.muzzleLightDuration;
    }
}
