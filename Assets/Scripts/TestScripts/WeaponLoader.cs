using System.Collections.Generic;
using UnityEngine;

public class WeaponLoader : MonoBehaviour
{
    [Header("Player Default")]
    public int startingWeaponId = 1;
    [Header("Dependancies")]
    public PlayerController controller;
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

    public void LoadWeapon(int id)
    {
        controller.currentProfile = HandheldList[id].profile;
        swayAndBobIK.currentProfile = HandheldList[id].profile;

        if (HandheldList[id].isGun)
        {
            recoilControllerIK.enabled = true;
            targetedGun.enabled = true;

            recoilControllerIK.currentProfile = HandheldList[id].profile;
            targetedGun.gunObj = HandheldList[id];

            targetedGun.mode = HandheldList[id].profile.supportedModes[0];
            gunAnimator.currentProfile = HandheldList[id].profile;
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
}
