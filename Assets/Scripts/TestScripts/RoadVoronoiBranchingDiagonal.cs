using System.Collections.Generic;
using UnityEngine;

public class RoadVoronoiBranchingDiagonal : MonoBehaviour
{
    public int width = 100;
    public int height = 100;
    public int intersectionCount = 10;

    private int[,] grid;
    private Texture2D tex;
    private List<Vector2Int> intersections;
    private List<Vector2Int> startEdges;

    void Start()
    {
        GenerateRoadVoronoi();
    }

    void GenerateRoadVoronoi()
    {
        grid = new int[width, height];
        tex = new Texture2D(width, height) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };

        // Generate intersections
        intersections = new List<Vector2Int>();
        for (int i = 0; i < intersectionCount; i++)
            intersections.Add(new Vector2Int(Random.Range(1, width - 1), Random.Range(1, height - 1)));

        // Generate starting edge points
        startEdges = new List<Vector2Int>();
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < intersectionCount / 2; j++)
                startEdges.Add(GetRandomEdgePoint());
        }

        // Connect every start edge to every intersection
        foreach (var start in startEdges)
        {
            foreach (var target in intersections)
            {
                var path = AStarPath(start, target);
                foreach (var p in path)
                    grid[p.x, p.y] = -1; // road
            }
        }

        // Flood-fill regions
        int regionId = 1;
        Dictionary<int, Color> regionColors = new Dictionary<int, Color>();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] == 0)
                {
                    Color c = new Color(Random.value, Random.value, Random.value);
                    regionColors[regionId] = c;
                    FloodFill(new Vector2Int(x, y), regionId);
                    regionId++;
                }
            }
        }

        // Draw
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] == -1)
                    tex.SetPixel(x, y, Color.black);
                else
                    tex.SetPixel(x, y, regionColors[grid[x, y]]);
            }
        }
        tex.Apply();
        GetComponent<Renderer>().material.mainTexture = tex;
    }

    Vector2Int GetRandomEdgePoint()
    {
        int side = Random.Range(0, 4);
        return side switch
        {
            0 => new Vector2Int(Random.Range(0, width), 0),
            1 => new Vector2Int(Random.Range(0, width), height - 1),
            2 => new Vector2Int(0, Random.Range(0, height)),
            _ => new Vector2Int(width - 1, Random.Range(0, height))
        };
    }

    bool InBounds(Vector2Int p) => p.x >= 0 && p.x < width && p.y >= 0 && p.y < height;

    void FloodFill(Vector2Int start, int regionId)
    {
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(start);
        grid[start.x, start.y] = regionId;

        Vector2Int[] dirs = {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
            new Vector2Int(1,1), new Vector2Int(1,-1), new Vector2Int(-1,1), new Vector2Int(-1,-1)
        };

        while (q.Count > 0)
        {
            var p = q.Dequeue();
            foreach (var d in dirs)
            {
                var np = p + d;
                if (InBounds(np) && grid[np.x, np.y] == 0)
                {
                    grid[np.x, np.y] = regionId;
                    q.Enqueue(np);
                }
            }
        }
    }

    List<Vector2Int> AStarPath(Vector2Int start, Vector2Int goal)
    {
        var open = new PriorityQueue<Vector2Int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, float> { [start] = 0 };
        var fScore = new Dictionary<Vector2Int, float> { [start] = Heuristic(start, goal) };

        open.Enqueue(start, fScore[start]);

        Vector2Int[] dirs = {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
            new Vector2Int(1,1), new Vector2Int(1,-1), new Vector2Int(-1,1), new Vector2Int(-1,-1)
        };

        while (open.Count > 0)
        {
            var current = open.Dequeue();
            if (current == goal) return ReconstructPath(cameFrom, current);

            foreach (var d in dirs)
            {
                var neighbor = current + d;
                if (!InBounds(neighbor)) continue;
                if (grid[neighbor.x, neighbor.y] == -1 && neighbor != goal) continue;

                float moveCost = (d.x != 0 && d.y != 0) ? 1.4142f : 1f; // diagonal cost
                float tentativeG = gScore[current] + moveCost;
                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, goal);
                    if (!open.Contains(neighbor))
                        open.Enqueue(neighbor, fScore[neighbor]);
                }
            }
        }
        return new List<Vector2Int>();
    }

    float Heuristic(Vector2Int a, Vector2Int b)
    {
        // Euclidean distance for diagonal movement
        return Mathf.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y));
    }

    List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        var path = new List<Vector2Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }
        path.Reverse();
        return path;
    }

    class PriorityQueue<T>
    {
        private List<(T item, float priority)> elements = new();
        public int Count => elements.Count;
        public void Enqueue(T item, float priority) => elements.Add((item, priority));
        public T Dequeue()
        {
            int bestIndex = 0;
            for (int i = 1; i < elements.Count; i++)
                if (elements[i].priority < elements[bestIndex].priority) bestIndex = i;
            var best = elements[bestIndex].item;
            elements.RemoveAt(bestIndex);
            return best;
        }
        public bool Contains(T item) => elements.Exists(e => EqualityComparer<T>.Default.Equals(e.item, item));
    }
}
