using System.Collections.Generic;
using UnityEngine;

public class VoronoiRoadRegions : MonoBehaviour
{
    public int width = 100;
    public int height = 100;
    public int intersectionCount = 10;
    private Texture2D tex;
    //public Texture2D outputTexture;

    private Color[,] grid;
    private List<Vector2Int> intersections;

    void Start()
    {
        grid = new Color[width, height];
        tex = new Texture2D(width, height) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
        intersections = new List<Vector2Int>();

        // Fill background white
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                grid[x, y] = Color.white;

        // Generate intersections
        for (int i = 0; i < intersectionCount; i++)
        {
            Vector2Int pos = new Vector2Int(Random.Range(1, width - 1), Random.Range(1, height - 1));
            intersections.Add(pos);
        }

        // All starting edge points
        List<Vector2Int> edgePoints = GetEdgePoints();

        // Path each edge point to all intersections
        foreach (var start in edgePoints)
        {
            foreach (var end in intersections)
            {
                List<Vector2Int> path = AStar(start, end, true); // true = allow diagonal
                foreach (var p in path)
                    grid[p.x, p.y] = Color.black;
            }
        }

        // Color regions
        ColorRegions();

        // Output texture
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                tex.SetPixel(x, y, grid[x, y]);
        tex.Apply();

        GetComponent<Renderer>().material.mainTexture = tex;
    }

    List<Vector2Int> GetEdgePoints()
    {
        var points = new List<Vector2Int>();

        // Top and bottom
        for (int x = 0; x < width; x++)
        {
            points.Add(new Vector2Int(x, 0));
            points.Add(new Vector2Int(x, height - 1));
        }

        // Left and right
        for (int y = 0; y < height; y++)
        {
            points.Add(new Vector2Int(0, y));
            points.Add(new Vector2Int(width - 1, y));
        }

        return points;
    }

    List<Vector2Int> AStar(Vector2Int start, Vector2Int goal, bool allowDiagonal)
    {
        var openSet = new List<Vector2Int> { start };
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, float> { [start] = 0 };
        var fScore = new Dictionary<Vector2Int, float> { [start] = Heuristic(start, goal) };

        Vector2Int[] directions = allowDiagonal
            ? new Vector2Int[] {
                new Vector2Int(1,0), new Vector2Int(-1,0),
                new Vector2Int(0,1), new Vector2Int(0,-1),
                new Vector2Int(1,1), new Vector2Int(1,-1),
                new Vector2Int(-1,1), new Vector2Int(-1,-1)
            }
            : new Vector2Int[] {
                new Vector2Int(1,0), new Vector2Int(-1,0),
                new Vector2Int(0,1), new Vector2Int(0,-1)
            };

        while (openSet.Count > 0)
        {
            // Find node with lowest fScore
            Vector2Int current = openSet[0];
            foreach (var n in openSet)
                if (fScore.ContainsKey(n) && fScore[n] < fScore[current])
                    current = n;

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            openSet.Remove(current);

            foreach (var dir in directions)
            {
                Vector2Int neighbor = current + dir;
                if (neighbor.x < 0 || neighbor.x >= width || neighbor.y < 0 || neighbor.y >= height)
                    continue;

                float tentative_gScore = gScore[current] + Vector2Int.Distance(current, neighbor);

                if (!gScore.ContainsKey(neighbor) || tentative_gScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentative_gScore;
                    fScore[neighbor] = tentative_gScore + Heuristic(neighbor, goal);
                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return new List<Vector2Int>();
    }

    float Heuristic(Vector2Int a, Vector2Int b) =>
        Vector2Int.Distance(a, b);

    List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        var totalPath = new List<Vector2Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Add(current);
        }
        return totalPath;
    }

    void ColorRegions()
    {
        bool[,] visited = new bool[width, height];
        Vector2Int[] dirs = {
            new Vector2Int(1,0), new Vector2Int(-1,0),
            new Vector2Int(0,1), new Vector2Int(0,-1)
        };

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!visited[x, y] && grid[x, y] != Color.black)
                {
                    Color regionColor = new Color(Random.value, Random.value, Random.value);
                    Queue<Vector2Int> q = new Queue<Vector2Int>();
                    q.Enqueue(new Vector2Int(x, y));
                    visited[x, y] = true;

                    while (q.Count > 0)
                    {
                        Vector2Int p = q.Dequeue();
                        grid[p.x, p.y] = regionColor;

                        foreach (var d in dirs)
                        {
                            Vector2Int np = p + d;
                            if (np.x >= 0 && np.x < width && np.y >= 0 && np.y < height)
                            {
                                if (!visited[np.x, np.y] && grid[np.x, np.y] != Color.black)
                                {
                                    visited[np.x, np.y] = true;
                                    q.Enqueue(np);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
