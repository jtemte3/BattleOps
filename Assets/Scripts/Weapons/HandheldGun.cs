using System;
using UnityEngine;

[Serializable]
public class HandheldGun : HandheldObject
{
    public GunProfile gunProfile;
    public GameObject meshObject;
    public GameObject muzzleObj;

    [Header("Ammo")]
    public int clipCount;
    public int ammoCount;

    [Header("Mode")]
    public ShootingModes mode;

    public override void Initialize()
    {
        if (gunProfile == null)
        {
            gunProfile = (GunProfile)profile;
        }

        clipCount = gunProfile.maxMagazines;
        ammoCount = gunProfile.magazineSize;
        mode = gunProfile.supportedModes[0];
    }

    public void ReloadWeapon()
    {
        if (clipCount > 0)
        {
            clipCount--;
            ammoCount = gunProfile.magazineSize;
        }
    }

    public void RefillWeapon()
    {
        clipCount = gunProfile.maxMagazines;
    }
}
