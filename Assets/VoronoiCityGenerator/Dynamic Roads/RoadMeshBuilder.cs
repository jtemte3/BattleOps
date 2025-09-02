using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class RoadMeshBuilder
{
    // --------------------------
    // Build road mesh along spline
    // --------------------------
    public static void BuildRoadMesh(List<Vector3> splinePoints, int roadId, float roadGroundOffset, float roadWidth, float roadThickness, float uvScale, Material roadMaterial, Transform parent)
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
        go.transform.parent = parent;
        go.AddComponent<MeshFilter>().mesh = mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = roadMaterial;
    }

    public static void BuildIntersectionCylinder(Vector3 position, int id, float roadGroundOffset, float roadWidth, float roadThickness, float uvScale, Material roadMaterial, Transform parent)
    {
        int segments = 32;
        float radius = roadWidth;
        float halfH = roadThickness - .001f;
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
        go.transform.parent = parent;
        go.AddComponent<MeshFilter>().mesh = mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = roadMaterial;
    }

    public static void BuildRoadMesh2D(List<Vector3> splinePoints, int roadId, float roadGroundOffset, float roadWidth, Material roadMaterial, Transform parent)
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
        go.transform.parent = parent;
        go.AddComponent<MeshFilter>().mesh = mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = roadMaterial;
    }
}
