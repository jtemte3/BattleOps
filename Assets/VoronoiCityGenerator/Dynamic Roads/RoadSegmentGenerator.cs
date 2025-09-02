using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class RoadSegmentGenerator : MonoBehaviour
{
    public RoadSegmentTracer tracer;
    public RoadSegmentCompressor compressor;
    public IntersectionNeighborFinder intersectionFinder;
    [Header("Road Settings")]
    public List<Vector2Int> roadPositions = new List<Vector2Int>();
    public List<Vector2Int> intersections = new List<Vector2Int>();
    public List<Neighbors> intersectionMap = new List<Neighbors>();
    public float roadWidth = 1f;
    public float roadThickness = 0.1f;
    public float roadGroundOffset = 0.1f;
    public float uvScale = 0.1f;
    public float gridScale = 3f;
    public int curveResolution = 6; // points per segment
    public Material roadMaterial;
    public Material roadCapMaterial;

    private List<List<Vector2Int>> segments = new();
    private List<Color> segmentColors = new();
    private List<Color> intersectionColors = new();

    public bool drawGizmos = true;
    public bool drawInvalidPaths = true;
    public bool drawIntersectionConnections = true;
    public bool drawSingleIntersectionConnections = true;
    public Vector2Int testIntersection = new Vector2Int();
    public bool makeRoadMeshes = true;
    public List<List<Vector2Int>> invalidPaths = new();

    // --------------------------
    // Build neighbor map (diagonal + cardinal)
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
                Vector2Int n = pos + d;
                if (roadSet.Contains(n))
                {
                    map[pos].Add(n);
                }
            }
        }

        return map;
    }

    // --------------------------
    // Trace connected segments
    // --------------------------
    
    List<List<Vector2Int>> PointBasedSegmentTracing(Dictionary<Vector2Int, List<Vector2Int>> map)
    {
        List<Vector2Int> visitedPositions = new();
        List<List<Vector2Int>> result = new();
        List<Vector2Int> firstPos = new();
        List<Vector2Int> lastPos = new();

        foreach (var kvp in map)
        {
            var start = kvp.Key;

            if (visitedPositions.Contains(start))
            {
                continue;
            }
            
            foreach (var neighbor in kvp.Value)
            {
                if (visitedPositions.Contains(neighbor))
                {
                    if (!map.ContainsKey(neighbor) /*|| !firstPos.Contains(neighbor) || !lastPos.Contains(start)*/)
                    {
                        continue;
                    }
                }

                
                List<Vector2Int> currentSegment = new() { start, neighbor };

                visitedPositions.Add(start);
                visitedPositions.Add(neighbor);

                var prev = start;
                var curr = neighbor;

                while (true)
                {
                    if (!map.TryGetValue(curr, out var neighbors))
                    {
                        break;
                    }

                    // Only consider fresh edges
                    var candidates = new List<Vector2Int>();

                    bool reachedEnd = false;
                    foreach (var n in neighbors)
                    {
                        if (n != prev /*&& !visitedPositions.Contains(n)*/)
                        {
                            candidates.Add(n);

                            if (visitedPositions.Contains(n))
                            {
                                reachedEnd = true;
                                currentSegment.Add(n);
                            }
                        }
                    }

                    if (candidates.Count == 0 || reachedEnd == true)
                    {
                        break;
                    }

                    var next = ChooseNextStraight(prev, curr, candidates);
                    currentSegment.Add(next);

                    visitedPositions.Add(next);

                    prev = curr;
                    curr = next;

                    // Optional loop stop
                    if (curr == start)
                    {
                        break;
                    }
                }
                result.Add(currentSegment);
            }
        }

        return result;
    }

    List<List<Vector2Int>> TraceSegments(Dictionary<Vector2Int, List<Vector2Int>> map, List<Vector2Int> EdgePos)
    {
        var visitedEdges = new HashSet<(Vector2Int, Vector2Int)>();
        var segments = new List<List<Vector2Int>>();

        foreach (var kvp in map)
        {
            var start = kvp.Key;
            foreach (var neighbor in kvp.Value)
            {
                // Only start if this directed edge is fresh
                if (visitedEdges.Contains((start, neighbor)) || visitedEdges.Contains((neighbor, start)))
                {
                    continue;
                }

                var segment = new List<Vector2Int> { start, neighbor };
                visitedEdges.Add((start, neighbor));
                visitedEdges.Add((neighbor, start));

                var prev = start;
                var curr = neighbor;

                while (true)
                {
                    if (!map.TryGetValue(curr, out var neighbors))
                    {
                        break;
                    }

                    // Only consider fresh edges
                    var candidates = new List<Vector2Int>();
                    foreach (var n in neighbors)
                    {
                        if (n != prev && !visitedEdges.Contains((curr, n)))
                        {
                            candidates.Add(n);
                        }
                    }

                    if (candidates.Count == 0)
                    {
                        break;
                    }

                    var next = ChooseNextStraight(prev, curr, candidates);

                    segment.Add(next);
                    visitedEdges.Add((curr, next));
                    visitedEdges.Add((next, curr));                   

                    prev = curr;
                    curr = next;

                    // Optional loop stop
                    if (curr == start)
                    {
                        break;
                    }
                }

                segments.Add(segment);
            }
        }

        return segments;
    }

    List<List<Vector2Int>> TraceSegmentsOld(Dictionary<Vector2Int, List<Vector2Int>> map, List<Vector2Int> EdgePos)
    {
        var visitedEdges = new HashSet<(Vector2Int, Vector2Int)>();
        List<Vector2Int> visitedPos = new();
        var segments = new List<List<Vector2Int>>();

        foreach (var kvp in map)
        {
            var start = kvp.Key;
            visitedPos.Add(start);
            foreach (var neighbor in kvp.Value)
            {
                // Only start if this directed edge is fresh
                if (visitedEdges.Contains((start, neighbor)) || visitedEdges.Contains((neighbor, start)) || visitedPos.Contains(neighbor))
                {
                    visitedPos.Add(neighbor);
                    continue;
                }

                /*if (EdgePos.Contains(neighbor))
                {
                    visitedPos.Add(neighbor);
                    break;
                }*/

                var segment = new List<Vector2Int> { start, neighbor };
                visitedEdges.Add((start, neighbor));
                visitedEdges.Add((neighbor, start));

                visitedPos.Add(neighbor);

                var prev = start;
                var curr = neighbor;

                while (true)
                {
                    if (!map.TryGetValue(curr, out var neighbors))
                    {
                        break;
                    }

                    // Only consider fresh edges
                    var candidates = new List<Vector2Int>();
                    foreach (var n in neighbors)
                    {
                        if (n != prev && !visitedEdges.Contains((curr, n)) && !visitedPos.Contains(n))
                        {
                            candidates.Add(n);
                        }

                        /*if (visitedPos.Contains(n))
                        {
                            candidates.Add(n);
                            break;
                        }*/
                    }

                    if (candidates.Count == 0)
                    {
                        break;
                    }

                    //var next = ChooseNextStraight(prev, curr, candidates, visitedPos);
                    var next = ChooseNextStraight(prev, curr, candidates);

                    segment.Add(next);
                    visitedEdges.Add((curr, next));
                    visitedEdges.Add((next, curr));

                    visitedPos.Add(curr);
                    visitedPos.Add(next);

                    prev = curr;
                    curr = next;

                    // Optional loop stop
                    if (curr == start)
                    {
                        break;
                    }
                }

                segments.Add(segment);
            }
        }

        return segments;
    }

    // Picks the candidate that best continues the current heading (max dot product).
    Vector2Int ChooseNextStraight(Vector2Int prev, Vector2Int curr, List<Vector2Int> candidates)
    {
        var dirIn = new Vector2(curr.x - prev.x, curr.y - prev.y);
        if (dirIn == Vector2.zero) return candidates[0];

        dirIn.Normalize();
        float bestScore = float.NegativeInfinity;
        int bestIdx = 0;

        for (int i = 0; i < candidates.Count; i++)
        {
            var d = new Vector2Int(candidates[i].x - curr.x, candidates[i].y - curr.y);

            float score = Vector2.Dot(dirIn, d); // closer to 1 == straighter

            if (score > bestScore)
            {
                bestScore = score;
                bestIdx = i;
            }
        }
        return candidates[bestIdx];
    }

    List<List<Vector2Int>> DetectSmallSegmentGaps(List<List<Vector2Int>> segmentsList, Dictionary<Vector2Int, List<Vector2Int>> map, List<Vector2Int> edgePos)
    {
        var result = new List<List<Vector2Int>>();
        List<Vector2Int> endNodes = new();
        List<Vector2Int> frontNodes = new();

        foreach (var segment in segmentsList)
        {
            endNodes.Add(segment[segment.Count -1]);
        }
        foreach (var segment in segmentsList)
        {
            frontNodes.Add(segment[0]);
        }

        Vector2Int[] dirs = {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
            new Vector2Int(1,1), new Vector2Int(-1,1), new Vector2Int(1,-1), new Vector2Int(-1,-1)
        };

        foreach (var seg in segmentsList)
        {
            var current = seg;

            bool isValidSegment = true;
            foreach (Vector2Int pos in seg)
            {
                if (edgePos.Contains(pos))
                {
                    isValidSegment = false;
                }
            }

            if (!isValidSegment && seg.Count < 4)
            {
                result.Add(seg);
                continue;
            }

            Vector2Int frontNode = seg[0];
            if (!endNodes.Contains(frontNode))
            {
                foreach (Vector2Int dir in dirs)
                {
                    Vector2Int node = frontNode + dir;

                    if (!seg.Contains(node) && map.ContainsKey(node) && !endNodes.Contains(node))
                    {
                        current.Insert(0, node);
                        break;
                    }
                }
            }

            Vector2Int endNode = seg[seg.Count - 1];

            if (!frontNodes.Contains(endNode))
            {
                foreach (Vector2Int dir in dirs)
                {
                    Vector2Int node = endNode + dir;

                    if (!seg.Contains(node) && map.ContainsKey(node) && !edgePos.Contains(endNode) && !frontNodes.Contains(node))
                    {
                        current.Add(node);
                        break;
                    }
                }
            }

            Vector2Int midNode = seg[(seg.Count - 1) / 2];

            if(!endNodes.Contains(midNode) && !frontNodes.Contains(midNode))
            {
                foreach (Vector2Int dir in dirs)
                {
                    Vector2Int node = midNode + dir;

                    if (!seg.Contains(node) && map.ContainsKey(node) && !endNodes.Contains(node) && !frontNodes.Contains(node)/*&& !edgePos.Contains(midNode)*/)
                    {
                        //current.Add(node);
                        current.Insert(((seg.Count - 1) / 2) + 1, node);
                        break;
                    }
                }
            }

            // Add the tail segment
            if (current.Count > 1)
            {
                result.Add(current);
            }
        }

        return result;
    }

    List<Vector2Int> IdentifyIntersections()
    {
        List<Vector2Int> intersectionPositions = new();

        for (int i = 0; i < segments.Count; i++)
        {
            for (int n = 0; n < segments[i].Count; n++)
            {
                for (int j = 0; j < segments.Count; j++)
                {
                    if (j == i)
                    {
                        continue;
                    }
                    if (segments[j].Contains(segments[i][n]))
                    {
                        if (!intersectionPositions.Contains(segments[i][n]))
                        {
                            intersectionPositions.Add(segments[i][n]);
                        }
                    }
                }
            }
        }

        return intersectionPositions;
    }

    List<List<Vector2Int>> SplitAtIntersections(List<List<Vector2Int>> segmentsList, List<Vector2Int> intersectionList, List<Vector2Int> edgePos)
    {
        var result = new List<List<Vector2Int>>();

        foreach (var seg in segmentsList)
        {
            var current = new List<Vector2Int>();
            for (int i = 0; i < seg.Count; i++)
            {
                var node = seg[i];
                current.Add(node);

                // If this is an intersection (degree > 2) and not the first node
                if (i > 0 && intersectionList.Contains(node) && i < seg.Count)
                {
                    // Commit current segment
                    if (current.Count > 1)
                    {
                        result.Add(new List<Vector2Int>(current));
                    }

                    // Start new segment beginning at this intersection
                    current = new List<Vector2Int> { node };
                }
                else if (i > 0 && edgePos.Contains(node) && i < seg.Count)
                {
                    // Commit current segment
                    if (current.Count > 1)
                    {
                        result.Add(new List<Vector2Int>(current));
                    }

                    // Start new segment beginning at this intersection
                    current = new List<Vector2Int> { node };
                }
            }

            // Add the tail segment
            if (current.Count > 1)
                result.Add(current);
        }

        return result;
    }

    // Sample an arc that passes exactly through start, mid, end (XZ plane). 
    // Falls back to a straight line if points are nearly collinear.
    List<Vector3> SampleCircularArc(Vector3 start, Vector3 mid, Vector3 end, int samples)
    {
        Vector2 A = new Vector2(start.x, start.z);
        Vector2 B = new Vector2(mid.x, mid.z);
        Vector2 C = new Vector2(end.x, end.z);

        // Circumcenter of triangle ABC
        float a = A.x - B.x, b = A.y - B.y;
        float c = A.x - C.x, d = A.y - C.y;
        float e = ((A.x * A.x - B.x * B.x) + (A.y * A.y - B.y * B.y)) * 0.5f;
        float f = ((A.x * A.x - C.x * C.x) + (A.y * A.y - C.y * C.y)) * 0.5f;
        float det = a * d - b * c;

        var pts = new List<Vector3>();

        // Nearly collinear → linear sample from start to end
        if (Mathf.Abs(det) < 1e-5f)
        {
            for (int i = 0; i <= samples; i++)
            {
                float t = i / (float)samples;
                Vector2 p2 = Vector2.Lerp(A, C, t);
                pts.Add(new Vector3(p2.x, start.y, p2.y));
            }
            return pts;
        }

        float cx = (d * e - b * f) / det;
        float cy = (-c * e + a * f) / det;
        Vector2 O = new Vector2(cx, cy);
        float r = Vector2.Distance(O, A);

        float angA = Mathf.Atan2(A.y - O.y, A.x - O.x);
        float angB = Mathf.Atan2(B.y - O.y, B.x - O.x);
        float angC = Mathf.Atan2(C.y - O.y, C.x - O.x);

        // CCW sweep from A to C
        float twoPI = 2f * Mathf.PI;
        float ccw_AC = Mathf.Repeat(angC - angA, twoPI); // in [0, 2π)
        float ccw_AM = Mathf.Repeat(angB - angA, twoPI);

        bool useCCW = ccw_AM > 0f && ccw_AM < ccw_AC;    // does CCW A→C pass through mid?
        float sweep = useCCW ? ccw_AC : (ccw_AC - twoPI); // CW if not CCW

        for (int i = 0; i <= samples; i++)
        {
            float t = i / (float)samples;
            float ang = angA + sweep * t;
            Vector2 p = O + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * r;
            pts.Add(new Vector3(p.x, start.y, p.y));
        }

        return pts;
    }

    // Wrapper that picks first, middle, last nodes of a segment and samples the arc
    List<Vector3> SampleThreePointArc(List<Vector2Int> segment, float gridScale, int samples)
    {
        if (segment == null || segment.Count == 0)
            return new List<Vector3>();

        int i0 = 0;
        int i1 = segment.Count / 2;                 // middle node index
        int i2 = segment.Count - 1;

        Vector3 start = new Vector3(segment[i0].x * gridScale, 0f, segment[i0].y * gridScale);
        Vector3 mid = new Vector3(segment[i1].x * gridScale, 0f, segment[i1].y * gridScale);
        Vector3 end = new Vector3(segment[i2].x * gridScale, 0f, segment[i2].y * gridScale);

        /*List<Vector3> pts = new()
        {
            start, mid, end
        };*/

        return SampleCircularArc(start, mid, end, samples);
        //return pts;
    }

    // --------------------------
    // Build road mesh along spline
    // --------------------------
    void BuildRoadMesh(List<Vector3> splinePoints, int roadId)
    {
        var verts = new List<Vector3>();
        var tris = new List<int>();
        var uvs = new List<Vector2>();

        for (int i = 0; i < splinePoints.Count; i++)
        {
            Vector3 forward = Vector3.zero;
            if (i < splinePoints.Count - 1) 
            {
                forward += (splinePoints[i + 1] - splinePoints[i]).normalized;
            }
            if (i > 0)
            {
                forward += (splinePoints[i] - splinePoints[i - 1]).normalized;
            }

            forward.Normalize();

            Vector3 left = new Vector3(-forward.z, 0, forward.x).normalized;
            Vector3 heightOffset = Vector3.up * roadGroundOffset;

            // Build ring of 4 verts (top-left, top-right, bottom-left, bottom-right)
            Vector3 v0 = splinePoints[i] + left * -roadWidth - Vector3.up * roadThickness + heightOffset; // bottom left
            Vector3 v1 = splinePoints[i] + left * roadWidth - Vector3.up * roadThickness + heightOffset;  // bottom right
            Vector3 v2 = splinePoints[i] + left * -roadWidth + Vector3.up * roadThickness + heightOffset; // top left
            Vector3 v3 = splinePoints[i] + left * roadWidth + Vector3.up * roadThickness + heightOffset;  // top right

            verts.Add(v0); verts.Add(v1); verts.Add(v2); verts.Add(v3);

            // World-space UVs
            // Top verts -> use XZ

            // World-space UVs (X/Z projected)
            for (int j = 0; j < 4; j++)
            {
                Vector3 v = verts[verts.Count - 4 + j];
                uvs.Add(new Vector2(v.x * uvScale, v.z * uvScale));
            }

        }

        int vertsPerRing = 4;
        for (int i = 0; i < splinePoints.Count - 1; i++)
        {
            int start = i * vertsPerRing;

            // Top quad
            tris.Add(start);
            tris.Add(start + vertsPerRing);
            tris.Add(start + 1);

            tris.Add(start + 1);
            tris.Add(start + vertsPerRing);
            tris.Add(start + vertsPerRing + 1);

            // Bottom quad (flip winding)
            tris.Add(start + 2);
            tris.Add(start + 3);
            tris.Add(start + vertsPerRing + 2);

            tris.Add(start + 3);
            tris.Add(start + vertsPerRing + 3);
            tris.Add(start + vertsPerRing + 2);

            // Left side
            tris.Add(start);
            tris.Add(start + 2);
            tris.Add(start + vertsPerRing);

            tris.Add(start + 2);
            tris.Add(start + vertsPerRing + 2);
            tris.Add(start + vertsPerRing);

            // Right side
            tris.Add(start + 1);
            tris.Add(start + vertsPerRing + 1);
            tris.Add(start + 3);

            tris.Add(start + 3);
            tris.Add(start + vertsPerRing + 1);
            tris.Add(start + vertsPerRing + 3);
        }

        // Cap start
        int s = 0;
        tris.Add(s);
        tris.Add(s + 1);
        tris.Add(s + 2);

        tris.Add(s + 1);
        tris.Add(s + 3);
        tris.Add(s + 2);

        // Cap end
        int e = (splinePoints.Count - 1) * vertsPerRing;
        tris.Add(e);
        tris.Add(e + 2);
        tris.Add(e + 1);

        tris.Add(e + 1);
        tris.Add(e + 2);
        tris.Add(e + 3);



        //var mesh = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
        var mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);

        mesh.SetUVs(0, uvs);
        //mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        var go = new GameObject("Road_" + roadId);
        go.transform.parent = transform;
        go.AddComponent<MeshFilter>().mesh = mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = roadMaterial;
    }

    void BuildIntersectionCylinder(Vector3 position, int id)
    {
        int segments = 32;
        float radius = roadWidth;
        float halfH = roadThickness - .001f ;
        Vector3 offset = Vector3.up * roadGroundOffset;

        var verts = new List<Vector3>();
        var tris = new List<int>();
        var uvs = new List<Vector2>();

        // ---- side rings ----
        for (int i = 0; i < segments; i++)
        {
            float a = (Mathf.PI * 2f * i) / segments;
            float x = Mathf.Cos(a) * radius;
            float z = Mathf.Sin(a) * radius;

            Vector3 top = new Vector3(x, halfH, z) + position + offset;
            Vector3 bottom = new Vector3(x, -halfH, z) + position + offset;

            verts.Add(top); uvs.Add(new Vector2(top.x * uvScale, top.z * uvScale));
            verts.Add(bottom); uvs.Add(new Vector2(bottom.x * uvScale, bottom.z * uvScale));
        }

        // ---- sides ----
        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;
            int i0 = i * 2;
            int i1 = next * 2;

            tris.Add(i0); tris.Add(i1); tris.Add(i0 + 1);
            tris.Add(i0 + 1); tris.Add(i1); tris.Add(i1 + 1);
        }

        // ---- NEW "TOP" CAP (actually at -halfH, flipped) ----
        int topRimStart = verts.Count;
        for (int i = 0; i < segments; i++)
        {
            Vector3 v = verts[i * 2 + 1]; // copy bottom ring
            verts.Add(v);
            uvs.Add(new Vector2(v.x * uvScale, -v.z * uvScale));
        }
        int topCenter = verts.Count;
        Vector3 topC = new Vector3(0, -halfH, 0) + position + offset;
        verts.Add(topC);
        uvs.Add(new Vector2(topC.x * uvScale, -topC.z * uvScale));

        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;
            tris.Add(topCenter);
            tris.Add(topRimStart + i);
            tris.Add(topRimStart + next);
        }

        // ---- NEW "BOTTOM" CAP (actually at +halfH, flipped) ----
        int botRimStart = verts.Count;
        for (int i = 0; i < segments; i++)
        {
            Vector3 v = verts[i * 2]; // copy top ring
            verts.Add(v);
            uvs.Add(new Vector2(v.x * uvScale, -v.z * uvScale));
        }
        int botCenter = verts.Count;
        Vector3 botC = new Vector3(0, halfH, 0) + position + offset;
        verts.Add(botC);
        uvs.Add(new Vector2(botC.x * uvScale, -botC.z * uvScale));

        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;
            tris.Add(botCenter);
            tris.Add(botRimStart + next);   // reversed winding to face outward
            tris.Add(botRimStart + i);
        }

        // ---- finalize mesh ----
        var mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetUVs(0, uvs);
        //mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        var go = new GameObject("Intersection_" + id);
        go.transform.parent = transform;
        go.AddComponent<MeshFilter>().mesh = mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = roadMaterial;
    }

    void BuildRoadMesh2D(List<Vector3> splinePoints, int roadId)
    {
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        int startVert = verts.Count;

        for (int i = 0; i < splinePoints.Count; i++)
        {
            Vector3 pos = splinePoints[i];

            Vector3 tangent = new();
            if (i == splinePoints.Count - 1)
            {
                Vector3 last = splinePoints[i - 1];
                tangent = (pos - last).normalized;
            }
            else
            {
                Vector3 next = splinePoints[i + 1];
                tangent = (next - pos).normalized;
            }
            
            Vector3 side = Vector3.Cross(Vector3.up, tangent) * roadWidth * 0.5f;

            verts.Add(pos - side);
            verts.Add(pos + side);

            if (verts.Count - startVert >= 4)
            {
                int vi = verts.Count - 4;
                tris.Add(vi); tris.Add(vi + 1); tris.Add(vi + 2);
                tris.Add(vi + 1); tris.Add(vi + 3); tris.Add(vi + 2);
            }

            float u = i / (float)(splinePoints.Count);
            uvs.Add(new Vector2(0, u));
            uvs.Add(new Vector2(1, u));
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();

        var go = new GameObject("Road_" + roadId);
        go.transform.parent = transform;
        go.AddComponent<MeshFilter>().mesh = mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = roadMaterial;
    }

    List<List<Vector2Int>> CleanupSegments(List<Vector2Int> EdgePos)
    {
        List<List<Vector2Int>> validSegments = new();

        foreach (var segment in segments)
        {
            if (segment.Count > 4)
            {
                validSegments.Add(segment);
            }
            else
            {
                foreach(Vector2Int node in segment)
                {
                    if (EdgePos.Contains(node))
                    {
                        validSegments.Add(segment);
                    }
                }
            }
        }

        return validSegments;
    }

    // --------------------------
    // Generate all road segments
    // --------------------------
    public void GenerateRoadSegments(List<Vector2Int> intersectionPositions, List<Vector2Int> edgePos)
    {
        var neighborMap = BuildNeighborMap();
        var roadSet = new HashSet<Vector2Int>(roadPositions);

        intersectionMap = intersectionFinder.FindNeighbors(neighborMap, intersectionPositions, roadPositions);
        //segments = compressor.BuildCompressedGraph(neighborMap, intersectionPositions);
        //segments = tracer.TraceAll(neighborMap, intersectionPositions);

        List<Vector2Int> allIntersections = new();
        allIntersections.AddRange(intersectionPositions);
        allIntersections.AddRange(edgePos);

        foreach (var neighbor in intersectionMap)
        {
            //neighbor.neighborPaths = tracer.TraceAll(neighborMap, neighbor.neighborPos);
            if(neighbor.neighborPos.Count == 1)
            {
                List<Vector2Int> path = RoadPathfinder.FindStraightestPath(neighbor.position, neighbor.neighborPos[0], roadPositions, new());
                if (path.Count > 0)
                {
                    neighbor.neighborPaths.Add(path);
                }
            }
            else
            {
                foreach (var neighborPos in neighbor.neighborPos)
                {
                    int index = -1;
                    index = allIntersections.IndexOf(neighborPos);
                    List<Vector2Int> updatedIntersections = allIntersections;
                    if (index > -1 && index < updatedIntersections.Count)
                    {
                        updatedIntersections.RemoveAt(index);
                    }
                    List<Vector2Int> path = RoadPathfinder.FindStraightestPath(neighbor.position, neighborPos, roadPositions, updatedIntersections);
                    if (path.Count > 0)
                    {
                        neighbor.neighborPaths.Add(path);
                    }
                }

                
            }

            intersections.Add(neighbor.position);
        }

        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0),   // E
            new Vector2Int(-1, 0),  // W
            new Vector2Int(0, 1),   // N
            new Vector2Int(0, -1),  // S
            new Vector2Int(1, 1),   // NE
            new Vector2Int(-1, 1),  // NW
            new Vector2Int(1, -1),  // SE
            new Vector2Int(-1, -1), // SW
        };

        List<Vector2Int> radiusList = new();
        for(int i = 1; i <= 2; i++)
        {
            foreach(Vector2Int dir in directions)
            {
                radiusList.Add(new Vector2Int(dir.x * i, dir.y));
                radiusList.Add(new Vector2Int(dir.x, dir.y * i));
                radiusList.Add(new Vector2Int(dir.x * i, dir.y * i));
            }
        }


        foreach (var intersectionObj in intersectionMap)
        {
            List<Vector2Int> validNeighbors = new List<Vector2Int>();
            List<List<Vector2Int>> validPaths = new();

            //bool isNeighborValid = true;
            foreach (var path in intersectionObj.neighborPaths)
            {
                bool isPathValid = true;
                if (path.Count > 7)
                {
                    for (int i = 4; i < path.Count - 3; i++)
                    {
                        bool isValid = true;

                        Vector2Int pos = path[i];

                        foreach (var dir in radiusList)
                        {
                            if (intersectionPositions.Contains(pos + dir))
                            {
                                isValid = false;
                                break;
                            }
                        }

                        if (!isValid)
                        {
                            Debug.Log(intersectionObj.position + " -> " + path[path.Count - 1] + " is an invalid path");
                            Debug.Log(path.ToSeparatedString(" -> "));
                            invalidPaths.Add(path);
                            isPathValid = false;
                            break;
                        }
                    }
                }
                if (!isPathValid)
                {
                    continue;
                }
                else
                {
                    validNeighbors.Add(path[path.Count - 1]);
                    validPaths.Add(path);
                }
            }

            intersectionObj.neighborPos = validNeighbors;
            intersectionObj.neighborPaths = validPaths;
        }

        foreach (var neighbor in intersectionMap)
        {
            foreach(var path in neighbor.neighborPaths)
            {
                Vector2Int startPos = path[0];
                Vector2Int endPos = path[path.Count-1];
                bool isDup = false;
                foreach(var seg in segments)
                {
                    if (seg[0] == endPos && seg[seg.Count-1] == startPos)
                    {
                        isDup = true;
                    }
                }
                if (!isDup)
                {
                    segments.Add(path);
                }
            }
        }
        //segments = tracer.TraceAll(neighborMap, intersectionPositions);

        //segments = TraceSegments(neighborMap, edgePos);
        //segments = PointBasedSegmentTracing(neighborMap);

        //segments = DetectSmallSegmentGaps(segments, neighborMap, edgePos);

        //segments = CleanupSegments(edgePos);


        //intersections = IdentifyIntersections();
        //segments = SplitAtIntersections(segments, intersections, edgePos);

        for (int i = 0; i < segments.Count; i++)
        {
            float r = Random.Range(0.0f, 1.0f);
            float g = Random.Range(0.0f, 1.0f);
            float b = Random.Range(0.0f, 1.0f);
            Color segmentColor = new(r, g, b, 1);
            segmentColors.Add(segmentColor);
        }

        for (int i = 0; i < intersectionMap.Count; i++)
        {
            float r = Random.Range(0.0f, 1.0f);
            float g = Random.Range(0.0f, 1.0f);
            float b = Random.Range(0.0f, 1.0f);
            Color intersectionColor = new(r, g, b, 1);
            intersectionColors.Add(intersectionColor);
        }

        if (makeRoadMeshes)
        {
            foreach (var seg in segments)
            {
                //var spline = SimpleSampleSplineWithBranch(seg, roadSet);
                var spline = SampleThreePointArc(seg, gridScale, curveResolution);
                BuildRoadMesh(spline, segments.IndexOf(seg));
                //BuildRoadMesh2D(spline, segments.IndexOf(seg));
            }

            foreach(var intersection in intersections)
            {
                Vector3 pos = new(intersection.x * gridScale, 0, intersection.y * gridScale);
                BuildIntersectionCylinder(pos, intersections.IndexOf(intersection));
            }
        }
    }

