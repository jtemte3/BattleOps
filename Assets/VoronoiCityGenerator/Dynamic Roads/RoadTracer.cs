using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;

public class RoadTracer : MonoBehaviour
{
    // Input: Dictionary of road positions -> their neighbors
    private Dictionary<Vector2Int, List<Vector2Int>> roadGraph;

    // Output: List of traced road segments
    public List<List<Vector2Int>> segments { get; private set; }

    // Track visited edges (so we don’t duplicate segments)
    private HashSet<(Vector2Int, Vector2Int)> visitedEdges;

    public List<List<Vector2Int>> Trace(Dictionary<Vector2Int, List<Vector2Int>> graph)
    {
        roadGraph = graph;
        segments = new List<List<Vector2Int>>();
        visitedEdges = new HashSet<(Vector2Int, Vector2Int)>();
        TraceAll();

        return segments;
    }

    private void TraceAll()
    {
        foreach (var node in roadGraph.Keys)
        {
            foreach (var neighbor in roadGraph[node])
            {
                // Check if edge was already processed
                if (visitedEdges.Contains((node, neighbor)) || visitedEdges.Contains((neighbor, node)))
                    continue;

                // Start tracing a segment
                var segment = TraceSegment(node, neighbor);
                if (segment.Count > 1)
                {
                    segments.Add(segment);
                }
                    
            }
        }
    }

    private List<Vector2Int> TraceSegment(Vector2Int start, Vector2Int next)
    {
        List<Vector2Int> segment = new List<Vector2Int>();
        Vector2Int prev = start;
        Vector2Int current = next;

        // Add first node of the segment
        segment.Add(prev);

        while (true)
        {
            // Register the edge as visited
            visitedEdges.Add((prev, current));
            visitedEdges.Add((current, prev));

            segment.Add(current);

            var neighbors = roadGraph[current];
            if (neighbors.Count != 2) // stop at intersections or dead ends
                break;

            // Continue straight line: pick the neighbor that's not the previous node
            Vector2Int nextNode = neighbors[0] == prev ? neighbors[1] : neighbors[0];

            if (visitedEdges.Contains((current, nextNode)))
                break; // already processed this edge

            prev = current;
            current = nextNode;
        }

        return segment;
    }
}
