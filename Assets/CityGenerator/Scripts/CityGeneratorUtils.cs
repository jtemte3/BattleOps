using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CityGeneratorUtils : MonoBehaviour
{
    public static Vector2Int GetRandomPosition(int gridWidth, int gridHeight)
    {
        return new Vector2Int(Random.Range((-gridWidth / 2) + 1, (gridWidth / 2) - 1), Random.Range((-gridHeight / 2) + 1, (gridHeight / 2) - 1));
    }

    public static Vector2Int GetRandomPosition(Dictionary<Vector2Int,bool> gridMap)
    {
        List<Vector2Int> openPositions = new List<Vector2Int>();

        foreach(var pos in gridMap)
        {
            if(pos.Value == false)
            {
                openPositions.Add(pos.Key);
            }
        }

        return openPositions[Random.Range(0, openPositions.Count)];
    }

    public static List<Vector2Int> FindSurroundingPositions(Vector2Int position, int length, int width, bool withMargins)
    {
        List<Vector2Int> points = new();
        int searchLength = length -1;
        int searchWidth = width -1;
        int lengthValue = 0;
        int widthValue = 0;
        // (condition) ? expressionTrue :  expressionFalse;
        if (withMargins)
        {
            searchLength = length;
            searchWidth = width;
            lengthValue = -1;
            widthValue = -1;
        }


        for (int i = lengthValue; i <= searchLength; i++)
        {
            for (int j = widthValue; j <= searchWidth; j++)
            {
                Vector2Int localPosition = new(i, j);
                points.Add(position + localPosition);
            }
        }

        return points;

    }

    public static bool isRoomValid(List<Vector2Int> roomList, List<Vector2Int> blockedList, List<CityBlock> cityBlocks, int gridWidth, int gridHeight, Vector2Int position, CityBlock currentBlock)
    {
        foreach (var item in roomList)
        {
            if (blockedList.Contains(item))
            {
                return false;
            }

            if (item.x >= (gridWidth / 2) || item.x <= (-gridWidth / 2) || item.y >= (gridHeight / 2) || item.y <= (-gridHeight / 2))
            {
                return false;
            }

            List<CityBlock> similarBlocks = cityBlocks.FindAll(x => x.blockId == currentBlock.GetId());

            foreach (var block in similarBlocks)
            {
                float distance = Vector2.Distance(position, block.GetPosition());

                if (distance < currentBlock.GetSpacing())
                {
                    return false;
                }
            }
        }
        return true;
    }

    public static bool IsPosInCityRange(Vector2Int pos, CityPreset city)
    {
        bool isValid = true;

        if (pos.x > city.gridWidth / 2 || pos.x < -(city.gridWidth / 2))
        {
            isValid = false;
        }
        if (pos.y > city.gridHeight / 2 || pos.y < -(city.gridHeight / 2))
        {
            isValid = false;
        }

        return isValid;
    }
}
