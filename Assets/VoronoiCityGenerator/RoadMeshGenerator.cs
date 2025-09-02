using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadMeshGenerator : MonoBehaviour
{
    public List<Vector2Int> roadPositions;   // fill this from your generator
    public float roadWidth = 1f;
    public float gridscale;
    public Material roadMaterial;

    class RoadNode
    {
        public Vector2Int pos;
        public List<RoadNode> neighbors = new();
        public RoadNode(Vector2Int p) { pos = p; }
    }

    Dictionary<Vector2Int, RoadNode> nodes;

    public void Generate()
    {
        BuildGraph();
        var segments = ExtractSegments();
        foreach (var seg in segments)
        {
            //BuildMesh(seg);
            BuildSmoothMesh(seg);
        }
    }

    void BuildGraph()
    {
        nodes = roadPositions.ToDictionary(p => p, p => new RoadNode(p));
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right, new Vector2Int(1,1), new Vector2Int(-1, -1), new Vector2Int(-1, 1), new Vector2Int(1, -1)};
        foreach (var n in nodes.Values)
        {
            foreach (var d in dirs)
            {
                var nPos = n.pos + d;
                if (nodes.ContainsKey(nPos))
                    n.neighbors.Add(nodes[nPos]);
            }
        }
    }

    List<List<Vector2Int>> ExtractSegments()
    {
        var segments = new List<List<Vector2Int>>();
        var visited = new HashSet<RoadNode>();

        foreach (var node in nodes.Values)
        {
            if (visited.Contains(node)) continue;
            if (node.neighbors.Count == 2) continue;

            foreach (var neighbor in node.neighbors)
            {
                if (visited.Contains(neighbor)) continue;

                var seg = new List<Vector2Int>();
                RoadNode current = node;
                RoadNode prev = null;

                while (current != null && !visited.Contains(current))
                {
                    seg.Add(current.pos);
                    visited.Add(current);
                    var next = current.neighbors
                        .FirstOrDefault(n => n != prev && !visited.Contains(n));
                    prev = current;
                    current = next;
                }
                if (seg.Count > 1) segments.Add(seg);
            }
        }
        return segments;
    }

    void BuildMesh(List<Vector2Int> segment)
    {
        var verts = new List<Vector3>();
        var tris = new List<int>();

        for (int i = 0; i < segment.Count - 1; i++)
        {
            Vector3 p0 = new Vector3(segment[i].x * gridscale, 0, segment[i].y * gridscale);
            Vector3 p1 = new Vector3(segment[i + 1].x * gridscale, 0, segment[i + 1].y * gridscale);

            Vector3 dir = (p1 - p0).normalized;
            Vector3 perp = Vector3.Cross(Vector3.up, dir) * (roadWidth * 0.5f);

            int vi = verts.Count;
            verts.Add(p0 - perp);
            verts.Add(p0 + perp);
            verts.Add(p1 - perp);
            verts.Add(p1 + perp);

            tris.Add(vi); tris.Add(vi + 1); tris.Add(vi + 2);
            tris.Add(vi + 1); tris.Add(vi + 3); tris.Add(vi + 2);
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();

        var go = new GameObject("RoadSegment");
        Instantiate(go);
        go.transform.parent = transform;
        var mf = go.AddComponent<MeshFilter>();
        mf.mesh = mesh;
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = roadMaterial;
    }

    void BuildSmoothMesh(List<Vector2Int> segment)
    {
        var verts = new List<Vector3>();
        var tris = new List<int>();

        for (int i = 0; i < segment.Count; i++)
        {
            Vector3 p = new Vector3(segment[i].x * gridscale, 0, segment[i].y * gridscale);

            // find tangent
            Vector3 forward = Vector3.zero;
            if (i > 0) forward += (p - new Vector3(segment[i - 1].x * gridscale, 0, segment[i - 1].y * gridscale)).normalized;
            if (i < segment.Count - 1) forward += (new Vector3(segment[i + 1].x * gridscale, 0, segment[i + 1].y * gridscale) - p).normalized;
            forward.Normalize();

            Vector3 side = Vector3.Cross(Vector3.up, forward) * (roadWidth * 0.5f);

            verts.Add(p - side); // left
            verts.Add(p + side); // right

            if (i < segment.Count - 1)
            {
                int vi = i * 2;
                tris.Add(vi);
                tris.Add(vi + 1);
                tris.Add(vi + 2);

                tris.Add(vi + 1);
                tris.Add(vi + 3);
                tris.Add(vi + 2);
            }
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();

        var go = new GameObject("RoadSmooth");
        go.transform.parent = transform;
        go.AddComponent<MeshFilter>().mesh = mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = roadMaterial;
        Instantiate(go);
    }
}
