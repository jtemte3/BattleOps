using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CityGenerator : MonoBehaviour
{
    public bool spawnPlayer;
    public GameObject playerPrefab;
    public CityManager cityManager;
    public GameObject entryRoom;

    public int gridWidth = 10;
    public int gridHeight = 10;
    public int roomCount = 10;
    public int gridScale = 5;

    //public bool[,] grid;
    public Dictionary<Vector2Int, bool> gridMap = new Dictionary<Vector2Int, bool>();
    public List<CityBlock> cityBlocks;
    public List<Vector2Int> blockedPositions;
    public List<Vector2Int> roadPositions;
    public int maxGenerationAttempts = 10;
    private Dictionary<CityBlock, List<CityBlock>> cityConnections = new Dictionary<CityBlock, List<CityBlock>>();
    public int cityConnectionCount = 0;

    AStarPathfinding pathfinder = new AStarPathfinding();
    //public HallwayInstantiator hallwayInstantiator;

    [Tooltip("Events to trigger when the generation is finished")]
    public UnityEvent OnGenerationFinished;
    public UnityEvent onGenerationFinished => OnGenerationFinished;

    void Start()
    {

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

    void GenerateCity()
    {
        // Step 1: Setup Entry Points

        // Step 2: Generate Rooms
        CreateCityBlocks();

        // Step 3: Connect Rooms to the Entry Room (roomPosition[0])
        ConnectCityBlocks();
        //ConnectCityBlocksToEntry(cityBlockPositions[0]);

        // Step 4: Connect Rooms with Hallways
        CreateRoadPaths();
        //BuildRoadPathsRemake();
        //BuildRoadPaths();

        // Step 5: Update Hallway Models
        //hallwayInstantiator.PlaceHallwayPrefabs(hallwayPositions, blockedPositions, gridScale);
        GenerateHallwayMeshes();

        // Step 6: Spawn Player Prefab or invoke OnGenerationFinished()
        if (spawnPlayer)
        {
            SpawnPlayerPrefab(cityBlocks[0]);
        }
        else
        {
            onGenerationFinished.Invoke();
        }
        
        
    }

    Vector2Int GetRandomPosition()
    {
        return new Vector2Int(Random.Range(1, gridWidth-1), Random.Range(1, gridHeight-1));
    }

    CityBlock GetRandomRoom(List<CityBlock> cityBlocks, HashSet<CityBlock> excludeSet)
    {
        List<CityBlock> possibleCityBlock = new List<CityBlock>();
        possibleCityBlock.AddRange(cityBlocks);

        CityBlock selectedCityBlock = possibleCityBlock[0];

        while (possibleCityBlock.Count > 0)
        {
            selectedCityBlock = possibleCityBlock[Random.Range(0, possibleCityBlock.Count)];
            if (excludeSet.Contains(selectedCityBlock))
            {
                possibleCityBlock.Remove(selectedCityBlock);
            }
            else
            {
                break;
            }
        }

        return selectedCityBlock;
    }

    void CreateCityBlocks()
    {
        for (int i = 0; i < roomCount; i++)
        {
            
            if (i == 0)
            {
                GameObject selectedCityBlock = Instantiate(entryRoom, new Vector3(0, 0, 0), Quaternion.identity);
                CityBlock cityBlock = selectedCityBlock.GetComponent<CityBlock>();

                Vector2Int position = new Vector2Int(gridWidth / 2, 0);
                List<Vector2Int> cityBlockGridPoints = FindSurroundingPositions(position, cityBlock.dimensions.x, cityBlock.dimensions.y, false);

                cityBlock.gridPosition = position;
                selectedCityBlock.transform.position = new Vector3((position.x * gridScale) + transform.position.x, 0, (position.y * gridScale) + transform.position.z);
                selectedCityBlock.transform.parent = this.transform;
                //grid[position.x, position.y] = room;
                gridMap[position] = true;
                cityBlocks.Add(cityBlock);
                blockedPositions.AddRange(cityBlockGridPoints);
            }
            else
            {
                bool isAvailable = false;

                GameObject selectedCityBlock = Instantiate(cityManager.cityBlocks[Random.Range(0, cityManager.cityBlocks.Count)], new Vector3(0, 0, 0), Quaternion.identity);
                CityBlock cityBlock = selectedCityBlock.GetComponent<CityBlock>();

                for (int a = 0; a < maxGenerationAttempts; a++)
                {
                    if (!isAvailable)
                    {
                        Vector2Int position = GetRandomPosition();

                        if (gridMap[position] == false)
                        {
                            List<Vector2Int> cityBlockGridPoints = FindSurroundingPositions(position, cityBlock.dimensions.x, cityBlock.dimensions.y, false);
                            List<Vector2Int> cityBlockGridPointsWithMargin = FindSurroundingPositions(position, cityBlock.dimensions.x-1, cityBlock.dimensions.y-1, true);

                            bool isValidPosition = isRoomValid(cityBlockGridPointsWithMargin, blockedPositions);

                            if (isValidPosition)
                            {
                                isAvailable = true;
                                cityBlock.gridPosition = position;
                                selectedCityBlock.transform.position = new Vector3((position.x * gridScale) + transform.position.x, 0, (position.y * gridScale) + transform.position.z);
                                selectedCityBlock.transform.parent = this.transform;
                                //grid[position.x, position.y] = room;
                                gridMap[position] = true;
                                cityBlocks.Add(cityBlock);
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
    }

    void ConnectCityBlocks()
    {
        foreach (CityBlock block in cityBlocks)
        {
            List<CityBlock> previuslyConnectedBlocks = new();
            
            previuslyConnectedBlocks.Add(block);

            int ConnectionCount = Random.Range(0, 5);

            for(int i = 0; i<= ConnectionCount; i++)
            {
                bool isValidConnection = false;
                CityBlock connectedBlock = null;

                while (isValidConnection == false)
                {
                    connectedBlock = cityBlocks[Random.Range(0, cityBlocks.Count)];

                    if (!previuslyConnectedBlocks.Contains(connectedBlock))
                    {
                        isValidConnection = true;
                    }
                }

                block.connectedBlocks.Add(connectedBlock);

                previuslyConnectedBlocks.Add(connectedBlock);
            }
        }
    }

    void ConnectCityBlocksToEntry(CityBlock entryRoom)
    {
        // Initialize the connections dictionary
/*        foreach (var room in cityBlockPositions)
        {
            cityConnections.Add(room, new List<CityBlock>());
        }*/

        // Keep track of connected rooms
        //HashSet<CityBlock> connectedCityBlocks = new HashSet<CityBlock> { entryRoom };

        // Step 1: Connect two rooms directly to the entry room
        for (int i = 0; i < 2; i++)
        {
            CityBlock blockToConnect = cityBlocks[Random.Range(1, cityBlocks.Count)];
            cityConnections[entryRoom].Add(blockToConnect);
            cityConnections[blockToConnect].Add(entryRoom);
            //connectedCityBlocks.Add(blockToConnect);
        }

        // Step 2: Connect remaining rooms to already connected rooms
        foreach (CityBlock block in cityBlocks)
        {
            CityBlock connectedBlock = cityBlocks[Random.Range(1, cityBlocks.Count)];

            // Connect the room to a connected room
            cityConnections[block].Add(connectedBlock);
            cityConnections[connectedBlock].Add(block);
            cityConnectionCount++;
        }

        // Step 2: Connect remaining rooms to already connected rooms
        /*while (connectedCityBlocks.Count < cityBlockPositions.Count)
        {
            CityBlock blockToConnect = GetRandomRoom(cityBlockPositions, connectedCityBlocks);
            CityBlock connectedBlock = GetRandomRoom(new List<CityBlock>(connectedCityBlocks), connectedCityBlocks);

            // Connect the room to a connected room
            cityConnections[blockToConnect].Add(connectedBlock);
            cityConnections[connectedBlock].Add(blockToConnect);
            connectedCityBlocks.Add(blockToConnect);
        }*/
    }

    void BuildRoadPaths()
    {
        foreach (var room in cityConnections)
        {
            int doorPos = Random.Range(0, room.Key.doors.Count);
            Door SD = room.Key.doors[doorPos];
            Vector2Int startPos = room.Key.gridPosition + SD.GetPathingVector();

            foreach (var connectedBlock in room.Value)
            {
                Door ED = connectedBlock.doors[Random.Range(0, connectedBlock.doors.Count)];
                Vector2Int endPos = connectedBlock.gridPosition + ED.GetPathingVector();
                List<Vector2Int> hallwayPath = pathfinder.FindPath(startPos, endPos, blockedPositions, roadPositions, gridWidth, gridHeight);

                SD.SetHallwayPath(hallwayPath);

                if (hallwayPath != null && hallwayPath.Count > 0)
                {
                    foreach (Vector2Int pos in hallwayPath)
                    {
                        if (gridMap[pos] == false)
                        {
                            //grid[pos.x, pos.y] = hallwayObj.GetComponent<RoomTwo>();
                            gridMap[pos] = true;
                            roadPositions.Add(pos);
                        }
                    }
                    SD.SetPathingState(true);
                    ED.SetPathingState(true);
                }
                else
                {
                    Debug.Log("hallwayPath = null");
                }
            }

            /*bool isConnected = false;

            while (isConnected == false)
            {
                

                *//*int maxDoors = room.Key.doors.Count;
                int minDoors = Mathf.CeilToInt(maxDoors / 2);*//*

                int connectedCount = 0;
                
                foreach (Door door in room.Key.doors)
                {
                    if (door.hasPath)
                    {
                        connectedCount++;
                    }
                }

                if (connectedCount == room.Key.doors.Count)
                {
                    isConnected = true;
                }
            }*/
        }
    }

    void BuildRoadPathsRemake()
    {
        foreach (var room in cityBlocks)
        {
            int doorPos = Random.Range(0, room.doors.Count);

            Debug.Log(room.gameObject.name +":"+ doorPos);

            Door startingDoor = room.doors[doorPos];
            Vector2Int startPos = room.gridPosition + startingDoor.GetPathingVector();

            foreach (var otherRoom in cityBlocks)
            {
                Door endingDoor = otherRoom.doors[Random.Range(0, otherRoom.doors.Count)];
                Vector2Int endPos = otherRoom.gridPosition + endingDoor.GetPathingVector();
                List<Vector2Int> hallwayPath = pathfinder.FindPath(startPos, endPos, blockedPositions, roadPositions, gridWidth, gridHeight);

                startingDoor.SetHallwayPath(hallwayPath);

                if (hallwayPath != null && hallwayPath.Count > 0)
                {
                    foreach (Vector2Int pos in hallwayPath)
                    {
                        if (gridMap[pos] == false)
                        {
                            //grid[pos.x, pos.y] = hallwayObj.GetComponent<RoomTwo>();
                            gridMap[pos] = true;
                            roadPositions.Add(pos);
                        }
                    }
                    startingDoor.SetPathingState(true);
                    endingDoor.SetPathingState(true);
                }
                else
                {
                    Debug.Log("hallwayPath = null");
                }
            }
        }
    }

    void CreateRoadPaths()
    {
        foreach (CityBlock block in cityBlocks)
        {
            Door startingDoor = block.doors[Random.Range(0, block.doors.Count)];
            Vector2Int startPos = block.gridPosition + startingDoor.GetPathingVector();

            foreach (CityBlock connectionBlock in block.connectedBlocks)
            {
                Door endingDoor = connectionBlock.doors[Random.Range(0, connectionBlock.doors.Count)];
                Vector2Int endingPos = connectionBlock.gridPosition + endingDoor.GetPathingVector();

                List<Vector2Int> path = pathfinder.FindPath(startPos, endingPos, blockedPositions, roadPositions, gridWidth, gridHeight);

                if (path != null)
                {
                    //startingDoor.SetHallwayPath(path);
                    startingDoor.SetPathingState(true);
                    endingDoor.SetPathingState(true);
                    foreach (Vector2Int position in path)
                    {
                        if (!roadPositions.Contains(position))
                        {
                            roadPositions.Add(position);
                        }
                    }
                }
                else
                {
                    startingDoor = block.doors[0];
                    startPos = block.gridPosition + startingDoor.GetPathingVector();

                    endingDoor = connectionBlock.doors[0];
                    endingPos = connectionBlock.gridPosition + endingDoor.GetPathingVector();

                    path = pathfinder.FindPath(startPos, endingPos, blockedPositions, roadPositions, gridWidth, gridHeight);

                    if (path != null)
                    {
                        //startingDoor.SetHallwayPath(path);
                        startingDoor.SetPathingState(true);
                        endingDoor.SetPathingState(true);
                        foreach (Vector2Int position in path)
                        {
                            if (!roadPositions.Contains(position))
                            {
                                roadPositions.Add(position);
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("Error pathfinding, result = null");
                    }
                }
            }
        }
    }

    void GenerateHallwayMeshes()
    {
        foreach (Vector2Int pos in roadPositions)
        {
            GameObject hallwayObj = Instantiate(cityManager.roadPrefab, new Vector3((pos.x * gridScale) + transform.position.x, 0, (pos.y * gridScale) + transform.position.z), Quaternion.identity);
            hallwayObj.transform.parent = this.transform;
            hallwayObj.GetComponent<CityBlock>().gridPosition = pos;
        }
    }

    void SpawnPlayerPrefab(CityBlock room)
    {
        GameObject entryPointObj = Instantiate(playerPrefab, new Vector3((room.gridPosition.x * gridScale)+transform.position.x, 0, (room.gridPosition.y * gridScale)+transform.position.z), Quaternion.identity);
        //entryPointObj.transform.localScale = new Vector3(roomSize.x, .1f, roomSize.y);
        entryPointObj.name = "EntryPoint";
    }

    List<Vector2Int> FindSurroundingPositions(Vector2Int position, int length, int width, bool withMargins)
    {
        List<Vector2Int> points = new();
        int searchLength = length -1;
        int searchWidth = width -1;
        int iValue = 0;
        int jValue = 0;
        // (condition) ? expressionTrue :  expressionFalse;
        if (withMargins)
        {
            searchLength = length+1;
            searchWidth = width+1;
            iValue = -2;
            jValue = -2;
        }
       

        for (int i = iValue ; i <= searchLength; i++)
        {
            for (int j = jValue; j <= searchWidth; j++)
            {
                Vector2Int localPosition = new(i, j);
                points.Add(position + localPosition);
            }
        }

        return points;

    }

    bool isRoomValid(List<Vector2Int> roomList, List<Vector2Int> blockedList)
    {
        foreach (var item in roomList)
        {
            if (blockedList.Contains(item))
            {
                return false;
            }

            if (item.x >= gridWidth || item.y >= gridHeight)
            {
                return false;
            }
        }
        return true;
    }

    /*bool IsWithinBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight;
    }*/
#if (UNITY_EDITOR)
    public void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(new Vector3(((gridHeight / 2) * gridScale)+transform.position.x, 0, ((gridWidth / 2) * gridScale)+transform.position.z), new Vector3(gridHeight * gridScale, gridScale, gridWidth * gridScale));
    }
#endif
}
