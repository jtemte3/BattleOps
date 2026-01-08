using UnityEngine;

public class GunAnimator : MonoBehaviour
{
    public ControlSchemeManager controlScheme;
    public TargetedGunFire gunScript;
    public SwayAndBobIK swayAndBobIK;
    public ADSManager adsManager;
    public Animator animator;

    public float reloadTime = 2.668f;
    private float resetTime = 0;

    // Update is called once per frame
    void Update()
    {
        if (gunScript.canShoot != true)
        {
            if (Time.time > resetTime)
            {
                gunScript.canShoot = true;
            }
        }

        if (Input.GetKeyDown(controlScheme.weaponReload))
        {
            gunScript.canShoot = false;
            animator.SetTrigger("reload");

            resetTime = Time.time + reloadTime;
        }

        bool isAds = Input.GetKey(controlScheme.weaponAimDownSights);

        if (isAds)
        {
            animator.SetBool("ads", true);
            adsManager.SetAdsState(true);
            swayAndBobIK.SetAdsOffset(true);
        }
        else
        {
            animator.SetBool("ads", false);
            adsManager.SetAdsState(false);
            swayAndBobIK.SetAdsOffset(false);
        }
    }
}
