using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Neighbors
{
    public Vector2Int position;
    public List<Vector2Int> neighborPos;
    public List<List<Vector2Int>> neighborPaths;

    public Neighbors()
    {
        position = new Vector2Int();
        neighborPos = new List<Vector2Int>();
        neighborPaths = new List<List<Vector2Int>>();
    }
}
