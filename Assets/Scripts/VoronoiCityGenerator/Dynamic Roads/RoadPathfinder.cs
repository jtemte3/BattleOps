using System;
using System.Collections.Generic;
using UnityEngine;

public static class RoadPathfinder
{
    // 8 possible move directions
    private static readonly Vector2Int[] dirs = new Vector2Int[]
    {
        new Vector2Int(1, 0), new Vector2Int(-1, 0),
        new Vector2Int(0, 1), new Vector2Int(0, -1),
        new Vector2Int(1, 1), new Vector2Int(-1, 1),
        new Vector2Int(1, -1), new Vector2Int(-1, -1)
    };

    public static List<Vector2Int> FindStraightestPath(
        Vector2Int start,
        Vector2Int goal,
        List<Vector2Int> roadPositions,
        List<Vector2Int> intersections)
    {
        // priority queue for A* (min-heap on fScore)
        var open = new SimplePriorityQueue<Vector2Int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, float>();
        var fScore = new Dictionary<Vector2Int, float>();

        // keep track of direction taken to reach a node
        var cameDir = new Dictionary<Vector2Int, Vector2Int>();

        gScore[start] = 0;
        fScore[start] = Heuristic(start, goal);
        open.Enqueue(start, fScore[start]);

        while (open.Count > 0)
        {
            var current = open.Dequeue();

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            foreach (var dir in dirs)
            {
                var next = current + dir;

                if (!roadPositions.Contains(next) || intersections.Contains(next))
                {
                    continue;
                }
                    

                float tentativeG = gScore[current] + 1;

                // penalty for turning: compare to previous direction
                if (cameDir.TryGetValue(current, out var prevDir) && prevDir != Vector2Int.zero)
                {
                    if (dir != prevDir)
                        tentativeG += 0.25f; // tweak penalty
                }

                if (!gScore.ContainsKey(next) || tentativeG < gScore[next])
                {
                    cameFrom[next] = current;
                    cameDir[next] = dir;
                    gScore[next] = tentativeG;
                    fScore[next] = tentativeG + Heuristic(next, goal);

                    if (open.Contains(next))
                        open.UpdatePriority(next, fScore[next]);
                    else
                        open.Enqueue(next, fScore[next]);
                }
            }
        }

        // no path found
        //Debug.LogWarning("Path not found: "+ start + " -> "+  goal);
        return new List<Vector2Int>();
    }

    private static float Heuristic(Vector2Int a, Vector2Int b)
    {
        // diagonal distance heuristic
        return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
    }

    private static List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
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
}

// --- SimplePriorityQueue ---
// Minimal priority queue implementation for A*
public class SimplePriorityQueue<T>
{
    private readonly SortedDictionary<float, Queue<T>> dict = new SortedDictionary<float, Queue<T>>();
    private readonly HashSet<T> set = new HashSet<T>();

    public int Count { get; private set; }

    public void Enqueue(T item, float priority)
    {
        if (!dict.TryGetValue(priority, out var q))
        {
            q = new Queue<T>();
            dict[priority] = q;
        }
        q.Enqueue(item);
        set.Add(item);
        Count++;
    }

    public T Dequeue()
    {
        while (dict.Count > 0)
        {
            // always take the smallest priority bucket
            var first = dict.Keys.GetEnumerator();
            first.MoveNext();
            var key = first.Current;

            var q = dict[key];
            if (q.Count > 0)
            {
                var item = q.Dequeue();
                if (q.Count == 0) dict.Remove(key);

                set.Remove(item);
                Count--;
                return item;
            }
            else
            {
                // remove empty bucket and continue
                dict.Remove(key);
            }
        }

        throw new InvalidOperationException("Queue empty.");
    }

    public void UpdatePriority(T item, float newPriority)
    {
        // naive: remove + re-add
        Remove(item);
        Enqueue(item, newPriority);
    }

    public bool Contains(T item) => set.Contains(item);

    private void Remove(T item)
    {
        // brute-force removal, can optimize
        foreach (var kvp in dict)
        {
            if (kvp.Value.Contains(item))
            {
                var newQ = new Queue<T>();
                foreach (var i in kvp.Value)
                {
                    if (!EqualityComparer<T>.Default.Equals(i, item))
                        newQ.Enqueue(i);
                }
                dict[kvp.Key] = newQ;
                set.Remove(item);
                Count--;
                return;
            }
        }
    }
}
