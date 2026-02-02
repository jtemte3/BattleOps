using System.Collections.Generic;
using UnityEngine;

public class WeaponLoader : MonoBehaviour
{
    [Header("Player Default")]
    public int startingWeaponId = 1;
    [Header("Dependancies")]
    public SwayAndBobIK swayAndBobIK;
    public RecoilControllerIK recoilControllerIK;
    public Animator animator;
    public TargetedGunFire targetedGun;
    public GunAnimator gunAnimator;
    public GameObject gunParent;
    public GameObject rightHandRef;
    public GameObject leftHandRef;

    [Space]
    [Header("Weapon Registry")]
    public List<HandheldObject> HandheldList;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadWeapon(startingWeaponId);
    }

    private void Update()
    {
        /*var keyCode = (KeyCode)System.Enum.Parse(typeof(KeyCode), Input.inputString.ToUpper());
        switch (keyCode)
        {
            case KeyCode.Alpha1:
                LoadWeapon(1);
                break;
        }*/
    }

    public void LoadWeapon(int id)
    {
        swayAndBobIK.currentProfile = HandheldList[id].profile;

        int layer = animator.GetLayerIndex(HandheldList[id].animationLayer.ToString());
        animator.SetLayerWeight(layer, 100);

        if (HandheldList[id].isGun)
        {
            HandheldGun handheldGun = (HandheldGun)HandheldList[id];
            GunProfile gunProfile = (GunProfile)handheldGun.profile;

            recoilControllerIK.enabled = true;
            targetedGun.enabled = true;

            handheldGun.meshObject.SetActive(true);

            recoilControllerIK.currentProfile = gunProfile;
            targetedGun.gunProfile = gunProfile;
            targetedGun.muzzleObj = handheldGun.muzzleObj;

            targetedGun.mode = gunProfile.supportedModes[0];
            gunAnimator.currentProfile = gunProfile;
        }
        else
        {
            recoilControllerIK.enabled = false;
            targetedGun.enabled = false;
        }


        rightHandRef.transform.position = HandheldList[id].profile.rightHandPosition;
        rightHandRef.transform.rotation = Quaternion.Euler(HandheldList[id].profile.rightHandRotation);

        leftHandRef.transform.position = HandheldList[id].profile.leftHandPosition;
        leftHandRef.transform.rotation = Quaternion.Euler(HandheldList[id].profile.leftHandRotation);
    }

    public void UnloadWeapon(int id)
    {
        int layer = animator.GetLayerIndex(HandheldList[id].animationLayer.ToString());
        animator.SetLayerWeight(layer, 0);

        recoilControllerIK.enabled = false;
        targetedGun.enabled = false;

        rightHandRef.transform.localPosition = Vector3.zero;
        rightHandRef.transform.localRotation = Quaternion.Euler(Vector3.zero);

        leftHandRef.transform.localPosition = Vector3.zero;
        leftHandRef.transform.localRotation = Quaternion.Euler(Vector3.zero);
    }
}
