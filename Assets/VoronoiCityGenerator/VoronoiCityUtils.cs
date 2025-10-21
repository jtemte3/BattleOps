using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoronoiCityUtils : MonoBehaviour
{
    public static Vector2Int GetRandomPosition(Dictionary<Vector2Int, GridCell> grid)
    {
        List<Vector2Int> openPositions = new List<Vector2Int>();

        foreach (var cell in grid)
        {
            if (cell.Value.isBlocked == false)
            {
                openPositions.Add(cell.Key);
            }
        }

        return openPositions[Random.Range(0, openPositions.Count)];
    }

    public static Vector2Int GetRandomPosition(Dictionary<Vector2Int, bool> grid)
    {
        List<Vector2Int> openPositions = new List<Vector2Int>();

        foreach (var cell in grid)
        {
            if (cell.Value == false)
            {
                openPositions.Add(cell.Key);
            }
        }

        return openPositions[Random.Range(0, openPositions.Count)];
    }

    public static Vector2Int GetRandomPosition(RegionData region, List<Vector2Int> blockedPositions, Dictionary<Vector2Int, GridCell> grid)
    {
        List<Vector2Int> openPositions = new List<Vector2Int>();

        foreach (var position in region.regionPositions)
        {
            if (!blockedPositions.Contains(position) && grid.ContainsKey(position))
            {
                openPositions.Add(position);
            }
        }

        if (openPositions.Count > 0)
        {
            return openPositions[Random.Range(0, openPositions.Count)];
        }
        else
        {
            return region.regionCenterPosition;
        }
        
    }

    public static bool isRoomValid(List<Vector2Int> roomList, List<Vector2Int> blockedList, List<GridBuilding> buildings, int gridWidth, int gridHeight, Vector2Int position, GridBuilding currentBlock)
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

            List<GridBuilding> similarBlocks = buildings.FindAll(x => x.blockId == currentBlock.GetId());

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

    public static bool isRoomValid(List<Vector2Int> roomList, Dictionary<Vector2Int,bool> grid, List<GridBuilding> buildings, Vector2Int position, GridBuilding currentBlock)
    {
        foreach (var item in roomList)
        {
            if (grid.ContainsKey(item))
            {
                if (grid[item] == true)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }


                List<GridBuilding> similarBlocks = buildings.FindAll(x => x.blockId == currentBlock.GetId());

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

    public static List<Vector2Int> FindSurroundingPositions(Vector2Int position, int length, int width, bool withMargins)
    {
        List<Vector2Int> points = new();
        int searchLength = length - 1;
        int searchWidth = width - 1;
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

    bool IsInBounds(Vector2Int pos, int width, int height)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }
}
