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
