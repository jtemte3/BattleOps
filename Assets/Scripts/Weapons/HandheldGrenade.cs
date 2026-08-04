using UnityEngine;

public class HandheldGrenade : HandheldObject
{
    public GrenadeProfile grenadeProfile;
    public GameObject spawner;
    public GameObject previewObj;
    public GameObject prefab;
    

    [Header("Fuse")]
    public float maxFuse = 5f;
    public float fuseVariation = 0;

    [Header("Ammo")]
    public int ammoCount;

    public override void Initialize()
    {
        if (grenadeProfile == null)
        {
            grenadeProfile = (GrenadeProfile)profile;
        }

        ammoCount = grenadeProfile.maxAmmount;
    }

    public void RefillWeapon()
    {
        ammoCount = grenadeProfile.maxAmmount;
    }
}