#if (UNITY_EDITOR)
    public void OnDrawGizmos()
    {
        if (drawGizmos)
        {
            if (segments.Count > 0)
            {
                for (int s = 0; s < segments.Count; s++)
                {
                    List<Vector2Int> segment = segments[s];

                    for (int i = 0; i < segment.Count; i++)
                    {
                        if (i != segment.Count - 1)
                        {
                            int nextIndex = i + 1;
                            Vector3 pos = new(segment[i].x * gridScale, 1, segment[i].y * gridScale);
                            Vector3 nextPos = new(segment[nextIndex].x * gridScale, 1, segment[nextIndex].y * gridScale);

                            Gizmos.color = segmentColors[s];
                            Gizmos.DrawLine(pos, nextPos);

                            float value = ((float)i / (float)segment.Count);
                            Gizmos.color = new Color(value, value, value, 1);
                            //Gizmos.color = Color.white;
                            Gizmos.DrawSphere(pos, 0.1f);

                        }
                    }
                }
            }

            if (invalidPaths.Count > 0 && drawInvalidPaths)
            {
                Gizmos.color = Color.red;
                foreach(var path in invalidPaths)
                {
                    for (int i = 0; i < path.Count; i++)
                    {
                        if (i != path.Count - 1)
                        {
                            int nextIndex = i + 1;
                            Vector3 pos = new(path[i].x * gridScale, 1, path[i].y * gridScale);
                            Vector3 nextPos = new(path[nextIndex].x * gridScale, 1, path[nextIndex].y * gridScale);

                            Gizmos.DrawLine(pos, nextPos);

                        }
                    }
                }
            }

            if (intersections.Count > 0)
            {
                Gizmos.color = new Color(0, 1, 0, .25f);
                foreach (Vector2Int intersection in intersections)
                {
                    Vector3 pos = new(intersection.x * gridScale, 1, intersection.y * gridScale);
                    Gizmos.DrawSphere(pos, 1f);
                }
            }

            if (intersectionMap.Count > 0 && drawIntersectionConnections)
            {
                if (drawSingleIntersectionConnections)
                {
                    if (intersections.Contains(testIntersection))
                    {
                        Neighbors intersection = new();
                        foreach (Neighbors possibleIntersection in intersectionMap)
                        {
                            if (possibleIntersection.position.Equals(testIntersection))
                            {
                                intersection = possibleIntersection;
                                break;
                            }
                        }

                        Gizmos.color = intersectionColors[intersectionMap.IndexOf(intersection)];

                        Vector3 pos = new(intersection.position.x * gridScale, 1, intersection.position.y * gridScale);
                        Gizmos.DrawSphere(pos, 1f);

                        foreach (Vector2Int neightborPosition in intersection.neighborPos)
                        {
                            Vector3 neighPos = new(neightborPosition.x * gridScale, 1, neightborPosition.y * gridScale);
                            Gizmos.DrawLine(pos, neighPos);
                        }

                    }
                }
                else
                {
                    foreach (Neighbors intersection in intersectionMap)
                    {
                        Gizmos.color = intersectionColors[intersectionMap.IndexOf(intersection)];

                        Vector3 pos = new(intersection.position.x * gridScale, 1, intersection.position.y * gridScale);
                        Gizmos.DrawSphere(pos, 1f);

                        foreach (Vector2Int neightborPosition in intersection.neighborPos)
                        {
                            Vector3 neighPos = new(neightborPosition.x * gridScale, 1, neightborPosition.y * gridScale);
                            Gizmos.DrawLine(pos, neighPos);
                        }

                    }
                }
            }
        }
    }
#endif
}
