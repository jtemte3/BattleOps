using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CityGenOne : MonoBehaviour
{
    public int seed = 0;
    public int width = 100;
    public int height = 100;

    public int intersectionCount;
    public int intersectionMargin;
    public int intersectionMinDistance;
    public int intersectionMaxAttempts;
    public int edgePositionCounts;
    public int edgeMinDistance;
    public int edgeMaxAttempts;

    private Dictionary<Vector2, int> grid;
    private Texture2D tex;
    private List<Vector2Int> intersections = new();
    private List<Vector2Int> edgePositions = new();
    // Start is called before the first frame update
    void Start()
    {

        if (seed != 0)
        {
            Random.InitState(seed);
        }
        else
        {
            seed = Random.Range(int.MinValue, int.MaxValue);
            Random.InitState(seed);
        }

        // -1 == roadPosition
        //  0 == emptyPosition
        GenerateGrid();
        PickIntersections();
        PickEdgePositions();

        ConnectIntersections();
        ConnectEdgesToIntersections();

        FillTexture();
        GetComponent<Renderer>().material.mainTexture = tex;
    }

    void GenerateGrid()
    {
        grid = new Dictionary<Vector2, int>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                grid.Add(new Vector2(i, j), 0);
            }
        }
    }

    void PickIntersections()
    {
        for (int i =  0; i < intersectionCount; i++)
        {
            for (int a = 0; a < intersectionMaxAttempts; a++)
            {
                int x = Random.Range(0 + intersectionMargin, width - intersectionMargin);
                int y = Random.Range(0 + intersectionMargin, height - intersectionMargin);

                Vector2Int position = new Vector2Int(x, y);
                
                bool isValid = true;
                
                if (intersections.Count > 0)
                {
                    foreach (Vector2Int intersection in intersections)
                    {
                        if (Vector2Int.Distance(intersection, position) < intersectionMinDistance)
                        {
                            isValid = false;
                            break;
                        }
                    }
                }
                

                if (!isValid)
                {
                    break;
                }
                else
                {
                    intersections.Add(position);
                    grid[position] = -1;
                }
            }
        }
    }

    void PickEdgePositions()
    {
        List<Vector2Int> allEdgePositions = GetEdgePoints();

        for (int i = 0; i < edgePositionCounts; i++)
        {
            for (int a = 0;a < edgeMaxAttempts; a++)
            {
                Vector2Int edgePosition = allEdgePositions[Random.Range(0, allEdgePositions.Count)];

                bool isValid = true;

                if (edgePositions.Count > 0)
                {
                    foreach (Vector2Int edge in edgePositions)
                    {
                        float distance = Vector2Int.Distance(edge, edgePosition);

                        if (distance < edgeMinDistance)
                        {
                            isValid = false;
                        }
                    }
                }

                if (isValid)
                {
                    edgePositions.Add(edgePosition);
                    break;
                }                
            }
        }
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

    private void ConnectIntersections()
    {
        List<Vector2Int> connectedPositions = new();
        Vector2Int position = intersections[0];

        for (int i = 0; i < intersections.Count; i++) 
        {
            if (!connectedPositions.Contains(position))
            {
                connectedPositions.Add(position);

                Vector2Int closestPosition = new(int.MaxValue, int.MaxValue);
                List<Vector2Int> remainingPositions = new();

                foreach (Vector2Int existingPosition in intersections)
                {
                    if (!connectedPositions.Contains(existingPosition))
                    {
                        remainingPositions.Add(existingPosition);
                    }
                }

                if (remainingPositions.Count.Equals(0))
                {
                    closestPosition = intersections[0];
                }
                else
                {
                    foreach (Vector2Int otherPosition in remainingPositions)
                    {
                        float closest = Vector2Int.Distance(position, closestPosition);
                        float distance = Vector2Int.Distance(position, otherPosition);

                        if (distance < closest)
                        {
                            closestPosition = otherPosition;
                        }
                    }
                }

                List<Vector2Int> road = AStarPath(position, closestPosition);

                foreach (Vector2Int roadPosition in road)
                {
                    grid[roadPosition] = -1;
                }

                position = closestPosition;
            }
        }
    }

    private void ConnectEdgesToIntersections()
    {
        foreach (Vector2Int position in edgePositions)
        {
            Vector2Int closestPosition = new(int.MaxValue, int.MaxValue);

            foreach (Vector2Int intersection in intersections)
            {
                float closest = Vector2Int.Distance(position, closestPosition);
                float distance = Vector2Int.Distance(position, intersection);

                if (distance < closest)
                {
                    closestPosition = intersection;
                }
            }

            List<Vector2Int> road = AStarPath(position, closestPosition);

            foreach (Vector2Int roadPosition in road)
            {
                grid[roadPosition] = -1;
            }
        }
    }

    void FillTexture()
    {
        // Make texture
        tex = new Texture2D(width, height);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (grid[pos] == -1)
                {
                    tex.SetPixel(x, y, Color.black);
                }
                else
                {
                    tex.SetPixel(x, y, Color.white);
                }
                /*else if (grid[x, y] >= 0) tex.SetPixel(x, y, regionColors[grid[x, y]]);
                else tex.SetPixel(x, y, Color.magenta); // should never happen*/
            }
        }
        tex.Apply();
    }


    bool InBounds(Vector2Int p) => p.x >= 0 && p.x < width && p.y >= 0 && p.y < height;
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
                //if (grid[neighbor.x, neighbor.y] == -1 && neighbor != goal) continue;
                if (grid[neighbor] == -1 && neighbor != goal) continue;

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
