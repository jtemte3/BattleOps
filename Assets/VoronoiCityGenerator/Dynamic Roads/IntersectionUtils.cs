using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class IntersectionUtils
{
    public static List<Neighbors> FindNeighbors(List<Vector2Int> intersections, List<Vector2Int> roadPositions, List<Vector2Int> directions)
    {
        var result = new List<Neighbors>();

        foreach (var start in intersections)
        {
            Neighbors neighbors = new Neighbors();
            neighbors.position = start;
            List<Vector2Int> found = new List<Vector2Int>();
            Queue<Vector2Int> open = new Queue<Vector2Int>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

            open.Enqueue(start);
            visited.Add(start);

            while (open.Count > 0)
            {
                var current = open.Dequeue();

                foreach (var dir in directions)
                {
                    var next = current + dir;

                    if (visited.Contains(next) || !roadPositions.Contains(next))
                    {
                        continue;
                    }
                        

                    // Another intersection = stop here, record neighbor
                    if (intersections.Contains(next) && next != start)
                    {
                        found.Add(next);
                        visited.Add(next); // prevent re-adding

                        continue; // do not expand past
                    }

                    // Road position = expand search
                    if (roadPositions.Contains(next))
                    {
                        visited.Add(next);
                        open.Enqueue(next);
                    }
                }
            }
            neighbors.neighborPos = found;
            result.Add(neighbors);
        }

        return result;
    }

    public static void GenerateIntersectionPaths(List<Neighbors> intersectionMap, List<Vector2Int> intersectionPositions, List<Vector2Int> roadPositions)
    {
        foreach (var neighbor in intersectionMap)
        {
            if (neighbor.neighborPos.Count == 1)
            {
                List<Vector2Int> path = RoadPathfinder.FindStraightestPath(neighbor.position, neighbor.neighborPos[0], roadPositions, new());
                if (path.Count > 0)
                {
                    neighbor.neighborPaths.Add(path);
                }
            }
            else
            {
                foreach (var neighborPos in neighbor.neighborPos)
                {
                    int index = -1;
                    index = intersectionPositions.IndexOf(neighborPos);
                    List<Vector2Int> updatedIntersections = intersectionPositions;
                    if (index > -1 && index < updatedIntersections.Count)
                    {
                        updatedIntersections.RemoveAt(index);
                    }
                    List<Vector2Int> path = RoadPathfinder.FindStraightestPath(neighbor.position, neighborPos, roadPositions, updatedIntersections);
                    if (path.Count > 0)
                    {
                        neighbor.neighborPaths.Add(path);
                    }
                }
            }
        }
    }

    public static List<Vector2Int> GetIntersectionList(List<Neighbors> intersectionMap)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        foreach (Neighbors neighbor in intersectionMap)
        {
            result.Add(neighbor.position);
        }

        return result;
    }

    public static void ValidateNeighborMap(List<Neighbors> intersectionMap, List<Vector2Int> intersectionPositions, int searchDepth, List<List<Vector2Int>> invalidPaths, List<Vector2Int> directions)
    {
        List<Vector2Int> radiusList = new();
        for (int i = 1; i <= searchDepth; i++)
        {
            foreach (Vector2Int dir in directions)
            {
                radiusList.Add(new Vector2Int(dir.x * i, dir.y));
                radiusList.Add(new Vector2Int(dir.x, dir.y * i));
                radiusList.Add(new Vector2Int(dir.x * i, dir.y * i));
            }
        }


        foreach (var intersectionObj in intersectionMap)
        {
            List<Vector2Int> validNeighbors = new List<Vector2Int>();
            List<List<Vector2Int>> validPaths = new();

            foreach (var path in intersectionObj.neighborPaths)
            {
                bool isPathValid = true;
                if (path.Count > 7)
                {
                    for (int i = 4; i < path.Count - 3; i++)
                    {
                        bool isValid = true;

                        Vector2Int pos = path[i];

                        foreach (var dir in radiusList)
                        {
                            if (intersectionPositions.Contains(pos + dir))
                            {
                                isValid = false;
                                break;
                            }
                        }

                        if (!isValid)
                        {
                            /*Debug.Log(intersectionObj.position + " -> " + path[path.Count - 1] + " is an invalid path");
                            Debug.Log(path.ToSeparatedString(" -> "));*/
                            invalidPaths.Add(path);
                            isPathValid = false;
                            break;
                        }
                    }
                }
                if (!isPathValid)
                {
                    continue;
                }
                else
                {
                    validNeighbors.Add(path[path.Count - 1]);
                    validPaths.Add(path);
                }
            }

            intersectionObj.neighborPos = validNeighbors;
            intersectionObj.neighborPaths = validPaths;
        }
    }
}
