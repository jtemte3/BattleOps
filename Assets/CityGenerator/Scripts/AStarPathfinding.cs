using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AStarPathfinding
{
    // Directions to move in a 4-way grid (up, down, left, right)
    private readonly Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end, List<Vector2Int> blockedPositions, List<Vector2Int> roadPositions, int gridWidth, int gridHeight)
    {
        if (blockedPositions.Contains(start))
        {
            return null;
        }
        // Open set for nodes to be evaluated, with start node added
        List<Node> openSet = new List<Node> { new Node(start, null, 0, GetHeuristic(start, end)) };
        // Closed set to track visited nodes
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();

        while (openSet.Count > 0)
        {
            // Find the node with the lowest F cost
            Node current = GetLowestFCostNode(openSet);

            // If reached the target, reconstruct the path
            if (current.Position == end)
            {
                return ReconstructPath(current);
            }

            openSet.Remove(current);
            //closedSet.Add(current.Position);

            if (!blockedPositions.Contains(current.Position))
            {
                closedSet.Add(current.Position);
            }
            else
            {
                // No path found
                return null;
            }

            // Explore neighbors
            foreach (Vector2Int direction in directions)
            {
                Vector2Int neighborPos = current.Position + direction;

                // Skip out-of-bounds, blocked, or already-closed positions
                if (!IsWithinBounds(neighborPos, gridWidth, gridHeight) ||
                    blockedPositions.Contains(neighborPos) ||
                    closedSet.Contains(neighborPos))
                {
                    continue;
                }

                float tentativeGCost = current.G + 1;

                /*if (roadPositions.Contains(neighborPos))
                {
                    tentativeGCost -= 1;
                }*/

                Node neighborNode = openSet.Find(node => node.Position == neighborPos);
                if (neighborNode == null)
                {
                    // New node, add it to open set
                    openSet.Add(new Node(neighborPos, current, tentativeGCost, GetHeuristic(neighborPos, end)));
                }
                else if (tentativeGCost < neighborNode.G)
                {
                    // Found a better path to this node
                    neighborNode.Parent = current;
                    neighborNode.G = tentativeGCost;
                    neighborNode.F = neighborNode.G + neighborNode.H;
                }
            }
        }

        // No path found
        return null;
    }

    // Node class for A* pathfinding
    private class Node
    {
        public Vector2Int Position;
        public Node Parent;
        public float G; // Cost from start to this node
        public float H; // Heuristic cost estimate to end
        public float F; // Total cost (G + H)

        public Node(Vector2Int position, Node parent, float g, float h)
        {
            Position = position;
            Parent = parent;
            G = g;
            H = h;
            F = G + H;
        }
    }

    // Heuristic function (Manhattan distance)
    private float GetHeuristic(Vector2Int a, Vector2Int b)
    {
        return Vector2Int.Distance(a, b);
        //return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    // Reconstructs the path from the end node to the start
    private List<Vector2Int> ReconstructPath(Node endNode)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Node current = endNode;
        while (current != null)
        {
            path.Add(current.Position);
            current = current.Parent;
        }
        path.Reverse();
        return path;
    }

    // Returns the node with the lowest F cost in the open set
    private Node GetLowestFCostNode(List<Node> openSet)
    {
        Node lowestFCostNode = openSet[0];
        foreach (Node node in openSet)
        {
            if (node.F < lowestFCostNode.F)
            {
                lowestFCostNode = node;
            }
        }
        return lowestFCostNode;
    }

    // Check if a position is within the grid bounds
    private bool IsWithinBounds(Vector2Int position, int width, int height)
    {
        return position.x >= 0 && position.x < width && position.y >= 0 && position.y < height;
    }
}
