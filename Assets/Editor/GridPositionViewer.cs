using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class GridPositionViewer : EditorWindow
{
    public GameObject gridItem;

    [MenuItem("Tools/Grid Position Viewer")]
    public static void ShowWindow()
    {
        GetWindow<GridPositionViewer>("Grid Position Viewer");
    }
    private void OnGUI()
    {
        gridItem = (GameObject)EditorGUILayout.ObjectField("Grid Item", gridItem, typeof(GameObject), true);

        int gridSize = gridItem.GetComponent<CityBlock>().gridScale;
        Vector3 localPos = gridItem.transform.position - gridItem.transform.parent.position;

        Vector3 gridPos = localPos;

        gridPos.x = Mathf.Round(gridPos.x / gridSize);
        gridPos.y = Mathf.Round(gridPos.y / gridSize);
        gridPos.z = Mathf.Round(gridPos.z / gridSize);

        EditorGUILayout.Vector2IntField("Position:", new Vector2Int((int)gridPos.x, (int)gridPos.z));
        if (GUILayout.Button("Apply To Grid"))
        {
            Undo.RecordObject(gridItem.GetComponent<CityBlock>(), "Apply to Grid");
            gridItem.GetComponent<CityBlock>().gridPosition = new Vector2Int((int)gridPos.x, (int)gridPos.z);
        }
    }
}
