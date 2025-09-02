using System.Collections.Generic;
using UnityEngine;

public class VoronoiCityTexture : MonoBehaviour
{
    [Header("Grid")]
    public int width = 256;
    public int height = 256;

    [Header("Seeds")]
    public int seedCount = 10;
    public int randomSeed = 0;

    [Header("Roads")]
    public int roadThickness = 1;
    public bool allowDiagonal = true;

    [Header("Output")]
    public Renderer outputRenderer; // Assign a Quad's renderer to display

    int[,] owner;
    bool[,] isRoad;
    int[,] regionId;

    Vector2Int[] dirs4 = new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };
    Vector2Int[] dirs8;

    void Start()
    {
        dirs8 = new Vector2Int[]
        {
            new Vector2Int(1,0), new Vector2Int(-1,0), new Vector2Int(0,1), new Vector2Int(0,-1),
            new Vector2Int(1,1), new Vector2Int(1,-1), new Vector2Int(-1,1), new Vector2Int(-1,-1)
        };

        GenerateTexture();
    }

    void GenerateTexture()
    {
        owner = new int[width, height];
        isRoad = new bool[width, height];
        regionId = new int[width, height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                owner[x, y] = -1;
                isRoad[x, y] = false;
                regionId[x, y] = -1;
            }

        // Place seeds
        System.Random rng = new System.Random(randomSeed);
        List<Vector2Int> seeds = new List<Vector2Int>();
        for (int i = 0; i < seedCount; i++)
        {
            int sx, sy;
            do
            {
                sx = rng.Next(width);
                sy = rng.Next(height);
            } while (owner[sx, sy] != -1);
            owner[sx, sy] = i;
            seeds.Add(new Vector2Int(sx, sy));
        }

        // BFS wavefront
        Queue<Vector2Int> q = new Queue<Vector2Int>(seeds);
        var dist = new int[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                dist[x, y] = int.MaxValue;

        foreach (var s in seeds) dist[s.x, s.y] = 0;

        while (q.Count > 0)
        {
            var v = q.Dequeue();
            int cx = v.x, cy = v.y;
            int curOwner = owner[cx, cy];
            var dirs = allowDiagonal ? dirs8 : dirs4;

            foreach (var d in dirs)
            {
                int nx = cx + d.x;
                int ny = cy + d.y;
                if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;

                if (owner[nx, ny] == -1)
                {
                    owner[nx, ny] = curOwner;
                    dist[nx, ny] = dist[cx, cy] + 1;
                    q.Enqueue(new Vector2Int(nx, ny));
                }
                else if (owner[nx, ny] != curOwner && owner[nx, ny] != -2)
                {
                    isRoad[cx, cy] = true;
                    isRoad[nx, ny] = true;
                    owner[cx, cy] = -2;
                    owner[nx, ny] = -2;
                }
            }
        }

        // Road dilation
        if (roadThickness > 0)
        {
            bool[,] newRoad = (bool[,])isRoad.Clone();
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (isRoad[x, y])
                        for (int dx = -roadThickness; dx <= roadThickness; dx++)
                            for (int dy = -roadThickness; dy <= roadThickness; dy++)
                            {
                                int nx = x + dx, ny = y + dy;
                                if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                                if (dx * dx + dy * dy <= roadThickness * roadThickness)
                                {
                                    newRoad[nx, ny] = true;
                                    owner[nx, ny] = -2;
                                }
                            }
            isRoad = newRoad;
        }

        // Flood fill regions
        int nextRegionId = 0;
        var dirsFill = allowDiagonal ? dirs8 : dirs4;
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (isRoad[x, y] || regionId[x, y] != -1) continue;
                Stack<Vector2Int> stack = new Stack<Vector2Int>();
                stack.Push(new Vector2Int(x, y));
                regionId[x, y] = nextRegionId;

                while (stack.Count > 0)
                {
                    var p = stack.Pop();
                    foreach (var d in dirsFill)
                    {
                        int nx = p.x + d.x, ny = p.y + d.y;
                        if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                        if (isRoad[nx, ny] || regionId[nx, ny] != -1) continue;
                        regionId[nx, ny] = nextRegionId;
                        stack.Push(new Vector2Int(nx, ny));
                    }
                }
                nextRegionId++;
            }

        // Create color palette
        Color[] palette = new Color[nextRegionId];
        for (int i = 0; i < nextRegionId; i++)
            palette[i] = Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.8f, 1f);

        // Build texture
        Texture2D tex = new Texture2D(width, height);
        tex.filterMode = FilterMode.Point;
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (isRoad[x, y]) tex.SetPixel(x, y, Color.black);
                else tex.SetPixel(x, y, palette[regionId[x, y]]);
            }
        tex.Apply();

        // Assign to renderer
        if (outputRenderer != null)
            outputRenderer.material.mainTexture = tex;
    }
}
