using System;
using System.Collections.Generic;
using UnityEngine;

public class RoadGrid : MonoBehaviour
{
    public int width = 100;
    public int height = 100;
    public int numIntersections = 10;
    public int numEdgePoints = 5;
    //public Texture2D texture;
    private Texture2D texture;

    private Color[,] grid;
    private Vector2Int[] intersections;
    private System.Random rand = new System.Random();

    void Start()
    {
        grid = new Color[width, height];
        texture = new Texture2D(width, height);

        intersections = new Vector2Int[numIntersections];
        for (int i = 0; i < numIntersections; i++)
            intersections[i] = new Vector2Int(rand.Next(width), rand.Next(height));

        // Connect intersections with nearest neighbor
        ConnectIntersections();

        // Add edge connections
        for (int i = 0; i < numEdgePoints; i++)
        {
            Vector2Int edgePoint = GetRandomEdgePoint();
            GrowRoad(edgePoint, intersections[rand.Next(intersections.Length)]);
        }

        // Flood fill regions
        ColorRegions();

        DrawTexture();

        GetComponent<Renderer>().material.mainTexture = texture;
    }

    void ConnectIntersections()
    {
        for (int i = 0; i < intersections.Length; i++)
        {
            Vector2Int nearest = intersections[0];
            float nearestDist = float.MaxValue;
            for (int j = 0; j < intersections.Length; j++)
            {
                if (i == j) continue;
                float dist = Vector2Int.Distance(intersections[i], intersections[j]);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = intersections[j];
                }
            }
            GrowRoad(intersections[i], nearest);
        }
    }

    void GrowRoad(Vector2Int start, Vector2Int end)
    {
        Vector2Int pos = start;
        while (pos != end)
        {
            grid[pos.x, pos.y] = Color.black;
            int dx = Math.Sign(end.x - pos.x);
            int dy = Math.Sign(end.y - pos.y);
            // Add diagonal and randomness
            if (rand.NextDouble() < 0.3) dx = 0;
            if (rand.NextDouble() < 0.3) dy = 0;
            pos = new Vector2Int(pos.x + dx, pos.y + dy);
        }
    }

    Vector2Int GetRandomEdgePoint()
    {
        int side = rand.Next(4);
        if (side == 0) return new Vector2Int(rand.Next(width), 0);
        if (side == 1) return new Vector2Int(rand.Next(width), height - 1);
        if (side == 2) return new Vector2Int(0, rand.Next(height));
        return new Vector2Int(width - 1, rand.Next(height));
    }

    void ColorRegions()
    {
        bool[,] visited = new bool[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!visited[x, y] && grid[x, y] != Color.black)
                {
                    Color regionColor = new Color((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble());
                    FloodFill(x, y, regionColor, visited);
                }
            }
        }
    }

    void FloodFill(int startX, int startY, Color col, bool[,] visited)
    {
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(new Vector2Int(startX, startY));

        while (q.Count > 0)
        {
            Vector2Int p = q.Dequeue();
            if (p.x < 0 || p.y < 0 || p.x >= width || p.y >= height) continue;
            if (visited[p.x, p.y] || grid[p.x, p.y] == Color.black) continue;

            visited[p.x, p.y] = true;
            grid[p.x, p.y] = col;

            q.Enqueue(new Vector2Int(p.x + 1, p.y));
            q.Enqueue(new Vector2Int(p.x - 1, p.y));
            q.Enqueue(new Vector2Int(p.x, p.y + 1));
            q.Enqueue(new Vector2Int(p.x, p.y - 1));
        }
    }

    void DrawTexture()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                texture.SetPixel(x, y, grid[x, y] == default ? Color.white : grid[x, y]);
        texture.Apply();
    }
}
