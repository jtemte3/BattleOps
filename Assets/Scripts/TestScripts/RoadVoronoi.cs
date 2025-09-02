using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RoadVoronoi : MonoBehaviour
{
    public string saveFolderName = "Road-First-Voronoi";
    public int width = 100;
    public int height = 100;
    public int intersectionCount = 10;
    [Range(0f, 1f)] public float branchChance = 0.2f; // probability a road cell will branch

    private int[,] grid;
    private Texture2D tex;
    private List<Vector2Int> intersections;

    void Start()
    {
        GenerateRoadVoronoi();
    }

    void GenerateRoadVoronoi()
    {
        grid = new int[width, height];
        tex = new Texture2D(width, height) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };

        intersections = new List<Vector2Int>();
        for (int i = 0; i < intersectionCount; i++)
            intersections.Add(new Vector2Int(Random.Range(1, width - 1), Random.Range(1, height - 1)));

        // Initialize road endpoints from edges
        Queue<Vector2Int> roadFrontier = new Queue<Vector2Int>();
        for (int i = 0; i < 4; i++)
        {
            var start = GetRandomEdgePoint();
            roadFrontier.Enqueue(start);
            grid[start.x, start.y] = -1;
        }

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        // Road growth loop
        while (roadFrontier.Count > 0 && intersections.Count > 0)
        {
            var current = roadFrontier.Dequeue();

            // Connect to nearest intersection
            var nearest = GetNearestIntersection(current);
            if (nearest != Vector2Int.zero) // if found
            {
                var path = AStarPath(current, nearest);
                foreach (var p in path)
                {
                    if (grid[p.x, p.y] != -1)
                    {
                        grid[p.x, p.y] = -1;
                        if (Random.value < branchChance) // possibly branch
                        {
                            foreach (var d in dirs)
                            {
                                var np = p + d;
                                if (InBounds(np) && grid[np.x, np.y] == 0)
                                    roadFrontier.Enqueue(np);
                            }
                        }
                    }
                }
                intersections.Remove(nearest);
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

    Vector2Int GetNearestIntersection(Vector2Int from)
    {
        float bestDist = float.MaxValue;
        Vector2Int best = Vector2Int.zero;
        foreach (var i in intersections)
        {
            float dist = Mathf.Abs(from.x - i.x) + Mathf.Abs(from.y - i.y);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = i;
            }
        }
        return best;
    }

    bool InBounds(Vector2Int p) => p.x >= 0 && p.x < width && p.y >= 0 && p.y < height;

    void FloodFill(Vector2Int start, int regionId)
    {
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(start);
        grid[start.x, start.y] = regionId;
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

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
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (open.Count > 0)
        {
            var current = open.Dequeue();
            if (current == goal) return ReconstructPath(cameFrom, current);

            foreach (var d in dirs)
            {
                var neighbor = current + d;
                if (!InBounds(neighbor)) continue;
                if (grid[neighbor.x, neighbor.y] == -1 && neighbor != goal) continue; // skip existing road unless goal

                float tentativeG = gScore[current] + 1;
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

    float Heuristic(Vector2Int a, Vector2Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

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

    private void SaveTexture(Texture2D texture)
    {
        byte[] bytes = texture.EncodeToPNG();
        string path = Path.Combine(Application.streamingAssetsPath, saveFolderName);
        //var dirPath = Application.dataPath + "/RenderOutput";
        if (!System.IO.Directory.Exists(path))
        {
            System.IO.Directory.CreateDirectory(path);
        }
        //System.IO.File.WriteAllBytes(path + "/R_" + Random.Range(0, 100000) + ".png", bytes);
        path += "/" + System.Guid.NewGuid().ToString() + ".png";
        System.IO.File.WriteAllBytes(path, bytes);
        Debug.Log(bytes.Length / 1024 + "Kb was saved as: " + path);
    }
}
