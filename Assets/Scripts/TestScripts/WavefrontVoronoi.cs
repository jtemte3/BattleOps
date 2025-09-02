using UnityEngine;

public class WavefrontVoronoi : MonoBehaviour
{
    public int width = 100;
    public int height = 100;
    public int intersectionCount = 10;
    public Color roadColor = Color.black;

    private Color[] regionColors;
    private int[,] grid; // -1 = unassigned, -2 = road, >=0 = region id

    void Start()
    {
        Generate();
    }

    void Generate()
    {
        grid = new int[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                grid[x, y] = -1;

        // Pick intersections
        Vector2Int[] intersections = new Vector2Int[intersectionCount];
        for (int i = 0; i < intersectionCount; i++)
            intersections[i] = new Vector2Int(Random.Range(0, width), Random.Range(0, height));

        // Assign random colors to regions
        regionColors = new Color[intersectionCount];
        for (int i = 0; i < intersectionCount; i++)
            regionColors[i] = Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.8f, 1f);

        // Wavefront growth
        bool changed = true;
        int[,] newGrid = (int[,])grid.Clone();

        // Seed initial points
        for (int i = 0; i < intersectionCount; i++)
            grid[intersections[i].x, intersections[i].y] = i;

        // Directions (8-way)
        Vector2Int[] dirs = {
            new Vector2Int(1,0), new Vector2Int(-1,0), new Vector2Int(0,1), new Vector2Int(0,-1),
            new Vector2Int(1,1), new Vector2Int(1,-1), new Vector2Int(-1,1), new Vector2Int(-1,-1)
        };

        while (changed)
        {
            changed = false;
            newGrid = (int[,])grid.Clone();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (grid[x, y] >= 0) // is region cell
                    {
                        foreach (var d in dirs)
                        {
                            int nx = x + d.x;
                            int ny = y + d.y;
                            if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;

                            if (grid[nx, ny] == -1)
                            {
                                newGrid[nx, ny] = grid[x, y];
                                changed = true;
                            }
                            else if (grid[nx, ny] >= 0 && grid[nx, ny] != grid[x, y])
                            {
                                newGrid[nx, ny] = -2; // road
                            }
                        }
                    }
                }
            }

            grid = newGrid;
        }

        // Make texture
        Texture2D tex = new Texture2D(width, height);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == -2) tex.SetPixel(x, y, roadColor);
                else if (grid[x, y] >= 0) tex.SetPixel(x, y, regionColors[grid[x, y]]);
                else tex.SetPixel(x, y, Color.magenta); // should never happen
            }
        }
        tex.Apply();

        GetComponent<Renderer>().material.mainTexture = tex;
    }
}
