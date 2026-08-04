using UnityEngine;
using UnityEngine.VFX;

public class TargetedGunFire : MonoBehaviour
{
    public ControlSchemeManager controlScheme;
    public PlayerController playerController;
    public Transform target;

    public RecoilControllerIK recoilController;
    public HandheldGun handheldGun;

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
            int currentMode = handheldGun.gunProfile.supportedModes.IndexOf(mode);

            int nextMode = currentMode + 1;

            if (nextMode > handheldGun.gunProfile.supportedModes.Count - 1)
            {
                nextMode = 0;
            }

            mode = handheldGun.gunProfile.supportedModes[nextMode];

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

        if (handheldGun.muzzleObj.GetComponent<Light>().enabled)
        {
            if (Time.time >= lightOffTime)
            {
                handheldGun.muzzleObj.GetComponent<Light>().enabled = false;
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
            nextFireTime = Time.time + 1f / handheldGun.gunProfile.fireRate;
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
            nextFireTime = Time.time + 1f / handheldGun.gunProfile.fireRate;
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
            nextFireTime = Time.time + 1f / handheldGun.gunProfile.fireRate;
        }
    }

    void FireWeapon()
    {
        if (handheldGun.ammoCount > 0)
        {
            recoilController.ApplyRecoil();

            GameObject bullet = Instantiate(handheldGun.gunProfile.bulletPrefab, handheldGun.muzzleObj.transform.position, handheldGun.muzzleObj.transform.rotation);
            bullet.transform.parent = null;
            bullet.GetComponent<Rigidbody>().linearVelocity = (target.transform.position - handheldGun.muzzleObj.transform.position).normalized * handheldGun.gunProfile.bulletSpeed;

            bullet.GetComponent<BulletData>().team = AITeam.Player;

            handheldGun.muzzleObj.GetComponent<VisualEffect>().Play();
            handheldGun.muzzleObj.GetComponent<Light>().enabled = true;
            lightOffTime = Time.time + handheldGun.gunProfile.muzzleLightDuration;

            handheldGun.ammoCount--;
        }
        else
        {
            //ToDo Play out of ammo sound here
        }
    }
}
