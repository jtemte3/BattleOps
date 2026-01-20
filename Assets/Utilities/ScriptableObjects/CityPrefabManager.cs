using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/CityPrefabManager")]
public class CityPrefabManager : ScriptableObject
{
    public List<GameObject> objectives;
    public List<GameObject> slumBuildings;
    public List<GameObject> cityBuildings;
    public GameObject roadPrefab;
    public GameObject cornerRoadPrefab;
}
