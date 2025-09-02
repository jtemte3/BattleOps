using System.Collections.Generic;
using UnityEngine;

public class RoadSegmentTracer : MonoBehaviour
{
    private Dictionary<Vector2Int, List<Vector2Int>> roadGraph;
    private HashSet<Vector2Int> intersectionSet;

    public HashSet<(Vector2Int, Vector2Int)> visitedConnections;
    public List<List<Vector2Int>> segments { get; private set; }

    public List<List<Vector2Int>> TraceAll(Dictionary<Vector2Int, List<Vector2Int>> graph,
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
                    continue; // don’t retrace same connection

                var segment = TraceSegment(intersection, neighbor);

                if (segment != null && segment.Count > 1)
                {
                    var start = segment[0];
                    var end = segment[segment.Count - 1];

                    if (!ConnectionVisited(start, end))
                    {
                        MarkConnection(start, end);
                        segments.Add(segment);
                    }
                }
            }
        }

        return segments;
    }

    private List<Vector2Int> TraceSegment(Vector2Int startIntersection, Vector2Int firstStep)
    {
        var path = new List<Vector2Int> { startIntersection, firstStep };

        var prev = startIntersection;
        var current = firstStep;

        int safety = 0; // safeguard against infinite/very long loops
        while (true)
        {
            if (++safety > 10000)
                return null;

            // if current is an intersection (but not the start) → segment ends
            if (intersectionSet.Contains(current) && current != startIntersection)
                return path;

            // otherwise continue along the road
            var neighbors = roadGraph[current];
            Vector2Int next = Vector2Int.zero;
            bool found = false;

            foreach (var n in neighbors)
            {
                if (n == prev) continue; // don’t go back
                next = n;
                found = true;
                break;
            }

            if (!found) // dead end
                return null;

            path.Add(next);
            prev = current;
            current = next;
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
