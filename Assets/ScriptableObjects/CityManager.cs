using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/CityManager")]
public class CityManager : ScriptableObject
{
    public List<GameObject> objectives;
    public List<GameObject> cityBlocks;
    public GameObject roadPrefab;
}
