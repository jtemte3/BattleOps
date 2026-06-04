using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VillageRoadGenerator : MonoBehaviour
{
    [Header("Settings")]
    public int intersectionCount = 5;
    public int maxNeighbors = 3;
    public float minDistance = 10f;
    public float cellSize = 3f;
    public int width = 50;
    public int height = 50;
    public int waveFrequency = 4;

    [Header("Inputs")]
    public List<Vector2Int> gridCells = new List<Vector2Int>();
    List<RoadNode> allNodes = new List<RoadNode>();
    public List<Transform> externalRoadNodes = new List<Transform>();

    List<RoadNode> villageNodes = new List<RoadNode>();
    HashSet<RoadEdge> edges = new HashSet<RoadEdge>();
    struct RoadNode
    {
        public Vector3 position;
        public bool isExternal;

        public RoadNode(Vector3 pos, bool external)
        {
            position = pos;
            isExternal = external;
        }
    }

    struct RoadEdge
    {
        public Vector3 a;
        public Vector3 b;

        public RoadEdge(Vector3 a, Vector3 b)
        {
            this.a = a;
            this.b = b;
        }

        public override int GetHashCode()
        {
            return a.GetHashCode() ^ b.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RoadEdge)) return false;
            RoadEdge e = (RoadEdge)obj;

            // undirected comparison
            return (a == e.a && b == e.b) || (a == e.b && b == e.a);
        }
    }

    public void Start()
    {
        gridCells = CreateWavyCity();

        GenerateVillage();
    }

    public void GenerateVillage()
    {
        if (gridCells.Count < intersectionCount)
        {
            Debug.LogError("Not enough grid cells.");
            return;
        }

        // -------------------------
        // 1. Pick random grid intersections
        // -------------------------

        List<Vector2Int> shuffled = gridCells
            .OrderBy(x => Random.value)
            .ToList();

        List<Vector2Int> chosen = new List<Vector2Int>();

        foreach (var cell in shuffled)
        {
            if (chosen.Count >= intersectionCount)
                break;

            Vector3 candidateWorld = new Vector3(cell.x * cellSize, transform.position.y, cell.y * cellSize);

            bool valid = true;

            foreach (var existing in chosen)
            {
                Vector3 existingWorld = new Vector3(existing.x * cellSize, transform.position.y, existing.y * cellSize);

                if (Vector3.Distance(candidateWorld, existingWorld) < minDistance)
                {
                    valid = false;
                    break;
                }
            }

            if (valid)
                chosen.Add(cell);
        }

        // Fallback warning (important for small grids)
        if (chosen.Count < intersectionCount)
        {
            Debug.LogWarning("Could not satisfy minimum distance for all intersections. Consider increasing grid size or reducing minDistance.");
        }


        foreach (var c in chosen)
        {
            Vector3 world = new Vector3(c.x * cellSize, transform.position.y, c.y * cellSize);
            villageNodes.Add(new RoadNode(world, false));
        }

        // -------------------------
        // 2. Add external nodes
        // -------------------------

        allNodes = new List<RoadNode>(villageNodes);
        if (externalRoadNodes.Count > 0)
        {
            foreach (var t in externalRoadNodes)
            {
                allNodes.Add(new RoadNode(t.position, true));
            }
        }

        // -------------------------
        // 3. Build connections
        // -------------------------

        

        foreach (var node in allNodes)
        {
            var neighbors = allNodes
                .Where(n => n.position != node.position)
                .OrderBy(n => Vector3.Distance(node.position, n.position))
                .Take(maxNeighbors)
                .ToList();

            // Always connect to closest
            edges.Add(new RoadEdge(node.position, neighbors[0].position));

            int neighborCount = Random.Range(0, maxNeighbors);
            // Optionally connect to next closest
            for (int i = 1; i < neighborCount; i++)
            {
                edges.Add(new RoadEdge(node.position, neighbors[i].position));
            }
        }
    }

    public List<Vector2Int> CreateRectangularGrid()
    {
        List<Vector2Int> newGrid = new List<Vector2Int>();

        for (int i = -(height / 2); i <= (height / 2); i++)
        {
            for (int j = -(width / 2); j <= (width / 2); j++)
            {
                newGrid.Add(new Vector2Int(i,j));
            }
        }

        return newGrid;
    }

    public List<Vector2Int> CreateWavyCity()
    {
        List<Vector2Int> newGrid = new List<Vector2Int>();

        float baseRadius = Mathf.Min(width, height) / 2f;
        float sinAmplitude = baseRadius * 0.1f;   // how far the waves push in/out
        float cosAmplitude = baseRadius * 0.1f;   // how far the waves push in/out
        float sinFrequency = waveFrequency;                  // number of waves around the circle
        float cosFrequency = waveFrequency;                  // number of waves around the circle

        for (int i = -height / 2; i <= height / 2; i++)
        {
            for (int j = -width / 2; j <= width / 2; j++)
            {
                Vector2 pos = new Vector2(j, i);
                float angle = Mathf.Atan2(i, j);             // angle around center
                float dist = pos.magnitude;

                // boundary radius at this angle
                //float wavyRadius = baseRadius + Mathf.Sin(angle * frequency) * amplitude;
                float wavyRadius = baseRadius
                    + Mathf.Sin(angle * sinFrequency) * sinAmplitude
                    + Mathf.Cos(angle * cosFrequency) * cosAmplitude;

                if (dist <= wavyRadius)
                {
                    newGrid.Add(new Vector2Int(j, i));
                }
            }
        }

        return newGrid;
    }

    public void OnDrawGizmos()
    {
        // -------------------------
        // 4. Debug visualize
        // -------------------------
        if (gridCells.Count > 0)
        {
            foreach (var gridcell in gridCells)
            {
                Vector3 pos = new Vector3(gridcell.x, transform.position.y, gridcell.y);
                Debug.DrawRay(pos, Vector3.up * 2f, Color.white, 20f);
            }
        }

        if (edges.Count > 0)
        {
            foreach (var e in edges)
            {
                Debug.DrawLine(e.a + Vector3.up, e.b + Vector3.up, Color.yellow, 20f);
            }
        }

        if (allNodes.Count > 0)
        {
            foreach (var n in allNodes)
            {
                Debug.DrawRay(n.position, Vector3.up * 2f, Color.red, 20f);
            }
        }

        if (externalRoadNodes.Count > 0)
        {
            foreach (var t in externalRoadNodes)
            {
                Debug.DrawRay(t.position, Vector3.up * 2f, Color.green, 20f);
            }
        }

        Debug.DrawRay(transform.position, Vector3.up * 2f, Color.orange, 20f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, new Vector3(width,1,height));
    }
}
