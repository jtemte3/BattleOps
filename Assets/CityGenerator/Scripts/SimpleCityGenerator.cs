using System.Collections.Generic;
using UnityEngine;

public class SimpleCityGenerator : MonoBehaviour
{
    public CityManager cityManager;

    [Header("Options")]
    public bool generateMainRoadMargins = true;
    public bool generateMargins = true;
    public bool generateRoads = true;
    public int maxGenerationAttempts = 10;
    public int seed = 0;

    [Header("City Layout Variables")]
    public int gridWidth = 10;
    public int gridHeight = 10;
    public int gridRoadIntervalX = 20;
    public int gridRoadIntervalY = 20;
    public int roomCount = 10;
    public int gridScale = 5;

    [Header("City Objects")]
    public Dictionary<Vector2Int, bool> gridMap = new Dictionary<Vector2Int, bool>();
    public List<CityBlock> cityBlocks;
    public List<Vector2Int> blockedPositions;
    public List<Vector2Int> roadPositions;
    
    // Start is called before the first frame update
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

        for (int i = 0; i <= gridHeight; i++)
        {
            for (int j = 0; j <= gridWidth; j++)
            {
                gridMap.TryAdd(new Vector2Int(j, i), false);
            }
        }

        //grid = new RoomTwo[gridWidth, gridHeight];
        GenerateCity();
    }

    private void GenerateCity()
    {
        if (generateRoads)
        {
            GenerateMainRoads();
        }
        // Step 1: Generate Rooms
            CreateCityBlocks();
        // Step 2: Generate Traps
            //Does not exist yet

        if (generateRoads)
        {
            // Step 3: Generate Road Positions
                IdentifyRoadPositions();
            // Step 4: Update Road Models
                GenerateRoadMeshes();
        }

    }

    private void GenerateMainRoads()
    {
        foreach (var position in gridMap)
        {
            if (position.Key.x % gridRoadIntervalX == 0)
            {

                Vector2Int newRoadPosition = new Vector2Int(position.Key.x, position.Key.y);
                
                //newRoadPosition.x += Random.Range(-1, 1);

                if (!blockedPositions.Contains(newRoadPosition))
                {
                    blockedPositions.Add(newRoadPosition);
                    roadPositions.Add(newRoadPosition);

                    if (!blockedPositions.Contains(new Vector2Int(newRoadPosition.x + 1, newRoadPosition.y)))
                    {
                        blockedPositions.Add(new Vector2Int(newRoadPosition.x + 1, newRoadPosition.y));
                        roadPositions.Add(new Vector2Int(newRoadPosition.x + 1, newRoadPosition.y));
                    }
                    if (!blockedPositions.Contains(new Vector2Int(newRoadPosition.x - 1, newRoadPosition.y)))
                    {
                        blockedPositions.Add(new Vector2Int(newRoadPosition.x - 1, newRoadPosition.y));
                        roadPositions.Add(new Vector2Int(newRoadPosition.x - 1, newRoadPosition.y));
                    }
                }
                
                
            }

            if (position.Key.y % gridRoadIntervalY == 0)
            {
                Vector2Int newRoadPosition = new Vector2Int(position.Key.x, position.Key.y);

                //newRoadPosition.y += Random.Range(-1, 1);

                if (!blockedPositions.Contains(newRoadPosition))
                {
                    blockedPositions.Add(newRoadPosition);
                    roadPositions.Add(newRoadPosition);

                    if (!blockedPositions.Contains(new Vector2Int(newRoadPosition.x, newRoadPosition.y + 1)))
                    {
                        blockedPositions.Add(new Vector2Int(newRoadPosition.x, newRoadPosition.y + 1));
                        roadPositions.Add(new Vector2Int(newRoadPosition.x, newRoadPosition.y + 1));
                    }
                    if (!blockedPositions.Contains(new Vector2Int(newRoadPosition.x, newRoadPosition.y - 1)))
                    {
                        blockedPositions.Add(new Vector2Int(newRoadPosition.x, newRoadPosition.y - 1));
                        roadPositions.Add(new Vector2Int(newRoadPosition.x, newRoadPosition.y - 1));
                    }
                }

                
                
            }
        }
    }

    void CreateCityBlocks()
    {
        for (int i = 0; i < roomCount; i++)
        {

            bool isAvailable = false;

            GameObject selectedCityBlock = Instantiate(cityManager.cityBlocks[Random.Range(0, cityManager.cityBlocks.Count)], new Vector3(0, 0, 0), Quaternion.identity);
            CityBlock currentCityBlock = selectedCityBlock.GetComponent<CityBlock>();

            for (int a = 0; a < maxGenerationAttempts; a++)
            {
                if (!isAvailable)
                {
                    Vector2Int position = CityGeneratorUtils.GetRandomPosition(gridWidth,gridHeight);

                    if (gridMap[position] == false)
                    {
                        List<Vector2Int> cityBlockGridPoints = CityGeneratorUtils.FindSurroundingPositions(position, currentCityBlock.dimensions.x, currentCityBlock.dimensions.y, false);
                        List<Vector2Int> cityBlockGridPointsWithMargin = CityGeneratorUtils.FindSurroundingPositions(position, currentCityBlock.dimensions.x, currentCityBlock.dimensions.y, generateMargins);

                        bool isValidPosition = CityGeneratorUtils.isRoomValid(cityBlockGridPointsWithMargin, blockedPositions, cityBlocks, gridWidth, gridHeight, position, currentCityBlock);

                        if (isValidPosition)
                        {
                            isAvailable = true;
                            currentCityBlock.gridPosition = position;
                            selectedCityBlock.transform.position = new Vector3((position.x * gridScale) + transform.position.x, 0, (position.y * gridScale) + transform.position.z);
                            selectedCityBlock.transform.parent = this.transform;

                            /*if (cityBlock.dimensions.x == cityBlock.dimensions.y)
                            {
                                selectedCityBlock.transform.Rotate(Vector3.up, 90 * Random.Range(0, 4));
                            }*/

                            //grid[position.x, position.y] = room;
                            gridMap[position] = true;
                            cityBlocks.Add(currentCityBlock);
                            blockedPositions.AddRange(cityBlockGridPoints);
                            break;
                        }
                    }
                }
            }

            if (!isAvailable)
            {
                Destroy(selectedCityBlock);
            }
        }
    }

    void IdentifyRoadPositions()
    {
        foreach (var position in gridMap)
        {
            if (!blockedPositions.Contains(position.Key))
            {
                roadPositions.Add(position.Key);
            }
        }
    }

    void GenerateRoadMeshes()
    {
        foreach (Vector2Int pos in roadPositions)
        {
            if (pos.x >= 0 && pos.y >= 0)
            {
                GameObject hallwayObj = Instantiate(cityManager.roadPrefab, new Vector3((pos.x * gridScale) + transform.position.x, 0, (pos.y * gridScale) + transform.position.z), Quaternion.identity);
                hallwayObj.transform.parent = this.transform;
                hallwayObj.GetComponent<CityBlock>().gridPosition = pos;
            }
        }
    }

    /*    // Update is called once per frame
        void Update()
        {

        }*/
#if (UNITY_EDITOR)
    public void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(new Vector3(((gridHeight / 2) * gridScale) + transform.position.x, 0, ((gridWidth / 2) * gridScale) + transform.position.z), new Vector3(gridHeight * gridScale, gridScale, gridWidth * gridScale));
    }
#endif
}
