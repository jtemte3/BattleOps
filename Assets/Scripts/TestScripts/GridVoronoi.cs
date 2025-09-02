using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public class GridVoronoi : MonoBehaviour
{
    public string saveFolderName = "Voronoi-Images";
    public int width = 100;
    public int height = 100;
    public int seedCount = 10;
    public int marginSize = 1;

    private Texture2D tex;
    private Color[] pixels;

    private List<Vector2Int> seedPositions;
    private Dictionary<Vector2Int, Color> seedColors;

    

    void Start()
    {
        Texture2D tex = GenerateVoronoi();

        GetComponent<Renderer>().material.mainTexture = tex;
        SaveTexture(tex);
    }

    Texture2D GenerateVoronoi()
    {
        tex = new Texture2D(width, height);
        pixels = new Color[width * height];
        seedPositions = new List<Vector2Int>();
        seedColors = new Dictionary<Vector2Int, Color>();

        // Place seeds
        for (int i = 0; i < seedCount; i++)
        {
            Vector2Int pos = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
            seedPositions.Add(pos);
            seedColors[pos] = new Color(Random.value, Random.value, Random.value);
        }

        // Assign regions
        int[,] regionMap = new int[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2Int current = new Vector2Int(x, y);
                float closestDist = float.MaxValue;
                int closestIndex = -1;

                for (int i = 0; i < seedPositions.Count; i++)
                {
                    float dist = Vector2Int.Distance(current, seedPositions[i]);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestIndex = i;
                    }
                }

                regionMap[x, y] = closestIndex;
            }
        }

        // Create margin mask — only check 4 neighbors
        Vector2Int[] dirs = {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool isMargin = false;
                int currentRegion = regionMap[x, y];

                foreach (var dir in dirs)
                {
                    int nx = x + dir.x;
                    int ny = y + dir.y;
                    if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                    {
                        if (regionMap[nx, ny] != currentRegion)
                        {
                            isMargin = true;
                            break;
                        }
                    }
                }

                if (isMargin)
                {
                    pixels[y * width + x] = Color.black;
                }
                else
                {
                    Vector2Int seedPos = seedPositions[currentRegion];
                    pixels[y * width + x] = seedColors[seedPos];
                }
            }
        }

        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels(pixels);
        tex.Apply();

        return tex;
    }

    private void SaveTexture(Texture2D texture)
    {
        byte[] bytes = texture.EncodeToPNG();
        string path = Path.Combine(Application.streamingAssetsPath, saveFolderName);
        //var dirPath = Application.dataPath + "/RenderOutput";
        if (!System.IO.Directory.Exists(path))
        {
            System.IO.Directory.CreateDirectory(path);
        }
        //System.IO.File.WriteAllBytes(path + "/R_" + Random.Range(0, 100000) + ".png", bytes);
        path += "/" + System.Guid.NewGuid().ToString() + ".png";
        System.IO.File.WriteAllBytes(path, bytes);
        Debug.Log(bytes.Length / 1024 + "Kb was saved as: " + path);
    }
}
