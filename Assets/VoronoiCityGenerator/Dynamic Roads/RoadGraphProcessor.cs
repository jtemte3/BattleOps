using System.Collections.Generic;
using UnityEngine;

public static class RoadGraphProcessor
{
    // Step 1. Convert dictionary to edge list
    public static List<(Vector2, Vector2)> BuildEdgeList(Dictionary<Vector2Int, List<Vector2Int>> graph)
    {
        var edges = new List<(Vector2, Vector2)>();
        var visited = new HashSet<(Vector2Int, Vector2Int)>();

        foreach (var kvp in graph)
        {
            foreach (var n in kvp.Value)
            {
                if (visited.Contains((kvp.Key, n)) || visited.Contains((n, kvp.Key)))
                    continue;

                visited.Add((kvp.Key, n));
                edges.Add((kvp.Key, n));
            }
        }
        return edges;
    }

    // Step 2. Intersection test
    private static bool SegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2, out Vector2 intersection)
    {
        intersection = Vector2.zero;

        float A1 = p2.y - p1.y;
        float B1 = p1.x - p2.x;
        float C1 = A1 * p1.x + B1 * p1.y;

        float A2 = q2.y - q1.y;
        float B2 = q1.x - q2.x;
        float C2 = A2 * q1.x + B2 * q1.y;

        float det = A1 * B2 - A2 * B1;
        if (Mathf.Abs(det) < 0.0001f) return false; // parallel

        float x = (B2 * C1 - B1 * C2) / det;
        float y = (A1 * C2 - A2 * C1) / det;
        intersection = new Vector2(x, y);

        bool onP = x >= Mathf.Min(p1.x, p2.x) && x <= Mathf.Max(p1.x, p2.x) &&
                   y >= Mathf.Min(p1.y, p2.y) && y <= Mathf.Max(p1.y, p2.y);
        bool onQ = x >= Mathf.Min(q1.x, q2.x) && x <= Mathf.Max(q1.x, q2.y) &&
                   y >= Mathf.Min(q1.y, q2.y) && y <= Mathf.Max(q1.y, q2.y);

        return onP && onQ;
    }

    // Step 3. Split edges at intersections
    public static List<(Vector2, Vector2)> SplitEdges(List<(Vector2, Vector2)> edges)
    {
        var newEdges = new List<(Vector2, Vector2)>(edges);
        var toAdd = new List<(Vector2, Vector2)>();
        var toRemove = new List<(Vector2, Vector2)>();

        for (int i = 0; i < newEdges.Count; i++)
        {
            for (int j = i + 1; j < newEdges.Count; j++)
            {
                var e1 = newEdges[i];
                var e2 = newEdges[j];

                if (SegmentsIntersect(e1.Item1, e1.Item2, e2.Item1, e2.Item2, out var inter))
                {
                    // If intersection is already one of the endpoints, ignore
                    if (inter == e1.Item1 || inter == e1.Item2 ||
                        inter == e2.Item1 || inter == e2.Item2)
                        continue;

                    // Mark originals for removal
                    toRemove.Add(e1);
                    toRemove.Add(e2);

                    // Split edge1
                    toAdd.Add((e1.Item1, inter));
                    toAdd.Add((inter, e1.Item2));

                    // Split edge2
                    toAdd.Add((e2.Item1, inter));
                    toAdd.Add((inter, e2.Item2));
                }
            }
        }

        // Apply removals/additions
        foreach (var r in toRemove) newEdges.Remove(r);
        foreach (var a in toAdd) newEdges.Add(a);

        return newEdges;
    }

    // Step 4. Rebuild graph from edge list
    public static Dictionary<Vector2, List<Vector2>> BuildGraph(List<(Vector2, Vector2)> edges)
    {
        var graph = new Dictionary<Vector2, List<Vector2>>();

        foreach (var e in edges)
        {
            if (!graph.ContainsKey(e.Item1)) graph[e.Item1] = new List<Vector2>();
            if (!graph.ContainsKey(e.Item2)) graph[e.Item2] = new List<Vector2>();

            if (!graph[e.Item1].Contains(e.Item2)) graph[e.Item1].Add(e.Item2);
            if (!graph[e.Item2].Contains(e.Item1)) graph[e.Item2].Add(e.Item1);
        }

        return graph;
    }
}
