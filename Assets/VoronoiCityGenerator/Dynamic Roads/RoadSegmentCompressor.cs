using System.Collections.Generic;
using UnityEngine;

public class RoadSegmentCompressor : MonoBehaviour
{
    private Dictionary<Vector2Int, List<Vector2Int>> roadGraph;
    private HashSet<Vector2Int> intersectionSet;

    public List<List<Vector2Int>> segments { get; private set; }
    private HashSet<(Vector2Int, Vector2Int)> visitedConnections;

    public List<List<Vector2Int>> BuildCompressedGraph(Dictionary<Vector2Int, List<Vector2Int>> graph,
        List<Vector2Int> knownIntersections)
    {
        roadGraph = graph;
        intersectionSet = new HashSet<Vector2Int>(knownIntersections);

        visitedConnections = new HashSet<(Vector2Int, Vector2Int)>();
        segments = new List<List<Vector2Int>>();

        foreach (var intersection in intersectionSet)
        {
            foreach (var neighbor in roadGraph[intersection])
            {
                if (ConnectionVisited(intersection, neighbor))
                    continue;

                var path = WalkUntilIntersection(intersection, neighbor);
                if (path != null && path.Count > 1)
                {
                    var start = path[0];
                    var end = path[path.Count - 1];
                    MarkConnection(start, end);
                    segments.Add(path);
                }
            }
        }

        return segments;
    }

    private List<Vector2Int> WalkUntilIntersection(Vector2Int start, Vector2Int next)
    {
        var path = new List<Vector2Int> { start, next };

        var prev = start;
        var current = next;

        int safety = 0;
        while (true)
        {
            if (++safety > 10000)
                return null;

            if (intersectionSet.Contains(current) && current != start)
                return path;

            var neighbors = roadGraph[current];
            Vector2Int forward = Vector2Int.zero;
            bool found = false;

            foreach (var n in neighbors)
            {
                if (n == prev) continue;
                forward = n;
                found = true;
                break;
            }

            if (!found)
                return null; // dead end

            path.Add(forward);
            prev = current;
            current = forward;
        }
    }

    private bool ConnectionVisited(Vector2Int a, Vector2Int b)
        => visitedConnections.Contains((a, b)) || visitedConnections.Contains((b, a));

    private void MarkConnection(Vector2Int a, Vector2Int b)
    {
        visitedConnections.Add((a, b));
        visitedConnections.Add((b, a));
    }
}
