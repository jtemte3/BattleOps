using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class CellGridManager : MonoBehaviour, ISerializationCallbackReceiver
{
    [Tooltip("GameObjects whose positions (XZ) define the perimeter polygon in order")]
    public List<GameObject> perimeterObjects = new List<GameObject>();

    [Min(1)] public int cellSize = 3;

    public CityPrefabManager cityManager;
    public ZoneType zone;
    public int maxGenerationAttempts;
    public int maxBuildings;
    public GameObject buildingParent;
    public List<GridBuilding> buildings = new List<GridBuilding>();
    

    [HideInInspector]
    public bool showGizmos = false;
    [HideInInspector]
    public string debugButtonText = "Show Gizmos";

    // --- Serializable helper list ---
    [System.Serializable]
    public struct CellData
    {
        public Vector2Int position;
        public bool taken;
    }

    [SerializeField] private List<CellData> serializedCells = new List<CellData>();

    // --- Runtime dictionary ---
    public Dictionary<Vector2Int, bool> cellMap = new Dictionary<Vector2Int, bool>();

    public void GenerateGrid()
    {
        List<Vector2> perimeter = GetPerimeterPoints();
        cellMap = BuildCellMapper.GenerateCellMap(perimeter, cellSize, transform.position);
        SaveToSerializedList();

        maxBuildings = (cellMap.Count / 25) + ((cellMap.Count % 25));
    }

    private List<Vector2> GetPerimeterPoints()
    {
        List<Vector2> points = new List<Vector2>();
        foreach (var obj in perimeterObjects)
        {
            if (obj != null)
            {
                Vector3 pos = obj.transform.position;
                points.Add(new Vector2(pos.x, pos.z));
            }
        }
        return points;
    }

    public void CreateBuildings()
    {
        for(int i = 0; i < maxBuildings; i++)
        {
            bool isAvailable = false;
            GameObject selectedCityBlock = Instantiate(GetPrefabForZone(zone), Vector3.zero, Quaternion.identity);

            GridBuilding currentCityBlock = selectedCityBlock.GetComponent<GridBuilding>();
            for (int a = 0; a < maxGenerationAttempts; a++)
            {
                if (!isAvailable)
                {
                    //Vector2Int position = CityGeneratorUtils.GetRandomPosition(city.gridWidth, city.gridHeight);
                    Vector2Int position = VoronoiCityUtils.GetRandomPosition(cellMap);
                    if (cellMap[position] == false)
                    {
                        List<Vector2Int> cityBlockGridPoints = VoronoiCityUtils.FindSurroundingPositions(position, currentCityBlock.dimensions.x, currentCityBlock.dimensions.y, false);
                        //List<Vector2Int> cityBlockGridPointsWithMargin = VoronoiCityUtils.FindSurroundingPositions(position, currentCityBlock.dimensions.x, currentCityBlock.dimensions.y, false);

                        bool isValidPosition = VoronoiCityUtils.isRoomValid(cityBlockGridPoints, cellMap, buildings, position, currentCityBlock);

                        if (isValidPosition)
                        {
                            isAvailable = true;
                            currentCityBlock.gridPosition = position;
                            selectedCityBlock.transform.position = new Vector3((position.x * cellSize) + transform.position.x, this.transform.position.y, (position.y * cellSize) + transform.position.z);
                            selectedCityBlock.transform.parent = buildingParent.transform;

                            cellMap[position] = true;
                            foreach (var cityCell in cityBlockGridPoints)
                            {
                                cellMap[cityCell] = true;
                            }

                            buildings.Add(currentCityBlock);
                            break;
                        }
                    }
                }
            }

            if (!isAvailable)
            {
                DestroyImmediate(selectedCityBlock);
            }
        }
    }

    GameObject GetPrefabForZone(ZoneType zone)
    {
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

    public void RemoveGeneratedBuildings()
    {
        List<GridBuilding> newBuildings = new();

        foreach (GridBuilding building in buildings)
        {
            GameObject gameObject = building.gameObject;

            if (gameObject.name.Contains("Clone"))
            {
                //buildings.Remove(building);
                DestroyImmediate(gameObject);
            }
            else
            {
                newBuildings.Add(building);
            }
        }

        buildings = newBuildings;

        ClearGridPositions();
        UpdateGridPositions();
    }

    public void RemoveBuildings()
    {
        List<GridBuilding> newBuildings = new();

        foreach (GridBuilding building in buildings)
        {
            GameObject gameObject = building.gameObject;

            if (gameObject.name.Contains("Clone"))
            {
                //buildings.Remove(building);
                DestroyImmediate(gameObject);
            }
            else
            {
                newBuildings.Add(building);
            }
        }

        buildings = newBuildings;

        ClearGridPositions();
        UpdateGridPositions();
    }

    public void ClearGridPositions()
    {
        Dictionary<Vector2Int, bool> newMap = new Dictionary<Vector2Int, bool>();
        foreach (var kvp in cellMap)
        {
            newMap[kvp.Key] = false;
        }

        cellMap = newMap;
    }

    public void UpdateGridPositions()
    {
        foreach (GridBuilding building in buildings)
        {
            List<Vector2Int> gridPoints = VoronoiCityUtils.FindSurroundingPositions(building.gridPosition, building.dimensions.x, building.dimensions.y, false);

            foreach (Vector2Int point in gridPoints)
            {
                if (cellMap.ContainsKey(point))
                {
                    cellMap[point] = true;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (showGizmos)
        {
            if (cellMap != null)
            {
                float baseY = transform.position.y;
                Gizmos.color = Color.white;

                foreach (var kvp in cellMap)
                {
                    Vector2Int cell = kvp.Key;
                    bool isFilled = kvp.Value;

                    // worldPos based on anchor + grid index
                    Vector3 worldPos = new Vector3(
                        transform.position.x + cell.x * cellSize,
                        baseY - .1f,
                        transform.position.z + cell.y * cellSize
                    );

                    if (isFilled)
                    {
                        Gizmos.color = Color.gray;
                        Gizmos.DrawCube(worldPos, new Vector3(cellSize, 0, cellSize));
                    }
                    else
                    {
                        Gizmos.color = Color.white;
                        Gizmos.DrawWireCube(worldPos, new Vector3(cellSize, 0, cellSize));
                    }
                }
            }
            if (perimeterObjects.Count > 0)
            {
                for (int i = 0; i < perimeterObjects.Count; i++)
                {
                    int nextIndex = i + 1;
                    if (nextIndex >= perimeterObjects.Count)
                    {
                        nextIndex -= perimeterObjects.Count;
                    }

                    Gizmos.color = Color.yellow;

                    Gizmos.DrawLine(perimeterObjects[i].transform.position, perimeterObjects[nextIndex].transform.position);

                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere(perimeterObjects[i].transform.position, .5f);
                }
            }

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, .5f);
        }
    }

    // --- Serialization helpers ---
    private void SaveToSerializedList()
    {
        serializedCells.Clear();
        foreach (var kvp in cellMap)
        {
            serializedCells.Add(new CellData { position = kvp.Key, taken = kvp.Value });
        }
    }

    private void LoadFromSerializedList()
    {
        cellMap = new Dictionary<Vector2Int, bool>();
        foreach (var c in serializedCells)
        {
            cellMap[c.position] = c.taken;
        }
    }

    public void OnBeforeSerialize() => SaveToSerializedList();
    public void OnAfterDeserialize() => LoadFromSerializedList();
}
