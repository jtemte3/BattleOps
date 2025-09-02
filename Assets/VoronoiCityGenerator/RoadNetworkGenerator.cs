using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RoadNetworkGenerator : MonoBehaviour
{
    [Header("Road Settings")]
    public List<Vector2Int> roadPositions = new List<Vector2Int>();
    public float roadWidth = 1f;
    public float gridScale = 3f;
    public int curveResolution = 6; // points per segment
    public int intersectionArcResolution = 6;
    public Material roadMaterial;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        if (roadMaterial != null) meshRenderer.material = roadMaterial;
    }

    void Start()
    {
        //GenerateRoadNetwork();
    }

    // --------------------------
    // STEP 1: Build neighborhood map including diagonals
    // --------------------------
    Dictionary<Vector2Int, List<Vector2Int>> BuildNeighborMap()
    {
        HashSet<Vector2Int> roadSet = new HashSet<Vector2Int>(roadPositions);
        Dictionary<Vector2Int, List<Vector2Int>> map = new Dictionary<Vector2Int, List<Vector2Int>>();

        Vector2Int[] dirs = {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
            new Vector2Int(1,1), new Vector2Int(-1,1), new Vector2Int(1,-1), new Vector2Int(-1,-1)
        };

        foreach (var pos in roadPositions)
        {
            map[pos] = new List<Vector2Int>();
            foreach (var d in dirs)
            {
                Vector2Int neigh = pos + d;
                if (roadSet.Contains(neigh)) map[pos].Add(neigh);
            }
        }

        return map;
    }

    // --------------------------
    // STEP 2: Trace connected segments into ordered lists
    // --------------------------
    List<List<Vector2Int>> TraceSegments(Dictionary<Vector2Int, List<Vector2Int>> map)
    {
        HashSet<(Vector2Int, Vector2Int)> visitedEdges = new();
        List<List<Vector2Int>> segments = new();

        foreach (var kvp in map)
        {
            Vector2Int start = kvp.Key;
            foreach (var neighbor in kvp.Value)
            {
                if (visitedEdges.Contains((start, neighbor))) continue;

                List<Vector2Int> segment = new List<Vector2Int> { start, neighbor };
                visitedEdges.Add((start, neighbor));
                visitedEdges.Add((neighbor, start));

                Vector2Int prev = start;
                Vector2Int curr = neighbor;

                while (map[curr].Count == 2) // continue straight segments
                {
                    Vector2Int next = map[curr].First(n => n != prev);
                    if (visitedEdges.Contains((curr, next))) break;

                    segment.Add(next);
                    visitedEdges.Add((curr, next));
                    visitedEdges.Add((next, curr));
                    prev = curr;
                    curr = next;
                }

                segments.Add(segment);
            }
        }

        return segments;
    }

    // --------------------------
    // STEP 3: Catmull-Rom spline interpolation
    // --------------------------
    Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        return 0.5f * ((2f * p1) +
                       (-p0 + p2) * t +
                       (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                       (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
    }

    // --------------------------
    // STEP 4: Build road mesh along a curve
    // --------------------------
    void BuildRoadMesh(List<Vector2Int> segment, List<Vector3> verts, List<int> tris, List<Vector2> uvs)
    {
        int startVert = verts.Count;
        List<Vector3> points = segment.Select(p => new Vector3(p.x * gridScale, 0, p.y * gridScale)).ToList();

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 p0 = i == 0 ? points[i] : points[i - 1];
            Vector3 p1 = points[i];
            Vector3 p2 = points[i + 1];
            Vector3 p3 = (i + 2 < points.Count) ? points[i + 2] : points[i + 1];

            for (int s = 0; s < curveResolution; s++)
            {
                float t = s / (float)curveResolution;
                Vector3 pos = CatmullRom(p0, p1, p2, p3, t);
                Vector3 tangent = (CatmullRom(p0, p1, p2, p3, t + 0.01f) - pos).normalized;
                Vector3 side = Vector3.Cross(Vector3.up, tangent) * roadWidth * 0.5f;

                verts.Add(pos - side);
                verts.Add(pos + side);

                if (verts.Count - startVert >= 4)
                {
                    int vi = verts.Count - 4;
                    tris.Add(vi); tris.Add(vi + 1); tris.Add(vi + 2);
                    tris.Add(vi + 1); tris.Add(vi + 3); tris.Add(vi + 2);
                }

                float u = (s / (float)curveResolution);
                uvs.Add(new Vector2(0, u));
                uvs.Add(new Vector2(1, u));
            }
        }
    }

    // --------------------------
    // STEP 5: Build blended intersection curves
    // --------------------------
    void BuildBlendedIntersection(Vector2Int pos, List<Vector2Int> neighbors,
                                  List<Vector3> verts, List<int> tris, List<Vector2> uvs)
    {
        if (neighbors.Count < 2) return;

        Vector3 center = new Vector3(pos.x * gridScale, 0, pos.y * gridScale);
        int centerIndex = verts.Count;
        verts.Add(center);
        uvs.Add(new Vector2(0.5f, 0.5f));

        // Directions to neighbors
        List<Vector3> dirs = neighbors
            .Select(n => (new Vector3(n.x * gridScale, 0, n.y * gridScale) - center).normalized)
            .ToList();

        // Sort directions clockwise
        dirs.Sort((a, b) => Mathf.Atan2(a.z, a.x).CompareTo(Mathf.Atan2(b.z, b.x)));

        float arcRadius = roadWidth * 0.5f;

        for (int i = 0; i < dirs.Count; i++)
        {
            Vector3 dirA = dirs[i];
            Vector3 dirB = dirs[(i + 1) % dirs.Count];

            List<int> arcIndices = new List<int>();

            for (int s = 0; s <= intersectionArcResolution; s++)
            {
                float t = s / (float)intersectionArcResolution;
                Vector3 dir = Vector3.Slerp(dirA, dirB, t).normalized;
                Vector3 posOnArc = center + dir * arcRadius;

                Vector3 tangent = Vector3.Cross(Vector3.up, dir).normalized;
                verts.Add(posOnArc - tangent * arcRadius);
                verts.Add(posOnArc + tangent * arcRadius);

                uvs.Add(new Vector2(0, t));
                uvs.Add(new Vector2(1, t));

                arcIndices.Add(verts.Count - 2);
            }

            // Triangles along arc
            for (int j = 0; j < arcIndices.Count - 1; j++)
            {
                int vi = arcIndices[j];
                int viNext = arcIndices[j + 1];
                tris.Add(centerIndex);
                tris.Add(vi);
                tris.Add(viNext);
                tris.Add(centerIndex);
                tris.Add(viNext);
                tris.Add(viNext + 1 < verts.Count ? viNext + 1 : viNext);
            }
        }
    }

    // --------------------------
    // STEP 6: Full generation
    // --------------------------
    public void GenerateRoadNetwork()
    {
        var neighborMap = BuildNeighborMap();
        var segments = TraceSegments(neighborMap);

        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        foreach (var seg in segments)
            BuildRoadMesh(seg, verts, tris, uvs);

        /*foreach (var kvp in neighborMap)
        {
            if (kvp.Value.Count > 2)
                BuildBlendedIntersection(kvp.Key, kvp.Value, verts, tris, uvs);
        }*/

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }
}
