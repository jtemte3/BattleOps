using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class GridVoronoiCity : MonoBehaviour
{
    [Header("Setup")]
    public CityPrefabManager cityManager;
    public RoadSegmentGenerator roadGenerator;

    [Header("Grid Settings")]
    public int width = 50;
    public int height = 50;
    public float gridSize = 3f;
    public int seed = 0;

    [Header("City Shape Settings")]
    public float noiseScale = 0.1f;
    public float baseScale = 0.05f;
    public Vector2 noiseOffset;

    [Header("City Block Generation")]
    public int numberOfRegions = 10;
    public int maxRegionAttempts = 100;
    public int minRegionDistance = 10;
    public int maxGenerationAttempts = 10;
    public bool enableZones = true;
    public bool generateCurveRoadMeshes = true;
    public bool generateGridRoadMeshes = true;
    public bool generateRoadCorners = true;
    public bool widenRoads = true;
    public bool generateBuildingMeshes = true;
    public bool generateWideRoadMeshes = true;
    public int totalSteps;
    public int currentStep;

    [Header("Loading Details")]
    [Range(0f, 1f)]
    public float loadingPercentage = 0;
    public string loadingState = "";
    public bool isFullyLoaded = false;
    private bool areRegionsCreated = false;
    private bool isGridPopulated = false;
    private bool areCentersIdentified = false;
    private bool areRoadsIdentified = false;
    private bool areRoadsEdgesIdentified = false;
    private bool areRoadsWidened = false;
    private bool areRoadMeshesSpawned = false;
    private bool areBuildingsSpawned = false;
    private bool areBuildingsActivated = false;
    public System.DateTime startTime;
    public System.DateTime endTime;
    public string elapsedTime = "Loading...";


    public Dictionary<Vector2Int, GridCell> cityGrid;
    public Dictionary<Vector2Int, GridCell> buildingGrid;
    //public Dictionary<Vector2Int, GridCell> roadGrid;
    public List<RegionData> regions = new();

    public List<Vector2Int> roadPositions = new();
    public List<Vector2Int> wideRoadPositions = new();
    public List<Vector2Int> roadIntersections = new();
    public List<Vector2Int> roadEdgePositions = new();
    public List<Vector2Int> blockedPositions = new();
    public List<GridBuilding> buildings = new();

    [Header("Debug Settings")]
    public bool showGizmos = false;
    public bool showUnits = false;
    public bool showHighlight = false;
    public Color gridColor = new Color(1, 1, 1, .1f);
    public Color gridBoundryColor = Color.yellow;
    public Color regionCenterColor = new Color(0, 1, 1, .5f);
    public Color IntersectionColor = new Color(0, 1, 0, .25f);
    public Color roadEdgeColor = new Color(1, .45f, 0, .5f);
    public Vector2Int highlightPos = new Vector2Int();

    void Start()
    {
        if (seed != 0)
        {
            Random.InitState(seed);
        }
        else
        {
            seed = Random.Range(int.MinValue, int.MaxValue);
            Random.InitState(seed);
        }

        cityGrid = new Dictionary<Vector2Int, GridCell>();
        buildingGrid = new Dictionary<Vector2Int, GridCell>();
        //roadGrid = new Dictionary<Vector2Int, GridCell>();

        CreateRectangularGrid(cityGrid);
        //CreateCircleCity(roadGrid);
        //CreateOrganicGrid();
        CreateWavyCity(buildingGrid);

        startTime = System.DateTime.Now;
        StartCoroutine(GenerateCityCoroutine());
    }

    void CreateRectangularGrid(Dictionary<Vector2Int, GridCell> grid)
    {
        for (int i = -(height / 2); i <= (height / 2); i++)
        {
            for (int j = -(width / 2); j <= (width / 2); j++)
            {
                cityGrid.TryAdd(new Vector2Int(j, i), new GridCell());
            }
        }
    }

    void CreateCircleCity(Dictionary<Vector2Int, GridCell> grid)
    {
        int radius = (Mathf.Min(width, height) / 2);

        for (int i = -height / 2; i <= height / 2; i++)
        {
            for (int j = -width / 2; j <= width / 2; j++)
            {
                if (j * j + i * i <= radius * radius) // inside circle
                {
                    grid.TryAdd(new Vector2Int(j, i), new GridCell());
                }
            }
        }
    }

    void CreateOrganicGrid()
    {
        float radius = Mathf.Min(width, height) / 2f;
        float innerRadius = radius * 0.85f;  // always included zone
        //float outerRadius = radius * .95f;  // hard cutoff zone

        noiseOffset = new Vector2(Random.Range(0f, 10000f), Random.Range(0f, 10000f));

        for (int i = -height / 2; i <= height / 2; i++)
        {
            for (int j = -width / 2; j <= width / 2; j++)
            {
                Vector2 pos = new Vector2(j, i);
                float dist = pos.magnitude;

                if (dist <= innerRadius)
                {
                    // always include inside core
                    cityGrid.TryAdd(new Vector2Int(j, i), new GridCell());
                }
                else if (dist > innerRadius)
                {
                    // noisy edge zone
                    /*float warpedX = j + Mathf.PerlinNoise(i * 0.1f + noiseOffset.x, 0f + noiseOffset.y) * 20f;
                    float warpedY = i + Mathf.PerlinNoise(0f + noiseOffset.x, j * 0.1f + noiseOffset.y) * 20f;
                    float noise = Mathf.PerlinNoise(warpedX * 0.05f + noiseOffset.x, warpedY * 0.05f + noiseOffset.y);*/

                    float noise = Mathf.PerlinNoise((i * noiseScale) + noiseOffset.x, (j * noiseScale) + noiseOffset.y);

                    //float normalizedDist = (dist - innerRadius) / (outerRadius - innerRadius); // 0 → 1
                    //if (dist + (noise - 0.5f) * 0.25f < 1f)
                    //{
                    //    grid.TryAdd(new Vector2Int(j, i), new GridCell());
                    //}
                    if (noise < .5f)
                    {
                        cityGrid.TryAdd(new Vector2Int(j, i), new GridCell());
                    }
                }
                // else: beyond outer radius, skip
            }
        }
    }

    void CreateWavyCity(Dictionary<Vector2Int, GridCell> grid)
    {
        float baseRadius = Mathf.Min(width, height) / 2f;
        float sinAmplitude = baseRadius * 0.1f;   // how far the waves push in/out
        float cosAmplitude = baseRadius * 0.1f;   // how far the waves push in/out
        float sinFrequency = 4f;                  // number of waves around the circle
        float cosFrequency = 4f;                  // number of waves around the circle

        grid.Clear();

        for (int i = -height / 2; i <= height / 2; i++)
        {
            for (int j = -width / 2; j <= width / 2; j++)
            {
                Vector2 pos = new Vector2(j, i);
                float angle = Mathf.Atan2(i, j);             // angle around center
                float dist = pos.magnitude;

                // boundary radius at this angle
                //float wavyRadius = baseRadius + Mathf.Sin(angle * frequency) * amplitude;
                float wavyRadius = baseRadius
                    + Mathf.Sin(angle * sinFrequency) * sinAmplitude
                    + Mathf.Cos(angle * cosFrequency) * cosAmplitude;

                if (dist <= wavyRadius)
                {
                    grid.TryAdd(new Vector2Int(j, i), new GridCell());
                }
            }
        }
    }

    IEnumerator GenerateCityCoroutine()
    {

        totalSteps = 5 + (numberOfRegions * 3) + (width * height);
        currentStep = 0;

        InitializeRegions();
        loadingState = "Initializing Regions";
        currentStep++;
        loadingPercentage = (float)currentStep / totalSteps;
        yield return null;
        areRegionsCreated = true;

        GenerateGrid();
        loadingState = "Separating Grid Into Regions";
        currentStep++;
        loadingPercentage = (float)currentStep / totalSteps;
        yield return null;
        isGridPopulated = true;

        IdentifyRegionCenters_Median();
        loadingState = "Identifying Region Centers";
        currentStep++;
        loadingPercentage = (float)currentStep / totalSteps;
        yield return null;
        areCentersIdentified = true;

        IdentifyMainRoadPositionsAndIntersections();
        loadingState = "Identifying Main Road Positions and Intersections";
        currentStep++;
        loadingPercentage = (float)currentStep / totalSteps;
        yield return null;
        areRoadsIdentified = true;

        IdentifyRoadEdges();
        loadingState = "Identifying Road and Boundary Intersections";
        currentStep++;
        loadingPercentage = (float)currentStep / totalSteps;
        yield return null;
        areRoadsEdgesIdentified = true;

        if (generateCurveRoadMeshes)
        {
            List<Vector2Int> allIntersectionsAndEdges = new();
            allIntersectionsAndEdges.AddRange(roadIntersections);
            allIntersectionsAndEdges.AddRange(roadEdgePositions);

            roadGenerator.roadPositions = roadPositions;
            roadGenerator.GenerateRoadSegments(allIntersectionsAndEdges);

            loadingState = "Procedurally Generating Curved Road Meshes and Intersections";
            currentStep++;
            loadingPercentage = (float)currentStep / totalSteps;
            yield return null;
        }

        if (widenRoads)
        {
            WidenRoads();
            loadingState = "Widening The Roads";
            currentStep++;
            loadingPercentage = (float)currentStep / totalSteps;
            yield return null;
        }
        
        areRoadsWidened = true;

        InstantiateRoadPrefabsWithCorners();
        loadingState = "Spawning Road Prefabs";
        currentStep++;
        loadingPercentage = (float)currentStep / totalSteps;
        yield return null;
        areRoadMeshesSpawned = true;

        if (generateBuildingMeshes)
        {
            int maxNumofBuildings = 0;
            foreach (var region in regions)
            {
                region.openPositions = region.regionPositions.Count - region.regionRoadCount;

                region.maxBuildings = (region.openPositions / 10) + ((region.openPositions % 10) * 3);
                maxNumofBuildings += region.maxBuildings;
            }

            totalSteps = 4 + (numberOfRegions * 3) + maxNumofBuildings;

            foreach (var region in regions)
            {
                loadingState = "Placing Buildings in Region: " + region.regionId;
                for (int i = 0; i < region.maxBuildings; i++)
                {
                    CreateRegionBuilding(region);
                    currentStep++;
                    loadingPercentage = (float)currentStep / totalSteps;
                    yield return null;
                }
            }
        }
        
        areBuildingsSpawned = true;

        foreach (GridBuilding building in buildings)
        {
            building.gameObject.SetActive(true);
        }
        areBuildingsActivated = true;

        loadingState = "Validating";
        GenerationCompletionCheck();
        yield return null;

        while (!isFullyLoaded)
        {
            yield return null;
        }

        loadingState = "Finished";
        loadingPercentage = 1f;
        endTime = System.DateTime.Now;
        elapsedTime = startTime.Subtract(endTime).Duration().ToString(@"mm\:ss\.fff");
        //yield return null;
    }

    private void GenerationCompletionCheck()
    {
        if (areRegionsCreated == true && isGridPopulated == true && areCentersIdentified == true && areRoadsIdentified == true && areRoadsEdgesIdentified == true && areRoadsWidened == true && areRoadMeshesSpawned == true && areBuildingsSpawned == true && areBuildingsActivated == true)
        {
            isFullyLoaded = true;
        }
        else
        {
            isFullyLoaded = false;
        }
    }

    void InitializeRegions()
    {
        for (int i = 0; i < numberOfRegions; i++)
        {
            Vector2Int seed = new(0,0);
            bool isValid = true;

            for (int n = 0; n < maxRegionAttempts; n++)
            {
                isValid = true;

                seed = new Vector2Int(
                    Random.Range((-width / 2), (width / 2)),
                    Random.Range((-height / 2), (height / 2))
                    );

                if (!cityGrid.ContainsKey(seed))
                {
                    break;
                }

                if (regions.Count > 0)
                {
                    foreach(RegionData regionData in regions)
                    {
                        float dist = Vector2Int.Distance(seed, regionData.regionSeedPosition);

                        if (dist < minRegionDistance)
                        {
                            isValid = false;
                        }
                    }
                }

                if (isValid)
                {
                    break;
                }
            }

            if (isValid)
            {
                RegionData region = new()
                {
                    regionId = i,
                    // Assign a random zone to this seed
                    zone = (ZoneType)Random.Range(0, System.Enum.GetValues(typeof(ZoneType)).Length),
                    regionSeedPosition = seed,
                    regionPositions = new List<Vector2Int>()
                };
                region.regionPositions.Add(seed);

                regions.Add(region);
            }
        }
    }

    void GenerateGrid()
    {
        for (int x = (-width/2); x <= (width/2); x++)
        {
            for (int y = (-height/2); y <= (height/2); y++)
            {
                
                Vector2Int current = new Vector2Int(x, y);
                if (cityGrid.ContainsKey(current))
                {
                    int closestSeedIndex = -1;
                    float minDist = float.MaxValue;

                    foreach (var region in regions)
                    {
                        float dist = Vector2Int.Distance(current, region.regionSeedPosition);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            closestSeedIndex = region.regionId;
                        }
                    }

                    cityGrid[current].seedId = closestSeedIndex;
                    regions[closestSeedIndex].regionPositions.Add(current);
                }
            }
        }
    }

    void IdentifyMainRoadPositionsAndIntersections()
    {
        if (cityManager.roadPrefab == null) return;

        foreach (RegionData region in regions)
        {
            foreach (Vector2Int pos in region.regionPositions)
            {
                if (cityGrid.ContainsKey(pos))
                {
                    (bool isBorder, List<Vector2Int> otherBorderPos, int borderCount) position = IsBorderCellAndIntersection(pos, 1);

                    bool isIntersection = false;

                    if (position.borderCount > 1)
                    {
                        isIntersection = true;
                    }

                    if (position.isBorder)
                    {
                        List<RegionData> borderRegions = new List<RegionData>();

                        foreach (Vector2Int borderpos in position.otherBorderPos)
                        {
                            if (!borderRegions.Contains(regions[cityGrid[borderpos].seedId]))
                            {
                                borderRegions.Add(regions[cityGrid[borderpos].seedId]);
                            }
                        }

                        if (!isIntersection)
                        {
                            bool makeRoad = true;

                            foreach (Vector2Int borderpos in position.otherBorderPos)
                            {
                                if (roadPositions.Contains(borderpos))
                                {
                                    makeRoad = false;
                                }
                            }
                            if (makeRoad)
                            {
                                roadPositions.Add(pos);
                                blockedPositions.Add(pos);
                            }
                            else
                            {
                                bool haveAllOtherRegionsFinished = true;

                                foreach (RegionData borderRegion in borderRegions)
                                {
                                    if (!borderRegion.finishedPlacingRoads)
                                    {
                                        haveAllOtherRegionsFinished = false;
                                        break;
                                    }
                                }

                                if (!haveAllOtherRegionsFinished)
                                {
                                    roadPositions.Add(pos);
                                    blockedPositions.Add(pos);
                                }
                            }
                        }
                        else
                        {
                            bool haveAllOtherRegionsFinished = true;

                            foreach (RegionData borderRegion in borderRegions)
                            {
                                if (!borderRegion.finishedPlacingRoads)
                                {
                                    haveAllOtherRegionsFinished = false;
                                    break;
                                }
                            }

                            if (!haveAllOtherRegionsFinished)
                            {
                                roadPositions.Add(pos);
                                blockedPositions.Add(pos);
                            }

                            bool isValidIntersection = true;
                            if (roadIntersections.Count > 0)
                            {

                                foreach (Vector2Int intersection in roadIntersections)
                                {
                                    float distance = Vector2Int.Distance(pos, intersection);

                                    if (distance < 2)
                                    {
                                        isValidIntersection = false;
                                        break;
                                    }
                                }
                            }

                            if (isValidIntersection && !haveAllOtherRegionsFinished)
                            {
                                roadIntersections.Add(pos);
                            }
                        }

                    }
                }
                
            }
            region.finishedPlacingRoads = true;
        }
    }

    void WidenRoads()
    {
        
        foreach(Vector2Int pos in roadPositions)
        {
            List<Vector2Int> surroundingPositions = new()
            {
                new(pos.x, pos.y + 1),
                new(pos.x, pos.y - 1),
                new(pos.x - 1, pos.y),
                new(pos.x + 1, pos.y),
                new(pos.x + 1, pos.y + 1),
                new(pos.x + 1, pos.y - 1),
                new(pos.x - 1, pos.y + 1),
                new(pos.x - 1, pos.y - 1)
            };

            foreach (Vector2Int newPos in surroundingPositions)
            {
                if (cityGrid.Keys.Contains(newPos) && !roadPositions.Contains(newPos))
                {
                    wideRoadPositions.Add(newPos);
                    blockedPositions.Add(newPos);
                }
            }
        }
    }

    void InstantiateRoadPrefabsWithCorners()
    {
        List<Vector2Int> validPositions = roadPositions;
        List<Vector2Int> positionsOnEdge = GetEdgePositions();

        if (generateWideRoadMeshes)
        {
            validPositions.AddRange(wideRoadPositions);
        }

        foreach (Vector2Int pos in validPositions)
        {
            int regionId = cityGrid[pos].seedId;
            RegionData region = regions[regionId];
            region.regionRoadCount++;

            if (generateGridRoadMeshes)
            {
                // Check neighbors
                bool up = validPositions.Contains(pos + Vector2Int.up);
                bool down = validPositions.Contains(pos + Vector2Int.down);
                bool left = validPositions.Contains(pos + Vector2Int.left);
                bool right = validPositions.Contains(pos + Vector2Int.right);

                Vector3 worldPos = new Vector3(pos.x * gridSize, 0f, pos.y * gridSize);

                GameObject prefabToSpawn = cityManager.roadPrefab;
                Quaternion rot = Quaternion.identity;

                // Cannot spawn a corner on the edge of the map
                if (!positionsOnEdge.Contains(pos) && generateRoadCorners)
                {
                    // Corner detection: exactly two neighbors that are not opposite
                    if (up && right && !down && !left)
                    {
                        prefabToSpawn = cityManager.cornerRoadPrefab;
                        rot = Quaternion.Euler(0f, 0f, 0f);
                    }
                    else if (right && down && !up && !left)
                    {
                        prefabToSpawn = cityManager.cornerRoadPrefab;
                        rot = Quaternion.Euler(0f, 90f, 0f);
                    }
                    else if (down && left && !up && !right)
                    {
                        prefabToSpawn = cityManager.cornerRoadPrefab;
                        rot = Quaternion.Euler(0f, 180f, 0f);
                    }
                    else if (left && up && !down && !right)
                    {
                        prefabToSpawn = cityManager.cornerRoadPrefab;
                        rot = Quaternion.Euler(0f, 270f, 0f);
                    }
                }

                GameObject road = Instantiate(prefabToSpawn, worldPos, rot, transform);
                road.GetComponent<GridBuilding>().gridPosition = pos;
            }
        }
    }

    void IdentifyRoadEdges()
    {
        //List<Vector2Int> edgesPositions = GetEdgePositions();
        List<Vector2Int> edgesPositions = GetEdgeCells();

        foreach (Vector2Int pos in edgesPositions)
        {
            if (roadPositions.Contains(pos))
            {
                roadEdgePositions.Add(pos);
            }
        }
    }

    List<Vector2Int> GetEdgePositions()
    {
        var points = new List<Vector2Int>();

        // Top and bottom
        for (int x = (-width / 2); x <= width / 2; x++)
        {
            points.Add(new Vector2Int(x, (-height / 2)));
            points.Add(new Vector2Int(x, height / 2));
        }

        // Left and right
        for (int y = (-height / 2); y <= height / 2; y++)
        {
            points.Add(new Vector2Int((-width / 2), y));
            points.Add(new Vector2Int(width / 2, y));
        }

        return points;
    }

    List<Vector2Int> GetEdgeCells()
    {
        List<Vector2Int> edgeCells = new List<Vector2Int>();

        // directions to check (4-neighbors, or 8 if you want diagonals)
        Vector2Int[] dirs = new Vector2Int[]
        {
            new Vector2Int(1, 0),   // E
            new Vector2Int(-1, 0),  // W
            new Vector2Int(0, 1),   // N
            new Vector2Int(0, -1),  // S
            new Vector2Int(1, 1),   // NE
            new Vector2Int(-1, 1),  // NW
            new Vector2Int(1, -1),  // SE
            new Vector2Int(-1, -1), // SW
        };

        foreach (var kvp in cityGrid)
        {
            Vector2Int pos = kvp.Key;
            int numberMissing = 0;
            //bool isEdgeCell = false;

            foreach (var dir in dirs)
            {
                if (!cityGrid.ContainsKey(pos + dir))
                {
                    numberMissing++;
                }
            }

            if (numberMissing > 0 && numberMissing < 5)
            {
                edgeCells.Add(pos);
            }
        }

        return edgeCells;
    }

    void IdentifyRegionCenters_Median()
    {
        foreach (var region in regions)
        {
            List<int> xValues = region.regionPositions.Select(p => p.x).ToList();
            List<int> yValues = region.regionPositions.Select(p => p.y).ToList();

            xValues.Sort();
            yValues.Sort();

            int midCount = region.regionPositions.Count / 2;
            int medianX = 0;
            int medianY = 0;
            
            if (region.regionPositions.Count % 2 == 1)
            {
                medianX = xValues[midCount];
                medianY = yValues[midCount];
            }
            else
            {
                medianX = (xValues[midCount - 1] + xValues[midCount]) / 2;
                medianY = (yValues[midCount - 1] + yValues[midCount]) / 2;
            }

            region.regionCenterPosition = new(medianX, medianY);
        }
    }

    void CreateRegionBuilding(RegionData region)
    {
        bool isAvailable = false;

        GameObject selectedCityBlock = Instantiate(GetPrefabForZone(region.zone), Vector3.zero, Quaternion.identity);
        selectedCityBlock.SetActive(false);

        GridBuilding currentCityBlock = selectedCityBlock.GetComponent<GridBuilding>();

        for (int a = 0; a < maxGenerationAttempts; a++)
        {
            if (!isAvailable)
            {
                //Vector2Int position = CityGeneratorUtils.GetRandomPosition(city.gridWidth, city.gridHeight);
                Vector2Int position = VoronoiCityUtils.GetRandomPosition(region, blockedPositions, cityGrid);
                if (buildingGrid.ContainsKey(position))
                {
                    if (cityGrid[position].isBlocked == false)
                    {
                        List<Vector2Int> cityBlockGridPoints = VoronoiCityUtils.FindSurroundingPositions(position, currentCityBlock.dimensions.x, currentCityBlock.dimensions.y, false);
                        //List<Vector2Int> cityBlockGridPointsWithMargin = VoronoiCityUtils.FindSurroundingPositions(position, currentCityBlock.dimensions.x, currentCityBlock.dimensions.y, false);

                        bool isValidPosition = VoronoiCityUtils.isRoomValid(cityBlockGridPoints, blockedPositions, buildings, width, height, position, currentCityBlock);

                        if (isValidPosition)
                        {
                            isAvailable = true;
                            currentCityBlock.gridPosition = position;
                            selectedCityBlock.transform.position = new Vector3((position.x * gridSize) + transform.position.x, 0, (position.y * gridSize) + transform.position.z);
                            selectedCityBlock.transform.parent = this.transform;

                            cityGrid[position].isBlocked = true;
                            buildings.Add(currentCityBlock);
                            blockedPositions.AddRange(cityBlockGridPoints);
                            region.actualBuildingCount++;
                            break;
                        }
                    }
                }
            }
        }

        if (!isAvailable)
        {
            Destroy(selectedCityBlock);
        }
    }

    (bool, List<Vector2Int>, int) IsBorderCellAndIntersection(Vector2Int pos, int range)
    {
        int currentId = cityGrid[pos].seedId;

        List<Vector2Int> surroundingPositions = new();
        List<Vector2Int> otherRegionPos = new();
        List<int> borderRegionSeeds = new();

        // Check surrounding cells
        surroundingPositions.Add(new(pos.x, pos.y + 1));
        surroundingPositions.Add(new(pos.x, pos.y - 1));
        surroundingPositions.Add(new(pos.x - 1, pos.y));
        surroundingPositions.Add(new(pos.x + 1, pos.y));

        foreach (Vector2Int newPos in surroundingPositions)
        {
            if (cityGrid.Keys.Contains(newPos))
            {
                int cellSeed = cityGrid[newPos].seedId;
                if (cellSeed != currentId)
                {
                    otherRegionPos.Add(newPos);

                    if (!borderRegionSeeds.Contains(cellSeed))
                    {
                        borderRegionSeeds.Add(cellSeed);
                    }
                }
            }
        }

        if (otherRegionPos.Count > 0)
        {
            return (true, otherRegionPos, borderRegionSeeds.Count);
        }

        return (false, null, 0);
    }

    GameObject GetPrefabForZone(ZoneType zone)
    {
        if (!enableZones)
        {
            zone = ZoneType.Normal;
        }

        switch (zone)
        {
            case ZoneType.Slum:
                if (cityManager.slumBuildings.Count > 0)
                    return cityManager.slumBuildings[Random.Range(0, cityManager.slumBuildings.Count)];
                break;
            case ZoneType.Normal:
                if (cityManager.cityBuildings.Count > 0)
                    return cityManager.cityBuildings[Random.Range(0, cityManager.cityBuildings.Count)];
                break;
        }
        return null;
    }

    public (Vector3, Vector3) GetLandingZoneRoadGen()
    {
        int randomIntersectionId = Random.Range(0, roadGenerator.intersections.Count);
        Vector2Int pos = roadGenerator.intersections[randomIntersectionId];

        Vector2Int closestPosition = new(int.MaxValue, int.MaxValue);

        foreach (Vector2Int intersection in roadGenerator.intersections)
        {
            float oldDist = Vector2Int.Distance(pos, closestPosition);
            float newDist = Vector2Int.Distance(pos, intersection);
            if (oldDist < newDist)
            {
                closestPosition = intersection;
            }
        }

        return (new Vector3(pos.x * gridSize, -.1f, pos.y * gridSize), new Vector3(closestPosition.x * gridSize, -.1f, closestPosition.y * gridSize));
    }

