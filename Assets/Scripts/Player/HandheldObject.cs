using System;
using UnityEngine;
using UnityEngine.VFX;

[Serializable]
public abstract class HandheldObject : MonoBehaviour
{
    public WeaponProfile profile;
    public bool isGun;
    public bool isGrenade;
    public AnimLayer animationLayer;

    public abstract void Initialize();
}
