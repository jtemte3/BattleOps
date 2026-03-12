using UnityEngine;

public class HandheldGrenade : HandheldObject
{

    public GameObject spawner;
    public GameObject previewObj;
    public GameObject prefab;
    

    [Header("Fuse")]
    public float maxFuse = 5f;
    public float fuseVariation = 0;
}
