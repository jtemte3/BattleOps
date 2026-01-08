using UnityEngine;
using UnityEngine.Animations.Rigging;

public class RecoilControllerIK : MonoBehaviour
{
    [Header("Dependancies")]
    public SwayAndBobIK swayController;
    public GameObject cameraParent;
    public MultiPositionConstraint weaponPositionConstraint;

    [Header("Camera Recoil Settings")]
    public bool enableCameraRecoil = true;
    public Vector2 cameraRecoilKick = new Vector2(2f, 1f); // (X: vertical, Y: horizontal)
    public float cameraReturnSpeed = 5f;
    public float cameraSnappiness = 10f;
    public float cameraRecoilScale = 5f;

    [Header("Weapon Recoil Settings")]
    public bool enableWeaponRecoil = true;
    public Vector3 weaponRecoilKick = new Vector3(0f, 0f, -0.1f);
    public float weaponReturnSpeed = 10f;
    public float weaponSnappiness = 25f;

    [Header("Recoil Scaling")]
    public float recoilMultiplier;
    public float minRecoilMultiplier = 1f;
    public float recoilIncreaseSpeed = 2f;
    public float recoilDecreaseSpeed = 1.5f;
    public float maxRecoilMultiplier = 2f;

    private Vector2 currentCameraRecoil;
    private Vector2 targetCameraRecoil;
    public Vector3 currentWeaponRecoil;
    public Vector3 targetWeaponRecoil;
    public bool isFiring;
    public bool resetRecoil = true;

    public void FixedUpdate()
    {
        if (isFiring)
        {
            recoilMultiplier = Mathf.MoveTowards(recoilMultiplier, maxRecoilMultiplier, recoilIncreaseSpeed * Time.deltaTime);

            resetRecoil = true;
        }
        else
        {
            recoilMultiplier = Mathf.MoveTowards(recoilMultiplier, minRecoilMultiplier, recoilDecreaseSpeed * Time.deltaTime);
        }

        if (resetRecoil)
        {
            //Calculate next recoil offset
            float vertical = Random.Range(cameraRecoilKick.x * 0.8f, cameraRecoilKick.x * 1.2f) * recoilMultiplier;
            float horizontal = Random.Range(-cameraRecoilKick.y, cameraRecoilKick.y) * recoilMultiplier;
            targetCameraRecoil += new Vector2(vertical, horizontal);
            targetWeaponRecoil += weaponRecoilKick * recoilMultiplier;

            resetRecoil = false;
        }

        // Smooth towards target recoil
        targetCameraRecoil = Vector2.Lerp(targetCameraRecoil, swayController.GetSwayRotation(), cameraReturnSpeed * Time.deltaTime);
        currentCameraRecoil = Vector2.Lerp(currentCameraRecoil, targetCameraRecoil, cameraSnappiness * Time.deltaTime);

        // Smoothly reduce target recoil back to resting position
        targetWeaponRecoil = Vector3.Lerp(targetWeaponRecoil, swayController.GetPositionOffset(), weaponReturnSpeed * Time.deltaTime);
        currentWeaponRecoil = Vector3.Lerp(targetWeaponRecoil, targetWeaponRecoil, weaponSnappiness * Time.deltaTime);
    }
    public void ApplyRecoil()
    {
        if (enableCameraRecoil)
        {
            // Apply camera recoil rotation
            cameraParent.transform.localRotation = Quaternion.Euler((-currentCameraRecoil.x + swayController.GetSwayRotation().x) / cameraRecoilScale, (currentCameraRecoil.y + swayController.GetSwayRotation().y) / cameraRecoilScale, (swayController.GetSwayRotation().z) / cameraRecoilScale);
        }
        
        if (enableWeaponRecoil)
        {
            // Apply to weapon holder position, account for bob and sway changes
            weaponPositionConstraint.data.offset = swayController.GetBobPosition() + swayController.GetSwayPosition() + currentWeaponRecoil;
        }
    }
}