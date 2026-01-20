using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RegionData
{
    public int regionId;
    public ZoneType zone;
    public List<Vector2Int> regionPositions;
    public Vector2Int regionCenterPosition;
    public Vector2Int regionSeedPosition;
    public int openPositions;
    public int maxBuildings;
    public int actualBuildingCount;
    public int regionRoadCount = 0;
    public bool finishedPlacingRoads;
}
