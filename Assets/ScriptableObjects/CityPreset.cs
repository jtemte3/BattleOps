using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Scriptable Objects/CityPreset")]
public class CityPreset : ScriptableObject
{
    [Header("City Layout Variables")]
    public int gridWidth;
    public int gridHeight;
    public int gridRoadIntervalX;
    public int gridRoadIntervalY;
    public int gridRoadOffset;
    public int roomCount;
}
