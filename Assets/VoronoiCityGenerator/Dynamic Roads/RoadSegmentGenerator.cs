using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadSegmentGenerator : MonoBehaviour
{
    [Header("Road Settings")]
    public List<Vector2Int> roadPositions = new List<Vector2Int>();
    public List<Vector2Int> intersections = new List<Vector2Int>();
    public List<Neighbors> intersectionMap = new List<Neighbors>();
    private List<List<Vector2Int>> segments = new();
    private List<List<Vector2Int>> invalidPaths = new();
    private List<Vector2Int> directions = new List<Vector2Int>
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

    [Header("Road Mesh Generator Settings")]
    public float roadWidth = 1f;
    public float roadThickness = 0.1f;
    public float roadGroundOffset = 0.1f;
    public float uvScale = 0.1f;
    public float gridScale = 3f;
    public int curveResolution = 6; // points per segment (segment being the first, middle and last point to make the curve)
    public Material roadMaterial;
    public Material roadCapMaterial;

    [Header("Debug Tools")]
    public bool drawGizmos = true;
    public bool drawInvalidPaths = true;
    public bool drawIntersectionConnections = true;
    public bool drawSingleIntersectionConnections = true;
    public Vector2Int testIntersection = new Vector2Int();
    public bool makeRoadMeshes = true;
    private List<Color> segmentColors = new();
    private List<Color> intersectionColors = new();

    // --------------------------
    // Generate all road segments
    // --------------------------
    public void GenerateRoadSegments(List<Vector2Int> intersectionPositions)
    {
        // --------------------------
        // Build an intersection neighbor map from each road position, searching in diagonal + cardinal directions
        // --------------------------
        intersectionMap = IntersectionUtils.FindNeighbors(intersectionPositions, roadPositions, directions);

        //Return a list of intersections identifies above
        intersections = IntersectionUtils.GetIntersectionList(intersectionMap);

        //Identify potential neigbors, and create a path to them using the road positions
        IntersectionUtils.GenerateIntersectionPaths(intersectionMap, intersectionPositions, roadPositions);

        //Go through each intersection to neighbor path and search to see if it passes another intersection, making it invalid.
        IntersectionUtils.ValidateNeighborMap(intersectionMap, intersections, searchDepth : 2, invalidPaths, directions);

        //Currently there are 2 road segments for each intersection and connected neighbor (goint to, and coming from). We only need one of them.
        RemoveDuplicateSegments();

        //For debugging purposes, assign each road segment a color
        for (int i = 0; i < segments.Count; i++)
        {
            float r = Random.Range(0.0f, 1.0f);
            float g = Random.Range(0.0f, 1.0f);
            float b = Random.Range(0.0f, 1.0f);
            Color segmentColor = new(r, g, b, 1);
            segmentColors.Add(segmentColor);
        }

        //For debugging purposes, assign each intersection group a color
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
            //Make a new mesh for each road segment
            foreach (var seg in segments)
            {
                //We don't want to make a mesh following every road position in the road segment, we just want to use 3 points: beginning, middle, end
                //Using those three points, we will make a curve and build our mesh following that path
                var curve = SimpleCurveBuilder.SampleThreePointArc(seg, gridScale, curveResolution);

                //Using the curve and the new path of nodes, generate a mesh for the path
                RoadMeshBuilder.BuildRoadMesh(curve, segments.IndexOf(seg), roadGroundOffset, roadWidth, roadThickness, uvScale, roadMaterial, this.transform);
                //RoadMeshBuilder.BuildRoadMesh2D(spline, segments.IndexOf(seg), roadGroundOffset, roadWidth, roadMaterial, this.transform);
            }
            //Make a new thin cylinder "plug" mesh for each intersection
            foreach (var intersection in intersections)
            {
                Vector3 pos = new(intersection.x * gridScale, 0, intersection.y * gridScale);
                RoadMeshBuilder.BuildIntersectionCylinder(pos, intersections.IndexOf(intersection), roadGroundOffset, roadWidth, roadThickness, uvScale, roadMaterial, this.transform);
            }
        }
    }

    private void RemoveDuplicateSegments()
    {
        foreach (var neighbor in intersectionMap)
        {
            foreach (var path in neighbor.neighborPaths)
            {
                Vector2Int startPos = path[0];
                Vector2Int endPos = path[path.Count - 1];
                bool isDup = false;
                foreach (var seg in segments)
                {
                    if (seg[0] == endPos && seg[seg.Count - 1] == startPos)
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
    }

#if (UNITY_EDITOR)
    public void OnDrawGizmos()
    {
        if (drawGizmos)
        {
            //Draw each road segment in the editor
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
                            Vector3 pos = new Vector3(segment[i].x * gridScale, 1, segment[i].y * gridScale) + transform.position;
                            Vector3 nextPos = new Vector3(segment[nextIndex].x * gridScale, 1, segment[nextIndex].y * gridScale) + transform.position;

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

            //Draw each invalid road segment in the editor
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
                            Vector3 pos = new Vector3(path[i].x * gridScale, 1, path[i].y * gridScale) + transform.position;
                            Vector3 nextPos = new Vector3(path[nextIndex].x * gridScale, 1, path[nextIndex].y * gridScale) + transform.position;

                            Gizmos.DrawLine(pos, nextPos);

                        }
                    }
                }
            }

            //Draw each invalid intersection in the editor
            if (intersections.Count > 0)
            {
                Gizmos.color = new Color(0, 1, 0, .25f);
                foreach (Vector2Int intersection in intersections)
                {
                    Vector3 pos = new Vector3(intersection.x * gridScale, 1, intersection.y * gridScale) + transform.position;
                    Gizmos.DrawSphere(pos, 1f);
                }
            }

            //Draw each intersection in the editor with its neighbor connections
            if (intersectionMap.Count > 0 && drawIntersectionConnections)
            {
                //Draw a specific intersection in the editor with its neighbor connections
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

                        Vector3 pos = new Vector3(intersection.position.x * gridScale, 1, intersection.position.y * gridScale) + transform.position;
                        Gizmos.DrawSphere(pos, 1f);

                        foreach (Vector2Int neightborPosition in intersection.neighborPos)
                        {
                            Vector3 neighPos = new Vector3(neightborPosition.x * gridScale, 1, neightborPosition.y * gridScale) + transform.position;
                            Gizmos.DrawLine(pos, neighPos);
                        }

                    }
                }
                else
                {
                    foreach (Neighbors intersection in intersectionMap)
                    {
                        Gizmos.color = intersectionColors[intersectionMap.IndexOf(intersection)];

                        Vector3 pos = new Vector3(intersection.position.x * gridScale, 1, intersection.position.y * gridScale) + transform.position;
                        Gizmos.DrawSphere(pos, 1f);

                        foreach (Vector2Int neightborPosition in intersection.neighborPos)
                        {
                            Vector3 neighPos = new Vector3(neightborPosition.x * gridScale, 1, neightborPosition.y * gridScale) + transform.position;
                            Gizmos.DrawLine(pos, neighPos);
                        }

                    }
                }
            }
        }
    }
#endif
}
