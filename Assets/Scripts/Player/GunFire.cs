using UnityEngine;

public class GunFire : MonoBehaviour
{
    public ControlSchemeManager controlScheme;
    public JetpackPlayerController playerController;
    public Camera mainCamera;
    public ClipManager clipManager;
    public RecoilController recoilController;
    public float fireRate = 10f; // bullets per second
    public GameObject gunMuzzle;
    public GameObject bulletPrefab;
    public float bulletSpeed;
    public float sightRange = 500f;

    private float nextFireTime = 0f;

    private void Start()
    {
        nextFireTime = Time.time;
    }

    void Update()
    {
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

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
        //RaycastHit hit;

        Vector3 targetPoint;
        targetPoint = ray.GetPoint(sightRange);

        GameObject bullet = Instantiate(bulletPrefab, gunMuzzle.transform.position, gunMuzzle.transform.rotation);
        bullet.transform.parent = null;
        bullet.GetComponent<Rigidbody>().linearVelocity = (targetPoint - gunMuzzle.transform.position).normalized * bulletSpeed;
    }
}