#if (UNITY_EDITOR)
    public void OnDrawGizmos()
    {
        if (showGizmos)
        {
            Gizmos.color = gridBoundryColor;
            Gizmos.DrawWireCube(transform.position, new Vector3(height * gridSize, 0, width * gridSize));
            //Gizmos.DrawWireCube(new Vector3(((city.gridHeight / 2) * gridScale) + transform.position.x, 0, ((city.gridWidth / 2) * gridScale) + transform.position.z), new Vector3(city.gridHeight * gridScale, gridScale, city.gridWidth * gridScale));

            Gizmos.color = gridColor;

            if (width % 10 == 0)
            {
                for (int x = (int)(-(width * 1.5) - gridSize); x <= (width * 1.5); x += 3)
                {
                    Vector3 startPos = new Vector3(x + 1.5f, 0, (float)((width * 1.5) + 1.5f)) + transform.position;
                    Vector3 endPos = new Vector3(x + 1.5f, 0, (float)((width * -1.5) - 1.5f)) + transform.position;
                    Gizmos.DrawLine(startPos, endPos);
                }

                for (int y = (int)(-(height * 1.5) - gridSize); y <= (height * 1.5); y += 3)
                {
                    Vector3 startPos = new Vector3((float)((height * 1.5) + 1.5f), 0, (float)y + 1.5f) + transform.position;
                    Vector3 endPos = new Vector3((float)((height * -1.5) - 1.5f), 0, (float)y + 1.5f) + transform.position;
                    Gizmos.DrawLine(startPos, endPos);
                }
            }
            else if (width % 10 == 5)
            {
                for (int x = (int)(-(height * 1.5) - gridSize); x <= (height * 1.5); x += 3)
                {
                    Vector3 startPos = new Vector3(x + 2.5f, 0, height * 1.5f) + transform.position;
                    Vector3 endPos = new Vector3(x + 2.5f, 0, -height * 1.5f) + transform.position;
                    Gizmos.DrawLine(startPos, endPos);
                }

                for (int y = (int)(-(height * 1.5) - gridSize); y <= (height * 1.5); y += 3)
                {
                    Vector3 startPos = new Vector3(height * 1.5f, 0, y + 2.5f) + transform.position;
                    Vector3 endPos = new Vector3(-height * 1.5f, 0, y + 2.5f) + transform.position;
                    Gizmos.DrawLine(startPos, endPos);
                }
            }

            if (showUnits)
            {
                for (int w = -(width / 2); w <= (width / 2); w++)
                {
                    Vector3 labelPos = new Vector3(((height / 2) * gridSize) + 3, 0f, w * gridSize) + transform.position;
                    Handles.Label(labelPos, w.ToString());
                }

                for (int h = -(height / 2); h <= (height / 2); h++)
                {
                    Vector3 labelPos = new Vector3(h * gridSize, 0, ((width / 2) * gridSize) + 3) + transform.position;
                    Handles.Label(labelPos, h.ToString());
                }
            }

            if (showHighlight)
            {
                Gizmos.color = Color.white;

                Vector3 pos = new Vector3(highlightPos.x * gridSize, 0, highlightPos.y * gridSize) + transform.position;

                Gizmos.DrawWireCube(pos, Vector3.one * 3);
            }

            Gizmos.color = regionCenterColor;
            if (regions.Count > 0)
            {
                foreach (var region in regions)
                {
                    Vector3 pos = new Vector3(region.regionCenterPosition.x * gridSize, 1, region.regionCenterPosition.y * gridSize) + transform.position;
                    Vector3 labelPos = new Vector3(region.regionCenterPosition.x * gridSize, 5, region.regionCenterPosition.y * gridSize) + transform.position;
                    Gizmos.DrawSphere(pos, 3f);
                    Handles.Label(labelPos, region.regionId.ToString());
                }

                if (roadIntersections.Count > 0)
                {
                    Gizmos.color = IntersectionColor;
                    foreach (Vector2Int intersection in roadIntersections)
                    {
                        Vector3 pos = new Vector3(intersection.x * gridSize, 1, intersection.y * gridSize) + transform.position;
                        Gizmos.DrawSphere(pos, 1f);
                    }
                }

                if (roadEdgePositions.Count > 0)
                {
                    Gizmos.color = roadEdgeColor;
                    //Gizmos.color = Color.orange;
                    foreach (Vector2Int edgePos in roadEdgePositions)
                    {
                        Vector3 pos = new Vector3(edgePos.x * gridSize, 1, edgePos.y * gridSize) + transform.position;
                        Gizmos.DrawSphere(pos, 1f);
                    }
                }
            }
        }
    }
#endif
}
