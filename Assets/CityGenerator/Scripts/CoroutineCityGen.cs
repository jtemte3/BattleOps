using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class CoroutineCityGen : MonoBehaviour
{
    public CityManager cityManager;

    [Header("Options")]
    public bool generateMainRoadMargins = true;
    public bool generateMargins = true;
    public bool generateRoads = true;
    public bool generateFoundations = true;
    public int maxGenerationAttempts = 10;
    public int maxObjectiveGenerationAttempts = 100;
    public int seed = 0;

    [Header("City Layout Variables")]
    public CityPreset city;
    public int gridScale = 3;
    bool shouldRoadOffsetX = false;
    /*bool shouldRoadOffsetY = false;
    bool shouldIntervalOffset = false;*/

    [Header("City Objects")]
    public Dictionary<Vector2Int, bool> gridMap = new Dictionary<Vector2Int, bool>();
    public List<CityBlock> cityBlocks = new List<CityBlock>();
    public List<Vector2Int> blockedPositions = new List<Vector2Int>();
    public List<Vector2Int> roadPositions = new List<Vector2Int>();

    [Header("Generation Status")]
    [Range(0f, 1f)]
    public float generationProgress = 0f;
    private bool generationComplete = false;

    private bool genRoads = false;
    private bool placedObjectives = false;
    private bool genCityBlocks = false;
    private bool genRoadPos = false;
    private bool genRoadMeshes = false;

    [Header("Debug Options")]
    public bool showGizmos = false;

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

        for (int i = -(city.gridHeight / 2); i <= city.gridHeight / 2; i++)
        {
            for (int j = -(city.gridWidth / 2); j <= city.gridWidth / 2; j++)
            {
                gridMap.TryAdd(new Vector2Int(j, i), false);
            }
        }

        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
            {
                List<Vector2Int> cityBlockGridPoints = CityGeneratorUtils.FindSurroundingPositions(child.GetComponent<CityBlock>().gridPosition, child.GetComponent<CityBlock>().dimensions.x, child.GetComponent<CityBlock>().dimensions.y, false);
                blockedPositions.AddRange(cityBlockGridPoints);
            }
        }

        StartCoroutine(GenerateCityCoroutine());
    }

    IEnumerator GenerateCityCoroutine()
    {
        int totalSteps = 5 + city.roomCount + roadPositions.Count; // Estimate
        int currentStep = 0;

        if (generateRoads)
        {
            GenerateMainRoadsTwo();
            currentStep++;
            generationProgress = (float)currentStep / totalSteps;
            yield return null;

            genRoads = true;
        }
        else
        {
            currentStep++;
            generationProgress = (float)currentStep / totalSteps;
            genRoads = true;
        }

        //PlaceObjectives();
        currentStep++;
        generationProgress = (float)currentStep / totalSteps;
        //yield return null;

        placedObjectives = true;

        for (int i = 0; i < city.roomCount; i++)
        {
            CreateSingleCityBlock(i);
            currentStep++;
            generationProgress = (float)currentStep / totalSteps;
            yield return null;

            genCityBlocks = true;
        }

        if (generateRoads)
        {
            if (generateFoundations)
            {
                IdentifyFoundationPositions();
            }
            currentStep++;
            generationProgress = (float)currentStep / totalSteps;
            yield return null;

            genRoadPos = true;

            GenerateRoadMeshes();
            currentStep++;
            generationProgress = (float)currentStep / totalSteps;
            yield return null;

            genRoadMeshes = true;
            /*GenerateRoadMeshesGradually(() => {
                currentStep++;
                generationProgress = Mathf.Clamp01((float)currentStep / totalSteps);
            });*/
        }
        else
        {
            genRoadPos = true;
            genRoadMeshes = true;
        }

        GenerationCompletionCheck();
        yield return null;

        while (!generationComplete)
        {
            yield return null;
        }

        generationProgress = 1f;
    }

    private void GenerateMainRoads()
    {
        foreach (var position in gridMap)
        {
            if (position.Key.x % city.gridRoadIntervalX == 0)
            {
                Vector2Int newRoadPosition = new Vector2Int(position.Key.x, position.Key.y);

                CreateRoadPosition(newRoadPosition);

                /*if (!blockedPositions.Contains(newRoadPosition))
                {
                    blockedPositions.Add(newRoadPosition);
                    roadPositions.Add(newRoadPosition);

                    if (generateMainRoadMargins)
                    {
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
                }*/
            }

            if (position.Key.y % city.gridRoadIntervalY == 0)
            {
                Vector2Int newRoadPosition = new Vector2Int(position.Key.x, position.Key.y);

                CreateRoadPosition(newRoadPosition);
                
                /*if (!blockedPositions.Contains(newRoadPosition))
                {
                    blockedPositions.Add(newRoadPosition);
                    roadPositions.Add(newRoadPosition);
                    if (generateMainRoadMargins)
                    {
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
                    
                }*/
            }
        }
    }

    void GenerateMainRoadsTwo()
    {
        for (int i = -(city.gridHeight / 2); i <= city.gridHeight / 2; i++)
        {
            if (i == 0)
            {
                generateMainRoadMargins = true;
            }
            else
            {
                generateMainRoadMargins = false;
            }

            if (i % city.gridRoadIntervalX == 0)
            {
                int direction = (Random.Range(0, 2) * 2 - 1);

                for (int j = -(city.gridWidth / 2); j <= city.gridWidth / 2; j++)
                {
                    Vector2Int newRoadPosition = new Vector2Int(i, j);
                    if (j % city.gridRoadIntervalX == 0)
                    {
                        if (shouldRoadOffsetX)
                        {
                            shouldRoadOffsetX = false;
                        }
                        else
                        {
                            shouldRoadOffsetX = true;
                        }
                    }

                    if (shouldRoadOffsetX)
                    {
                        newRoadPosition.x = i + city.gridRoadOffset * direction;
                    }

                    CreateRoadPosition(newRoadPosition);
                }
            }
        }

        for (int j = -(city.gridWidth / 2); j <= city.gridWidth / 2; j++)
        {
            if (j == 0)
            {
                generateMainRoadMargins = true;
            }
            else
            {
                generateMainRoadMargins = false;
            }

            if (j % city.gridRoadIntervalX == 0)
            {
                //int direction = (Random.Range(0, 2) * 2 - 1);

                for (int i = -(city.gridHeight / 2); i <= city.gridHeight / 2; i++)
                {
                    Vector2Int newRoadPosition = new(i, j);

                    /*if (i % city.gridRoadIntervalY == 0)
                    {
                        if (shouldRoadOffsetY)
                        {
                            shouldRoadOffsetY = false;
                        }
                        else
                        {
                            shouldRoadOffsetY = true;
                        }
                    }

                    if (shouldRoadOffsetY)
                    {
                        newRoadPosition.y = j + city.gridRoadOffset * direction;
                    }*/

                    CreateRoadPosition(newRoadPosition);
                }
            }
        }
    }

    void CreateRoadPosition(Vector2Int newRoadPosition)
    {
        if (!blockedPositions.Contains(newRoadPosition))
        {
            blockedPositions.Add(newRoadPosition);
            roadPositions.Add(newRoadPosition);
        }
        if (generateMainRoadMargins)
        {
            List<Vector2Int> marginPos = new()
            {
                new Vector2Int(newRoadPosition.x + 1, newRoadPosition.y),
                new Vector2Int(newRoadPosition.x - 1, newRoadPosition.y),
                new Vector2Int(newRoadPosition.x, newRoadPosition.y + 1),
                new Vector2Int(newRoadPosition.x, newRoadPosition.y - 1)
            };

            foreach (Vector2Int pos in marginPos)
            {
                if (CityGeneratorUtils.IsPosInCityRange(pos, city) && !blockedPositions.Contains(pos))
                {
                    blockedPositions.Add(pos);
                    roadPositions.Add(pos);
                }
            }
        }
    }
    
    

    void IdentifyFoundationPositions()
    {
        foreach (var position in gridMap)
        {
            if (!blockedPositions.Contains(position.Key) && !roadPositions.Contains(position.Key))
            {
                roadPositions.Add(position.Key);
            }
        };
    }

    void CreateSingleCityBlock(int index)
    {
        bool isAvailable = false;

        GameObject selectedCityBlock = Instantiate(cityManager.cityBlocks[Random.Range(0, cityManager.cityBlocks.Count)], Vector3.zero, Quaternion.identity);
        CityBlock currentCityBlock = selectedCityBlock.GetComponent<CityBlock>();

        for (int a = 0; a < maxGenerationAttempts; a++)
        {
            if (!isAvailable)
            {
                //Vector2Int position = CityGeneratorUtils.GetRandomPosition(city.gridWidth, city.gridHeight);
                Vector2Int position = CityGeneratorUtils.GetRandomPosition(gridMap);

                if (gridMap[position] == false)
                {
                    List<Vector2Int> cityBlockGridPoints = CityGeneratorUtils.FindSurroundingPositions(position, currentCityBlock.dimensions.x, currentCityBlock.dimensions.y, false);
                    List<Vector2Int> cityBlockGridPointsWithMargin = CityGeneratorUtils.FindSurroundingPositions(position, currentCityBlock.dimensions.x, currentCityBlock.dimensions.y, generateMargins);

                    bool isValidPosition = CityGeneratorUtils.isRoomValid(cityBlockGridPointsWithMargin, blockedPositions, cityBlocks, city.gridWidth, city.gridHeight, position, currentCityBlock);

                    if (isValidPosition)
                    {
                        isAvailable = true;
                        currentCityBlock.gridPosition = position;
                        selectedCityBlock.transform.position = new Vector3((position.x * gridScale) + transform.position.x, 0, (position.y * gridScale) + transform.position.z);
                        selectedCityBlock.transform.parent = this.transform;

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

    void GenerateRoadMeshes()
    {
        foreach (Vector2Int pos in roadPositions.Distinct())
        {
            /*if (pos.x >= 0 && pos.y >= 0)
            {
                GameObject hallwayObj = Instantiate(cityManager.roadPrefab, new Vector3((pos.x * gridScale) + transform.position.x, 0, (pos.y * gridScale) + transform.position.z), Quaternion.identity);
                hallwayObj.transform.parent = this.transform;
                hallwayObj.GetComponent<CityBlock>().gridPosition = pos;
            }*/

            GameObject hallwayObj = Instantiate(cityManager.roadPrefab, new Vector3((pos.x * gridScale) + transform.position.x, 0, (pos.y * gridScale) + transform.position.z), Quaternion.identity);
            hallwayObj.transform.parent = this.transform;
            hallwayObj.GetComponent<CityBlock>().gridPosition = pos;
        }
    }

/*    void PlaceObjectives()
    {
        bool isAvailable = false;

        GameObject selectedObjBlock = Instantiate(cityManager.objectives[0], Vector3.zero, Quaternion.identity);
        CityBlock currentObjBlock = selectedObjBlock.GetComponent<CityBlock>();

        for (int a = 0; a < maxObjectiveGenerationAttempts; a++)
        {
            if (!isAvailable)
            {
                //Vector2Int position = CityGeneratorUtils.GetRandomPosition(city.gridWidth, city.gridHeight);
                Vector2Int position = CityGeneratorUtils.GetRandomPosition(gridMap);

                if (gridMap[position] == false)
                {
                    List<Vector2Int> cityBlockGridPoints = CityGeneratorUtils.FindSurroundingPositions(position, currentObjBlock.dimensions.x, currentObjBlock.dimensions.y, false);
                    List<Vector2Int> cityBlockGridPointsWithMargin = CityGeneratorUtils.FindSurroundingPositions(position, currentObjBlock.dimensions.x, currentObjBlock.dimensions.y, generateMargins);

                    bool isValidPosition = CityGeneratorUtils.isRoomValid(cityBlockGridPointsWithMargin, blockedPositions, cityBlocks, city.gridWidth, city.gridHeight, position, currentObjBlock);

                    if (isValidPosition)
                    {
                        isAvailable = true;
                        currentObjBlock.gridPosition = position;
                        selectedObjBlock.transform.position = new Vector3((position.x * gridScale) + transform.position.x, 0, (position.y * gridScale) + transform.position.z);
                        selectedObjBlock.transform.parent = this.transform;

                        gridMap[position] = true;
                        cityBlocks.Add(currentObjBlock);
                        blockedPositions.AddRange(cityBlockGridPoints);
                        break;
                    }
                }
            }
        }
    }*/

    /*void GenerateRoadMeshesGradually(System.Action onCompleteStep)
    {
        StartCoroutine(GenerateRoadMeshesCoroutine(onCompleteStep));
    }

    IEnumerator GenerateRoadMeshesCoroutine(System.Action onCompleteStep)
    {
        foreach (Vector2Int pos in roadPositions)
        {
            if (pos.x >= 0 && pos.y >= 0)
            {
                GameObject roadObj = Instantiate(cityManager.roadPrefab, new Vector3((pos.x * gridScale) + transform.position.x, 0, (pos.y * gridScale) + transform.position.z), Quaternion.identity);
                roadObj.transform.parent = this.transform;
                roadObj.GetComponent<CityBlock>().gridPosition = pos;
            }

            onCompleteStep?.Invoke();
            yield return null; // wait a frame
        }

        generationComplete = true;
    }*/

    private void GenerationCompletionCheck()
    {
        if (genRoads == true && genCityBlocks == true && genRoadPos == true && genRoadMeshes == true && placedObjectives == true)
        {
            generationComplete = true;
        }
        else
        {
            generationComplete = false;
        }
    }
#if (UNITY_EDITOR)
    public void OnDrawGizmos()
    {
        if (showGizmos)
        {
            if (city)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(transform.position, new Vector3(city.gridHeight * gridScale, 0, city.gridWidth * gridScale));
                //Gizmos.DrawWireCube(new Vector3(((city.gridHeight / 2) * gridScale) + transform.position.x, 0, ((city.gridWidth / 2) * gridScale) + transform.position.z), new Vector3(city.gridHeight * gridScale, gridScale, city.gridWidth * gridScale));

                Gizmos.color = new Color(1, 1, 1, .1f);

                if (city.gridWidth % 10 == 0)
                {
                    for (int x = (int)(-(city.gridWidth * 1.5) - gridScale); x <= (city.gridWidth * 1.5); x += 3)
                    {
                        Vector3 startPos = new Vector3(x + 1.5f, 0, (float)((city.gridWidth * 1.5) + 1.5f));
                        Vector3 endPos = new Vector3(x + 1.5f, 0, (float)((city.gridWidth * -1.5) - 1.5f));
                        Gizmos.DrawLine(startPos, endPos);
                    }

                    for (int y = (int)(-(city.gridHeight * 1.5) - gridScale); y <= (city.gridHeight * 1.5); y += 3)
                    {
                        Vector3 startPos = new Vector3((float)((city.gridHeight * 1.5) + 1.5f), 0, (float)y + 1.5f);
                        Vector3 endPos = new Vector3((float)((city.gridHeight * -1.5) - 1.5f), 0, (float)y + 1.5f);
                        Gizmos.DrawLine(startPos, endPos);
                    }
                }
                else if (city.gridWidth % 10 == 5)
                {
                    for (int x = (int)(-(city.gridHeight * 1.5) - gridScale); x <= (city.gridHeight * 1.5); x += gridScale)
                    {
                        Vector3 startPos = new Vector3(x + 2.5f, 0, city.gridHeight * 1.5f);
                        Vector3 endPos = new Vector3(x + 2.5f, 0, -city.gridHeight * 1.5f);
                        Gizmos.DrawLine(startPos, endPos);
                    }

                    for (int y = (int)(-(city.gridHeight * 1.5) - gridScale); y <= (city.gridHeight * 1.5); y += gridScale)
                    {
                        Vector3 startPos = new Vector3(city.gridHeight * 1.5f, 0, y + 2.5f);
                        Vector3 endPos = new Vector3(-city.gridHeight * 1.5f, 0, y + 2.5f);
                        Gizmos.DrawLine(startPos, endPos);
                    }
                }
                /*for (int y = -48; y <= 45; y += 3)
                {
                    Vector3 startPos = new Vector3(46.5f, 0, y + 1.5f);
                    Vector3 endPos = new Vector3(-46.5f, 0, y + 1.5f);
                    Gizmos.DrawLine(startPos, endPos);
                }*/
            }
        }
    }
#endif
}
