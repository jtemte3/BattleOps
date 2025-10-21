using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using static UnityEngine.Rendering.HableCurve;

[ExecuteInEditMode]
public class RoadBuilder : MonoBehaviour
{
    [Header("Road Path Setup")]
    public List<RoadElement> roadPath;

    List<GameObject> roadObjects = new();

    [Header("Road Mesh Generator Settings")]
    public float roadWidth = 1f;
    public float roadThickness = 0.1f;
    public float uvScale = 0.1f;
    public float roadGroundOffset = 0.1f;
    public int curveResolution = 6; // points per Road Element
    public Material roadMaterial;
    public Material roadCapMaterial;

    [Header("Debug Tools")]
    public bool showGizmos = false;

    [ContextMenu("Generate Road Mesh")]
    public void GenerateRoadMesh()
    {

        if (roadObjects.Count > 0)
        {
            RemoveRoadMesh();
        }

        //List<Vector3> roadElements = (List<Vector3>)roadPath.Select(x => x.anchor.position);
        List<Vector3> roadElements = new();

        foreach (var roadElement in roadPath)
        {
            roadElements.Add(roadElement.anchor.position);
        }

        var curve = SimpleCurveBuilder.SimpleSplineFromTrasforms(roadElements, curveResolution);

        roadObjects.Add(RoadMeshBuilder.BuildRoadPathMesh(curve, roadWidth, roadThickness, uvScale, roadMaterial, this.transform));

        foreach(var roadElement in roadPath)
        {
            if (roadElement.isIntersection)
            {
                roadObjects.Add(RoadMeshBuilder.BuildRoadPathIntersections(roadElement.anchor.position, roadPath.IndexOf(roadElement), roadWidth, roadThickness, uvScale, roadMaterial, this.transform));
            }
        }
    }

    public void RemoveRoadMesh()
    {
        foreach(var obj in roadObjects)
        {
            DestroyImmediate(obj);
        }
    }

    private void OnDrawGizmos()
    {
        if (showGizmos)
        {
            if (roadPath.Count > 0)
            {
                //List<Vector3> roadElements = (List<Vector3>)roadPath.Select(x => x.anchor.position);
                List<Vector3> roadElements = new();

                foreach (var roadElement in roadPath)
                {
                    roadElements.Add(roadElement.anchor.position);
                }

                List<Vector3> curve = SimpleCurveBuilder.SimpleSplineFromTrasforms(roadElements, curveResolution);

                if (roadElements.Count > 0)
                {
                    for (int i = 0; i < curve.Count; i++)
                    {
                        if (i != curve.Count - 1)
                        {
                            Gizmos.color = Color.grey;
                            int nextIndex = i + 1;
                            Vector3 pos = new Vector3(curve[i].x, curve[i].y, curve[i].z);
                            Vector3 nextPos = new Vector3(curve[nextIndex].x, curve[nextIndex].y, curve[nextIndex].z);

                            Gizmos.DrawLine(pos, nextPos);
                        }
                    }
                }
                foreach (var roadElement in roadPath)
                {
                    if (roadElement.isIntersection)
                    {
                        Gizmos.color = Color.grey;
                        Gizmos.DrawSphere(roadElement.anchor.position, 0.5f);
                    }
                }
            }
        }
    }
}
[System.Serializable]
public class RoadElement
{
    public Transform anchor;
    public bool isIntersection;
}