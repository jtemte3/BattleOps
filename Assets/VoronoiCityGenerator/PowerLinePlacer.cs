using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PowerLinePlacer : MonoBehaviour
{
    [Header("References")]
    public GridVoronoiCity cityGenerator; // Reference to your grid/voronoi generator
    public GameObject powerPolePrefab;
    public Material wireMaterial;

    [Header("Settings")]
    public float poleHeight = 6f;
    public float sagAmount = 1.5f;
    public float poleSpacing = 10f; // world units between poles
    public int wireSegments = 16;

    private readonly Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

    public void PlacePowerLines()
    {
        if (cityGenerator == null || powerPolePrefab == null || cityGenerator.roadPositions == null) return;

        // clear previous poles/wires
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        // Build a set of road cells for fast lookup
        var roadSet = new HashSet<Vector2Int>(cityGenerator.roadPositions);

        // Step 1: build edge list = (roadCell, sideCell)
        var edges = new List<(Vector2Int roadCell, Vector2Int sideCell)>();
        foreach (var road in cityGenerator.roadPositions)
        {
            foreach (var dir in directions)
            {
                Vector2Int neighbor = road + dir;
                if (!roadSet.Contains(neighbor))
                {
                    edges.Add((road, neighbor));
                }
            }
        }

        if (edges.Count == 0) return;

        // Build a hashset for fast contains checks
        var edgeSet = new HashSet<(Vector2Int, Vector2Int)>(edges.Select(e => (e.roadCell, e.sideCell)));

        // Step 2: group edges into continuous runs
        var runs = GroupContinuousEdges(edges, edgeSet);

        // Step 3: for each run, randomly decide whether to place poles on it
        foreach (var run in runs)
        {
            // randomize which runs get poles for variety
            if (Random.value > 0.5f) continue;

            // order the run so poles are placed along the path
            var orderedRun = OrderRunByRoadAdjacency(run);

            var placedPolePositions = new List<Vector3>();
            Vector3 lastPlaced = Vector3.positiveInfinity;

            foreach (var edge in orderedRun)
            {
                // convert side cell to world position (ground-level)
                Vector3 sideWorld = new Vector3(edge.sideCell.x * cityGenerator.gridSize, 0f, edge.sideCell.y * cityGenerator.gridSize);

                // ensure spacing
                if (placedPolePositions.Count == 0 || Vector3.Distance(new Vector3(lastPlaced.x, 0, lastPlaced.z), sideWorld) >= poleSpacing)
                {
                    GameObject pole = Instantiate(powerPolePrefab, sideWorld, Quaternion.identity, transform);
                    // raise pole so its bottom sits on ground (adjust depending on prefab pivot)
                    pole.transform.position += Vector3.up * (poleHeight / 2f);
                    var polePos = pole.transform.position;
                    placedPolePositions.Add(polePos);
                    lastPlaced = sideWorld;
                }
            }

            // Step 4: connect consecutive poles with sagging wires
            for (int i = 0; i < placedPolePositions.Count - 1; i++)
            {
                CreateWire(placedPolePositions[i], placedPolePositions[i + 1]);
            }
        }
    }

    // Groups continuous edges into runs using BFS and returns a list of runs.
    List<List<(Vector2Int roadCell, Vector2Int sideCell)>> GroupContinuousEdges(
        List<(Vector2Int roadCell, Vector2Int sideCell)> edges,
        HashSet<(Vector2Int, Vector2Int)> edgeSet)
    {
        var runs = new List<List<(Vector2Int, Vector2Int)>>();
        var visitedSide = new HashSet<Vector2Int>();

        foreach (var edge in edges)
        {
            if (visitedSide.Contains(edge.sideCell)) continue;

            var run = new List<(Vector2Int, Vector2Int)>();
            var queue = new Queue<(Vector2Int, Vector2Int)>();
            queue.Enqueue((edge.roadCell, edge.sideCell));

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var currentSide = current.Item2;

                if (visitedSide.Contains(currentSide)) continue;

                visitedSide.Add(currentSide);
                run.Add((current.Item1, current.Item2));

                // explore neighboring edges that continue the line (shift both road and side by same dir)
                foreach (var dir in directions)
                {
                    Vector2Int nextRoad = current.Item1 + dir;
                    Vector2Int nextSide = current.Item2 + dir;
                    var nextEdge = (nextRoad, nextSide);
                    if (edgeSet.Contains(nextEdge) && !visitedSide.Contains(nextSide))
                    {
                        queue.Enqueue(nextEdge);
                    }
                }
            }

            if (run.Count > 0)
                runs.Add(run);
        }

        return runs;
    }

    // Try to order a run by walking roadCell adjacency. Returns an ordered list.
    List<(Vector2Int roadCell, Vector2Int sideCell)> OrderRunByRoadAdjacency(
        List<(Vector2Int roadCell, Vector2Int sideCell)> run)
    {
        int n = run.Count;
        if (n <= 1) return new List<(Vector2Int, Vector2Int)>(run);

        // Build adjacency across indices where roadCells are adjacent (manhattan distance == 1)
        var adj = new List<int>[n];
        for (int i = 0; i < n; i++) adj[i] = new List<int>();

        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                var a = run[i].roadCell;
                var b = run[j].roadCell;
                int manhattan = Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
                if (manhattan == 1)
                {
                    adj[i].Add(j);
                    adj[j].Add(i);
                }
            }
        }

        // find a start node (endpoint with degree 1) if possible
        int start = -1;
        for (int i = 0; i < n; i++)
        {
            if (adj[i].Count == 1)
            {
                start = i;
                break;
            }
        }
        if (start == -1) start = 0; // loop case or isolated -> arbitrary start

        // DFS to produce an ordered traversal that covers connected nodes
        var orderedIndices = new List<int>();
        var visited = new bool[n];
        var stack = new Stack<int>();
        stack.Push(start);

        while (stack.Count > 0)
        {
            int idx = stack.Pop();
            if (visited[idx]) continue;
            visited[idx] = true;
            orderedIndices.Add(idx);

            // push neighbors in order to try to keep locality (reverse so first neighbor is processed first)
            var neighbors = adj[idx];
            for (int k = neighbors.Count - 1; k >= 0; k--)
            {
                int nb = neighbors[k];
                if (!visited[nb]) stack.Push(nb);
            }
        }

        // If DFS didn't visit some nodes (branching), append remaining nodes
        for (int i = 0; i < n; i++)
        {
            if (!visited[i]) orderedIndices.Add(i);
        }

        var orderedRun = new List<(Vector2Int, Vector2Int)>();
        foreach (var idx in orderedIndices)
            orderedRun.Add(run[idx]);

        return orderedRun;
    }

    void CreateWire(Vector3 start, Vector3 end)
    {
        var wireObj = new GameObject("Wire");
        wireObj.transform.parent = transform;
        var lr = wireObj.AddComponent<LineRenderer>();
        lr.material = wireMaterial;
        lr.positionCount = Mathf.Max(3, wireSegments);
        lr.widthMultiplier = 0.05f;
        lr.useWorldSpace = true;

        for (int i = 0; i < lr.positionCount; i++)
        {
            float t = i / (float)(lr.positionCount - 1);
            Vector3 point = Vector3.Lerp(start, end, t);
            point.y -= Mathf.Sin(Mathf.PI * t) * sagAmount;
            lr.SetPosition(i, point);
        }
    }
}
