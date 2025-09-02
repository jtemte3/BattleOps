using UnityEngine;

public class GunFire : MonoBehaviour
{
    public ControlSchemeManager controlScheme;
    public JetpackPlayerController playerController;
    public ClipManager clipManager;
    public RecoilController recoilController;
    public float fireRate = 10f; // bullets per second

    private float nextFireTime;
    private bool firstPass = true;

    void Update()
    {
        if (firstPass)
        {
            recoilController.ApplyRecoil();
            firstPass = false;
        }

        if (playerController.GetMovementState() != "sprinting" && clipManager.isClipped != true)
        {
            bool isFiring = Input.GetKey(controlScheme.weaponFire);

            // Update recoil scaling
            recoilController.isFiring = isFiring;

            if (isFiring && Time.time >= nextFireTime)
            {
                FireWeapon();
                nextFireTime = Time.time + 1f / fireRate;
            }
        }
    }

    void FireWeapon()
    {
        recoilController.ApplyRecoil();
    }
}