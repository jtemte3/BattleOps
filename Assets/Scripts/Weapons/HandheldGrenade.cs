using UnityEngine;

public class HandheldGrenade : HandheldObject
{
    public GameObject previewObj;
    public GameObject prefab;
    public GameObject spawner;

    [Header("Fuse")]
    public float maxFuse = 5f;
    public float fuseVariation = 0;
}
