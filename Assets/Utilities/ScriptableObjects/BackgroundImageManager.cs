using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Scriptable Objects/BackgroundImageManager")]
public class BackgroundImageManager : ScriptableObject
{
    public List<Sprite> images;

    public Sprite GetRandomImage()
    {
        return images[Random.Range(0, images.Count)];
    }
}
