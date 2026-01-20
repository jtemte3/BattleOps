using UnityEngine;
using UnityEngine.VFX;

public class TargetedGunFire : MonoBehaviour
{
    public ControlSchemeManager controlScheme;
    public PlayerController playerController;
    public Transform target;

    public RecoilControllerIK recoilController;
    public HandheldObject gunObj;

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
            int currentMode = gunObj.profile.supportedModes.IndexOf(mode);

            int nextMode = currentMode + 1;

            if (nextMode > gunObj.profile.supportedModes.Count - 1)
            {
                nextMode = 0;
            }

            mode = gunObj.profile.supportedModes[nextMode];

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

        if (gunObj.muzzleObj.GetComponent<Light>().enabled)
        {
            if (Time.time >= lightOffTime)
            {
                gunObj.muzzleObj.GetComponent<Light>().enabled = false;
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
            nextFireTime = Time.time + 1f / gunObj.profile.fireRate;
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
            nextFireTime = Time.time + 1f / gunObj.profile.fireRate;
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
            nextFireTime = Time.time + 1f / gunObj.profile.fireRate;
        }
    }

    void FireWeapon()
    {
        recoilController.ApplyRecoil();

        GameObject bullet = Instantiate(gunObj.profile.bulletPrefab, gunObj.muzzleObj.transform.position, gunObj.muzzleObj.transform.rotation);
        bullet.transform.parent = null;
        bullet.GetComponent<Rigidbody>().linearVelocity = (target.transform.position - gunObj.muzzleObj.transform.position).normalized * gunObj.profile.bulletSpeed;

        gunObj.muzzleObj.GetComponent<VisualEffect>().Play();
        gunObj.muzzleObj.GetComponent<Light>().enabled = true;
        lightOffTime = Time.time + gunObj.profile.muzzleLightDuration;
    }
}
