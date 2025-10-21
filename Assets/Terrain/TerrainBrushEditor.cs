using System;
using System.IO;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class TerrainBrushEditor : MonoBehaviour
{
    public enum BrushMode { None, RaiseLower, SetHeight, Smooth }

    [Header("Brush Settings")]
    public BrushMode brushMode = BrushMode.None;
    public float brushRadius = 5f;
    public float brushStrength = 0.5f;
    public float targetHeight = 0f;
    public LayerMask terrainMask;

    [HideInInspector] public Mesh mesh;
    [HideInInspector] public float[] heightMap;
    Vector3[] verts;

    void OnEnable()
    {
        mesh = GetComponent<MeshFilter>().sharedMesh;
        if (mesh == null) return;

        verts = mesh.vertices;
        if (heightMap == null || heightMap.Length != verts.Length)
        {
            heightMap = new float[verts.Length];
            for (int i = 0; i < verts.Length; i++)
                heightMap[i] = verts[i].y;
        }
    }

    public void ApplyBrush(Vector3 worldHitPoint, float deltaTime)
    {
        if (mesh == null || brushMode == BrushMode.None) return;

        Vector3 hitPoint = transform.InverseTransformPoint(worldHitPoint);
        float sqrRadius = brushRadius * brushRadius;

        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 v = verts[i];
            float sqrDist = (new Vector2(v.x, v.z) - new Vector2(hitPoint.x, hitPoint.z)).sqrMagnitude;
            if (sqrDist <= sqrRadius)
            {
                float falloff = 1f - Mathf.Sqrt(sqrDist) / brushRadius;
                float delta = brushStrength * falloff * deltaTime;

                switch (brushMode)
                {
                    case BrushMode.RaiseLower:
                        verts[i].y += delta;
                        break;
                    case BrushMode.SetHeight:
                        verts[i].y = Mathf.MoveTowards(v.y, targetHeight, delta);
                        break;
                    case BrushMode.Smooth:
                        SmoothVertex(i, delta);
                        break;
                }

                heightMap[i] = verts[i].y;
            }
        }

        mesh.vertices = verts;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        var mc = GetComponent<MeshCollider>();
        if (mc)
        {
            mc.sharedMesh = null;
            mc.sharedMesh = mesh;
        }
    }

    void SmoothVertex(int index, float delta)
    {
        int width = (int)Mathf.Sqrt(verts.Length);
        int x = index % width;
        int z = index / width;

        float sum = 0f;
        int count = 0;

        for (int dz = -1; dz <= 1; dz++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                int nx = x + dx;
                int nz = z + dz;
                if (nx >= 0 && nx < width && nz >= 0 && nz < width)
                {
                    sum += verts[nz * width + nx].y;
                    count++;
                }
            }
        }

        float avg = sum / count;
        verts[index].y = Mathf.Lerp(verts[index].y, avg, delta);
    }

    public void SampleTargetHeight(Vector3 worldPoint)
    {
        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
        targetHeight = localPoint.y;
    }

    // --- Heightmap Save/Load ---
    [Serializable]
    public class HeightRangeData { public float minHeight; public float maxHeight; }

    public void SaveHeightMap(string path)
    {
        if (mesh == null) return;

        int width = (int)Mathf.Sqrt(verts.Length);
        int height = width;

        float minH = float.MaxValue, maxH = float.MinValue;
        for (int i = 0; i < verts.Length; i++)
        {
            if (verts[i].y < minH) minH = verts[i].y;
            if (verts[i].y > maxH) maxH = verts[i].y;
        }

        float range = Mathf.Max(0.0001f, maxH - minH);
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        for (int z = 0; z < height; z++)
            for (int x = 0; x < width; x++)
            {
                int idx = z * width + x;
                float n = (verts[idx].y - minH) / range;
                tex.SetPixel(x, z, new Color(n, n, n));
            }

        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());

        string jsonPath = Path.ChangeExtension(path, ".json");
        HeightRangeData hr = new HeightRangeData { minHeight = minH, maxHeight = maxH };
        File.WriteAllText(jsonPath, JsonUtility.ToJson(hr));

        Debug.Log($"Heightmap saved: {path} (+ JSON: {jsonPath})");
    }

    public void LoadHeightMap(string path, bool restoreOriginalRange = true)
    {
        if (!File.Exists(path)) { Debug.LogWarning("Heightmap file not found."); return; }
        if (mesh == null) return;

        byte[] data = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGB24, false);
        tex.LoadImage(data);

        int width = tex.width;
        int height = tex.height;

        if (verts.Length != width * height) { Debug.LogWarning("Heightmap size mismatch."); return; }

        float minH = 0f, maxH = 1f;
        if (restoreOriginalRange)
        {
            string jsonPath = Path.ChangeExtension(path, ".json");
            if (File.Exists(jsonPath))
            {
                try
                {
                    HeightRangeData hr = JsonUtility.FromJson<HeightRangeData>(File.ReadAllText(jsonPath));
                    minH = hr.minHeight; maxH = hr.maxHeight;
                }
                catch { Debug.LogWarning("Failed to load JSON range."); }
            }
        }

        float range = Mathf.Max(0.0001f, maxH - minH);
        for (int z = 0; z < height; z++)
            for (int x = 0; x < width; x++)
            {
                int idx = z * width + x;
                float n = tex.GetPixel(x, z).r;
                verts[idx].y = minH + n * range;
                heightMap[idx] = verts[idx].y;
            }

        mesh.vertices = verts;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        var mc = GetComponent<MeshCollider>();
        if (mc) { mc.sharedMesh = null; mc.sharedMesh = mesh; }

        Debug.Log($"Heightmap loaded: {path}");
    }
}
