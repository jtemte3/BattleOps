using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponLoader : MonoBehaviour
{
    [Header("Player Default")]
    public int startingWeaponId = 1;
    [Header("State Details")]
    public int currentWeaponId;
    [Header("Dependancies")]
    public SwayAndBobIK swayAndBobIK;
    public RecoilControllerIK recoilControllerIK;
    public Animator animator;
    public TargetedGunFire targetedGun;
    public GrenadeThrower grenadeScript;
    public GunAnimator gunAnimator;
    public GrenadeAnimator grenadeAnimator;
    public AmmoCounter ammoCounter;
    public GameObject gunParent;
    public GameObject rightHandRef;
    public GameObject leftHandRef;
    public bool weaponSwap = false;
    public float swapTime;
    private float resetTime;

    [Space]
    [Header("Weapon Registry")]
    public List<HandheldObject> HandheldList;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        targetedGun.enabled = false;
        grenadeScript.enabled = false;
        recoilControllerIK.enabled = false;
        gunAnimator.enabled = false;
        grenadeAnimator.enabled = false;

        InitializeWeapons();

        LoadWeapon(startingWeaponId);
    }

    private void InitializeWeapons()
    {
        foreach (HandheldObject handheldObject in HandheldList)
        {
            handheldObject.Initialize();
        }
    }

    private void Update()
    {
        List<KeyCode> keys = new List<KeyCode>
        {
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3
        };

        KeyCode input = KeyCode.None;

        foreach(KeyCode key in keys)
        {
            if (Input.GetKeyDown(key))
            {
                input = key;
            }
        }

        switch (input)
        {
            case KeyCode.None:
                break;
            case KeyCode.Alpha1:
                UnloadWeapon(currentWeaponId);
                currentWeaponId = 0;
                break;
            case KeyCode.Alpha2:
                UnloadWeapon(currentWeaponId);
                currentWeaponId = 1;
                break;
            case KeyCode.Alpha3:
                UnloadWeapon(currentWeaponId);
                currentWeaponId = 2;
                break;
        }

        if (weaponSwap)
        {
            if (Time.time >= resetTime)
            {
                weaponSwap = false;
                animator.SetBool(AnimParams.swap, false);

                LoadWeapon(currentWeaponId);
            }
        }
    }

    public void LoadWeapon(int id)
    {
        currentWeaponId = id;
        swayAndBobIK.currentProfile = HandheldList[id].profile;

        int layer = animator.GetLayerIndex(HandheldList[id].animationLayer.ToString());
        animator.SetLayerWeight(layer, 1.0f);

        int emptylayer = animator.GetLayerIndex(AnimLayer.EmptyHands.ToString());
        animator.SetLayerWeight(emptylayer, 0);

        if (HandheldList[id].isGun)
        {
            LoadGun(id);
        }
        else if (HandheldList[id].isGrenade)
        {
            LoadGrenade(id);
        }
        else
        {
            recoilControllerIK.enabled = false;
            targetedGun.enabled = false;
        }

        ammoCounter.handHeld = HandheldList[id];

        rightHandRef.transform.position = HandheldList[id].profile.rightHandPosition;
        rightHandRef.transform.rotation = Quaternion.Euler(HandheldList[id].profile.rightHandRotation);

        leftHandRef.transform.position = HandheldList[id].profile.leftHandPosition;
        leftHandRef.transform.rotation = Quaternion.Euler(HandheldList[id].profile.leftHandRotation);
    }

    private void LoadGun(int id)
    {
        HandheldGun handheldGun = (HandheldGun)HandheldList[id];

        recoilControllerIK.enabled = true;
        targetedGun.enabled = true;
        gunAnimator.enabled = true;

        handheldGun.meshObject.SetActive(true);

        recoilControllerIK.currentProfile = handheldGun.gunProfile;
        targetedGun.handheldGun = handheldGun;

        targetedGun.mode = handheldGun.mode;
        gunAnimator.currentProfile = handheldGun.gunProfile;
    }

    private void LoadGrenade(int id)
    {
        HandheldGrenade handheldGrenade = (HandheldGrenade)HandheldList[id];

        //Enable Grenade Throwing script here, assign grenade profile, and grenade spawner
        grenadeScript.enabled = true;
        grenadeAnimator.enabled = true;

        grenadeScript.handheldGrenade = handheldGrenade;

        if (handheldGrenade.ammoCount > 0)
        {
            handheldGrenade.previewObj.SetActive(true);
        }
        else
        {
            handheldGrenade.previewObj.SetActive(false);
        }
    }

    public void UnloadWeapon(int id)
    {
        //Set Animation to swap, and create trigger time to reset
        weaponSwap = true;
        animator.SetBool(AnimParams.swap, true);
        resetTime = Time.time + swapTime;

        //If the weapon is a gun, unload its related scripts :)
        if (HandheldList[id].isGun)
        {
            recoilControllerIK.enabled = false;
            targetedGun.enabled = false;
            gunAnimator.enabled = false;

            HandheldGun handheldGun = (HandheldGun)HandheldList[id];

            handheldGun.meshObject.SetActive(false);
        }
        else if (HandheldList[id].isGrenade)
        {
            recoilControllerIK.enabled = false;
            targetedGun.enabled = false;
            grenadeScript.enabled = false;
            grenadeAnimator.enabled = false;

            HandheldGrenade handheldGrenade = (HandheldGrenade)HandheldList[id];

            handheldGrenade.previewObj.SetActive(false);
        }

        //For Safety, unload old weapon animation layer and load empty hands animation layer
        int emptylayer = animator.GetLayerIndex(AnimLayer.EmptyHands.ToString());
        animator.SetLayerWeight(emptylayer, 1.0f);

        int oldLayer = animator.GetLayerIndex(HandheldList[id].animationLayer.ToString());
        animator.SetLayerWeight(oldLayer, 0);
    }
}
