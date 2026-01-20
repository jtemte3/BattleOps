using UnityEngine;

public class GunAnimator : MonoBehaviour
{
    public ControlSchemeManager controlScheme;
    public TargetedGunFire gunScript;
    public RecoilControllerIK recoilController;
    public SwayAndBobIK swayAndBobIK;
    public ADSManager adsManager;
    public Animator animator;
    public GunProfile currentProfile;
    [Header("Gun Clipping Setup")]
    public Camera cam;
    public float distance;
    public LayerMask collisionLayers;
    public bool isClipped;
    public float unclipTime = 0;


    private float resetTime = 0;

    // Update is called once per frame
    void Update()
    {
        if (gunScript.canShoot != true && isClipped == false)
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

            resetTime = Time.time + currentProfile.reloadTime;
        }

        bool isAds = Input.GetKey(controlScheme.weaponAimDownSights);

        if (isAds)
        {
            //animator.SetBool("ads", true);
            adsManager.SetAdsState(true);
            recoilController.isAds = true;

            if (Input.GetKey(controlScheme.leanLeft))
            {
                swayAndBobIK.SetOffsetType("adsLeanLeft");
            }
            else if (Input.GetKey(controlScheme.leanRight))
            {
                swayAndBobIK.SetOffsetType("adsLeanRight");
            }
            else
            {
                swayAndBobIK.SetOffsetType("adsCenter");
            }

        }
        else
        {
            //animator.SetBool("ads", false);
            adsManager.SetAdsState(false);
            recoilController.isAds = false;

            if (Input.GetKey(controlScheme.leanLeft))
            {
                swayAndBobIK.SetOffsetType("idleLeanLeft");
            }
            else if (Input.GetKey(controlScheme.leanRight))
            {
                swayAndBobIK.SetOffsetType("idleLeanRight");
            }
            else
            {
                swayAndBobIK.SetOffsetType("idleCenter");
            }
        }
    }

    void FixedUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, distance))
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
