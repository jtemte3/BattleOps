using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class MaskedPerlinTerrain : MonoBehaviour
{
    [Header("Grid")]
    [Min(2)] public int xVerts = 201;
    [Min(2)] public int zVerts = 201;
    public float cellSize = 1f;

    [Header("Noise")]
    public int seed = 12345;
    public float amplitude = 20f;
    public float frequency = 0.01f;
    [Range(1, 8)] public int octaves = 4;
    [Range(0.1f, 1f)] public float persistence = 0.5f;
    [Min(1f)] public float lacunarity = 2f;
    public Vector2 noiseOffset;

    [Header("Valley Mask")]
    public Texture2D mask;                 // Grayscale. White = valley by default
    public bool invertMask = false;        // If true, black = valley
    [Range(0f, 1f)] public float maskStrength = 1f; // 0 = ignore mask, 1 = full influence
    public float valleyDepth = 10f;        // Extra depth added where mask is bright
    public AnimationCurve valleyBlend = AnimationCurve.Linear(0, 0, 1, 1); // shape of carve vs mask value

    [Header("Post")]
    [Range(0.5f, 3f)] public float mountainSharpness = 1f; // >1 sharp ridges, <1 smoother
    public bool autoUpdateInEditor = true;

    Mesh mesh;
    Vector3[] verts;
    Vector2[] uvs;
    int[] tris;

    void OnEnable()
    {
        EnsureMesh();
        Generate();
    }

    /*void OnValidate()
    {
        xVerts = Mathf.Max(2, xVerts);
        zVerts = Mathf.Max(2, zVerts);
        cellSize = Mathf.Max(0.001f, cellSize);
        amplitude = Mathf.Max(0f, amplitude);
        frequency = Mathf.Max(0.00001f, frequency);
        persistence = Mathf.Clamp01(persistence);
        lacunarity = Mathf.Max(1f, lacunarity);
        mountainSharpness = Mathf.Max(0.01f, mountainSharpness);

        if (autoUpdateInEditor && enabled)
            Generate();
    }*/

    [ContextMenu("Generate Now")]
    public void Generate()
    {

        EnsureMesh();
        BuildArraysIfNeeded();

        // base uv mapping across the mesh [0..1]
        int i = 0;
        for (int z = 0; z < zVerts; z++)
        {
            for (int x = 0; x < xVerts; x++, i++)
            {
                float u = (float)x / (xVerts - 1);
                float v = (float)z / (zVerts - 1);
                uvs[i] = new Vector2(u, v);
            }
        }

        // heights
        i = 0;
        for (int z = 0; z < zVerts; z++)
        {
            for (int x = 0; x < xVerts; x++, i++)
            {
                Vector2 uv = uvs[i];

                float h = FractalPerlin(
                    (x * cellSize + noiseOffset.x) * frequency,
                    (z * cellSize + noiseOffset.y) * frequency);

                // Sharpen or smooth mountains
                if (!Mathf.Approximately(mountainSharpness, 1f))
                    h = Mathf.Sign(h) * Mathf.Pow(Mathf.Abs(h), mountainSharpness);

                h *= amplitude;

                // Mask sample
                float m = SampleMask(uv);
                if (invertMask) m = 1f - m;

                // Blend curve and strength
                float carve = valleyBlend.Evaluate(m) * maskStrength;

                // Reduce height proportionally and add extra depth
                float hMasked = h * (1f - carve) - (valleyDepth * carve);

                verts[i] = new Vector3(x * cellSize, hMasked, z * cellSize);
            }
        }

        // triangles
        int t = 0;
        int w = xVerts;
        for (int z = 0; z < zVerts - 1; z++)
        {
            for (int x = 0; x < xVerts - 1; x++)
            {
                int a = z * w + x;
                int b = a + w;
                int c = a + 1;
                int d = b + 1;

                tris[t++] = a; tris[t++] = b; tris[t++] = c;
                tris[t++] = c; tris[t++] = b; tris[t++] = d;
            }
        }

        mesh.Clear();
        mesh.indexFormat = (verts.Length > 65000) ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Optional collider
        var mc = GetComponent<MeshCollider>();
        if (mc)
        {
            mc.sharedMesh = null;
            mc.sharedMesh = mesh;
        }
    }

    float FractalPerlin(float x, float z)
    {
        float amp = 1f;
        float freq = 1f;
        float sum = 0f;
        float max = 0f;

        for (int o = 0; o < octaves; o++)
        {
            float n = Mathf.PerlinNoise(x * freq, z * freq) * 2f - 1f; // [-1,1]
            sum += n * amp;
            max += amp;
            amp *= persistence;
            freq *= lacunarity;
        }
        return sum / Mathf.Max(0.0001f, max); // normalize back to [-1,1]
    }

    float SampleMask(Vector2 uv)
    {
        if (!mask) return 0f;
        // Ensure mask is readable in import settings
        return mask.GetPixelBilinear(uv.x, uv.y).grayscale;
    }

    void EnsureMesh()
    {
        var mf = GetComponent<MeshFilter>();
        if (mesh == null)
        {
            mesh = new Mesh { name = "MaskedPerlinTerrain" };
            mf.sharedMesh = mesh;
        }
        else if (mf.sharedMesh != mesh)
        {
            mf.sharedMesh = mesh;
        }
    }

    void BuildArraysIfNeeded()
    {
        int vCount = xVerts * zVerts;
        int qCount = (xVerts - 1) * (zVerts - 1);

        if (verts == null || verts.Length != vCount)
        {
            verts = new Vector3[vCount];
            uvs = new Vector2[vCount];
        }
        if (tris == null || tris.Length != qCount * 6)
            tris = new int[qCount * 6];
    }

    [ContextMenu("Randomize Offset")]
    public void SetRandomNoiseOffset()
    {
        noiseOffset = new(Random.Range(-500, 500), Random.Range(-500, 500));
    }

    [ContextMenu("Reset Offset")]
    public void ResetNoiseOffsete()
    {
        noiseOffset = new(0, 0);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 size = new Vector3((xVerts - 1) * cellSize, 0.01f, (zVerts - 1) * cellSize);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(new Vector3(size.x * 0.5f, 0f, size.z * 0.5f), size);
    }
#endif
}
