using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BuildCellMapper
{
    public static Dictionary<Vector2Int, bool> GenerateCellMap(List<Vector2> perimeter, int cellSize, Vector3 anchorPos)
    {
        Dictionary<Vector2Int, bool> cellMap = new Dictionary<Vector2Int, bool>();

        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;
        foreach (var p in perimeter)
        {
            if (p.x < minX) minX = p.x;
            if (p.x > maxX) maxX = p.x;
            if (p.y < minY) minY = p.y;
            if (p.y > maxY) maxY = p.y;
        }

        for (float x = minX; x <= maxX; x += cellSize)
        {
            for (float y = minY; y <= maxY; y += cellSize)
            {
                Vector2 cellCenter = new Vector2(x, y);
                if (PointInPolygon(cellCenter, perimeter))
                {
                    // index relative to anchor
                    int cx = Mathf.RoundToInt((x - anchorPos.x) / cellSize);
                    int cy = Mathf.RoundToInt((y - anchorPos.z) / cellSize);

                    cellMap[new Vector2Int(cx, cy)] = false;
                }
            }
        }

        return cellMap;
    }

    private static bool PointInPolygon(Vector2 point, List<Vector2> polygon)
    {
        bool inside = false;
        for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
        {
            if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
                (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) /
                 (polygon[j].y - polygon[i].y) + polygon[i].x))
            {
                inside = !inside;
            }
        }
        return inside;
    }
}