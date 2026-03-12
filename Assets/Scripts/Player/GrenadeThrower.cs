using UnityEngine;

public class GrenadeThrower : MonoBehaviour
{
    [Header("References")]
    public ControlSchemeManager controls;
    public PlayerController playerController;
    public HandheldGrenade profile;
    public Transform target;
    public Camera cam;

    [Header("Throw")]
    public float maxThrowRange = 100f;
    public float minThrowRange = 25f;
    public float maxThrowForce = 25f;
    public float minThrowForce = 10f;
    public float upwardBoost = 0.1f;

    GameObject currentGrenade;
    Grenade grenadeScript;

    float cookTimer;
    bool cooking;
    float actualFuse;
    public bool isRecharging = false;
    public float rechargeTime = 1f;
    float rechargeTimer;

    public bool canThrow = true;

    void Update()
    {
        if (playerController.GetMountStatus() == true)
        {
            canThrow = false;
        }
        else
        {
            canThrow = true;
        }

        if (isRecharging)
        {
            rechargeTimer += Time.deltaTime;

            if (rechargeTimer >= rechargeTime)
            {
                rechargeTimer = 0;
                profile.previewObj.SetActive(true);
                isRecharging = false;
            }
        }
        else
        {
            if (canThrow)
            {
                HandleInput();
            }
        }
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(controls.weaponFire))
        {
            StartCooking();
        }

        if (Input.GetKeyUp(controls.weaponFire))
        {
            ThrowGrenade();
        }

        if (cooking)
        {
            cookTimer += Time.deltaTime;

            if (cookTimer >= actualFuse)
            {
                // Explode in hand
                grenadeScript.Arm(0);
            }
        }
    }

    void StartCooking()
    {
        if (currentGrenade != null) return;

        cookTimer = 0f;
        cooking = true;

        actualFuse = profile.maxFuse - Random.Range(0, profile.fuseVariation);

        profile.previewObj.SetActive(false);

        currentGrenade = Instantiate(profile.prefab, profile.spawner.transform.position, profile.spawner.transform.rotation, profile.spawner.transform);
        currentGrenade.SetActive(true);

        Rigidbody rb = currentGrenade.GetComponent<Rigidbody>();
        rb.isKinematic = true;

        grenadeScript = currentGrenade.GetComponent<Grenade>();
    }

    void ThrowGrenade()
    {
        if (!cooking) return;

        cooking = false;

        currentGrenade.transform.parent = null;

        Rigidbody rb = currentGrenade.GetComponent<Rigidbody>();
        rb.isKinematic = false;

        (Vector3 throwDirection, float throwForce) = GetAimDetails();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector3 finalDir = throwDirection + Vector3.up * upwardBoost;

        rb.AddForce(finalDir * throwForce, ForceMode.VelocityChange);

        float remainingFuse = actualFuse - cookTimer;

        grenadeScript.Arm(remainingFuse);

        currentGrenade = null;
        grenadeScript = null;
        isRecharging = true;
    }

    (Vector3 direction, float throwForce) GetAimDetails()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(.5f, .5f));
        Vector3 dir = new Vector3();
        float force;

        if (Physics.Raycast(ray, out RaycastHit hit, maxThrowRange))
        {
            dir = (hit.point - profile.spawner.transform.position).normalized;
            force = GetThrowForce(hit.point, profile.spawner.transform.position);
            return (dir, force);
        }

        dir = (target.position - profile.spawner.transform.position).normalized;
        force = GetThrowForce(target.position, profile.spawner.transform.position);

        return (dir, force);

    }

    float GetThrowForce(Vector3 target, Vector3 spawner)
    {
        float force;

        float distance = Vector3.Distance(target, spawner);

        float percentage = Mathf.Max(distance / 10, minThrowRange/100);

        if (percentage < 1)
        {
            force = Mathf.Max(maxThrowForce * percentage, minThrowForce);
        }
        else
        {
            force = maxThrowForce;
        }

        return force;
    }
}
