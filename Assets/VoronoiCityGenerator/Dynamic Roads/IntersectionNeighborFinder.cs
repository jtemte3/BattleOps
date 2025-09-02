using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Rendering.FilterWindow;

public class IntersectionNeighborFinder : MonoBehaviour
{
/*    public HashSet<Vector2Int> intersections;   // all intersections
    public HashSet<Vector2Int> roadPositions;   // all road tiles

    public List<Neighbors> neighbors;*/


    public List<Neighbors> FindNeighbors(
        Dictionary<Vector2Int, List<Vector2Int>> map, List<Vector2Int> intersections, List<Vector2Int> roadPositions)
    {
        var result = new List<Neighbors>();

        // 8 intra-cardinal directions
        Vector2Int[] directions = new Vector2Int[]
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

        foreach (var start in intersections)
        {
            /*if (edgePos.Contains(start))
            {
                continue;
            }*/

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

                    if (visited.Contains(next) || !map.ContainsKey(next))
                    {
                        continue;
                    }
                        

                    // Another intersection = stop here, record neighbor
                    if (intersections.Contains(next) && next != start)
                    {
                        found.Add(next);
                        visited.Add(next); // prevent re-adding

                        /*List<Vector2Int> possibleNeighbors = new();
                        foreach (var dirs in directions)
                        {
                            possibleNeighbors.Add(next + dirs);
                        }
                        Queue<Vector2Int> newOpen = new Queue<Vector2Int>();
                        foreach (var element in open)
                        {
                            if (!possibleNeighbors.Contains(element))
                            {
                                newOpen.Enqueue(element);
                            }
                        }*/
                        //open.Clear();
                        //open = newOpen;
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
}
