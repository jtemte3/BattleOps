using UnityEngine;
using UnityEngine.Animations.Rigging;

public class RecoilControllerIK : MonoBehaviour
{
    [Header("Dependancies")]
    public SwayAndBobIK swayController;
    public GameObject cameraParent;
    public MultiPositionConstraint weaponPositionConstraint;
    public MultiAimConstraint headAimConstraint;
    public GunProfile currentProfile;

    [Header("Camera Recoil Settings")]
    public bool enableCameraRecoil = true;
    //public Vector2 cameraRecoilKick = new Vector2(2f, 1f); // (X: vertical, Y: horizontal)
    //public float cameraReturnSpeed = 5f;
    //public float cameraSnappiness = 10f;
    //public float cameraRecoilScale = 5f;

    [Header("Weapon Recoil Settings")]
    public bool enableWeaponRecoil = true;
    //public Vector3 weaponRecoilKick = new Vector3(0f, 0f, -0.1f);
    //public float weaponReturnSpeed = 10f;
    //public float weaponSnappiness = 25f;

    [Header("Recoil Scaling")]
    private float recoilMultiplier;
    public float semiAutoScale = 2f;
    public float adsScale = .75f;
    //public float recoilIncreaseSpeed = 2f;
    //public float recoilDecreaseSpeed = 1.5f;
    //public float maxRecoilMultiplier = 2f;

    private Vector2 currentCameraRecoil;
    private Vector2 targetCameraRecoil;
    public Vector3 currentWeaponRecoil;
    public Vector3 targetWeaponRecoil;
    public bool isFiring;
    public bool resetRecoil = true;
    public bool isFullAuto = true;
    public bool isAds = false;

    public void FixedUpdate()
    {
        if (isFiring)
        {
            if (isFullAuto)
            {
                if (isAds)
                {
                    recoilMultiplier = Mathf.MoveTowards(recoilMultiplier, (currentProfile.maxRecoilScale * adsScale), currentProfile.adsAcumulationSpeed * Time.deltaTime);
                }
                else
                {
                    recoilMultiplier = Mathf.MoveTowards(recoilMultiplier, currentProfile.maxRecoilScale, currentProfile.acumulationSpeed * Time.deltaTime);
                }
            }
            else
            {
                if (isAds)
                {
                    recoilMultiplier = currentProfile.minRecoilScale * (semiAutoScale * adsScale);
                }
                else
                {
                    recoilMultiplier = currentProfile.minRecoilScale * semiAutoScale;
                }
            }
            

            resetRecoil = true;
        }
        else
        {
            recoilMultiplier = Mathf.MoveTowards(recoilMultiplier, currentProfile.minRecoilScale, currentProfile.cooldownSpeed * Time.deltaTime);
        }

        if (resetRecoil)
        {
            //Calculate next recoil offset
            float vertical = Random.Range(currentProfile.cameraRecoilKick.x * 0.8f, currentProfile.cameraRecoilKick.x * 1.2f) * recoilMultiplier;
            float horizontal = Random.Range(-currentProfile.cameraRecoilKick.y, currentProfile.cameraRecoilKick.y) * recoilMultiplier;
            targetCameraRecoil += new Vector2(vertical, horizontal);
            targetWeaponRecoil += currentProfile.weaponRecoilKick * recoilMultiplier;

            resetRecoil = false;
        }

        // Smooth towards target recoil
        targetCameraRecoil = Vector2.Lerp(targetCameraRecoil, swayController.GetSwayRotation(), currentProfile.cameraReturnSpeed * Time.deltaTime);
        currentCameraRecoil = Vector2.Lerp(currentCameraRecoil, targetCameraRecoil, currentProfile.cameraSnappiness * Time.deltaTime);

        // Smoothly reduce target recoil back to resting position
        targetWeaponRecoil = Vector3.Lerp(targetWeaponRecoil, swayController.GetPositionOffset(), currentProfile.weaponReturnSpeed * Time.deltaTime);
        currentWeaponRecoil = Vector3.Lerp(targetWeaponRecoil, targetWeaponRecoil, currentProfile.weaponSnappiness * Time.deltaTime);
    }
    public void ApplyRecoil()
    {
        if (enableCameraRecoil)
        {
            // Apply camera recoil rotation
            //cameraParent.transform.localRotation = Quaternion.Euler((-currentCameraRecoil.x + swayController.GetSwayRotation().x) / cameraRecoilScale, (currentCameraRecoil.y + swayController.GetSwayRotation().y) / cameraRecoilScale, (swayController.GetSwayRotation().z) / cameraRecoilScale);
            headAimConstraint.data.offset = new Vector3((-currentCameraRecoil.x + swayController.GetSwayRotation().x) / currentProfile.cameraRecoilScale, (currentCameraRecoil.y + swayController.GetSwayRotation().y) / currentProfile.cameraRecoilScale, (swayController.GetSwayRotation().z) / currentProfile.cameraRecoilScale);
        }

        if (enableWeaponRecoil)
        {
            // Apply to weapon holder position, account for bob and sway changes
            weaponPositionConstraint.data.offset = swayController.GetBobPosition() + swayController.GetSwayPosition() + currentWeaponRecoil;
        }
    }
}