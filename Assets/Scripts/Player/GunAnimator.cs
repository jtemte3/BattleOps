using UnityEngine;

public class GunAnimator : MonoBehaviour
{
    public ControlSchemeManager controlScheme;
    public TargetedGunFire gunScript;
    public Animator animator;
    public GunProfile currentProfile;
    [Header("Gun Clipping Setup")]
    public Camera cam;
    public float distance;
    public LayerMask excludedLayers;
    public bool isClipped;
    public bool isReloading;
    public float unclipTime = 0;


    private float resetTime = 0;

    // Update is called once per frame
    void Update()
    {
        if (gunScript.canShoot != true && isClipped == false && isReloading == false)
        {
            if (Time.time > resetTime)
            {
                gunScript.canShoot = true;
            }
        }

        if (gunScript.canShoot != true && isReloading == true)
        {
            if (Time.time > resetTime)
            {
                gunScript.canShoot = true;
                isReloading = false;
                gunScript.handheldGun.ReloadWeapon();
            }
        }

        if (Input.GetKeyDown(controlScheme.weaponReload) && gunScript.handheldGun.clipCount > 0 && !isReloading)
        {
            gunScript.canShoot = false;
            isReloading = true;
            animator.SetTrigger("reload");

            resetTime = Time.time + currentProfile.reloadTime;
        }
    }

    void FixedUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, distance, ~excludedLayers))
        {
            isClipped = true;
            gunScript.canShoot = false;
            animator.SetBool("clipped", true);
        }
        else
        {
            if (isClipped)
            {
                resetTime = Time.time + unclipTime;
            }

            isClipped = false;
            animator.SetBool("clipped", false);
        }
    }
}
