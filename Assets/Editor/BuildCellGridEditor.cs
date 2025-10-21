using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(CellGridManager))]
public class BuildCellGridEditor : Editor
{
    public override void OnInspectorGUI()
    {
        
        DrawDefaultInspector();

        CellGridManager grid = (CellGridManager)target;

        GUILayout.Space(10);
        
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        GUILayout.Label("Procedural Placement Tools", GUILayout.Width(200));
        if (GUILayout.Button("Generate Grid", GUILayout.Width(200)))
        {
            grid.GenerateGrid();
            EditorUtility.SetDirty(grid); // mark dirty so it saves
        }

        if (GUILayout.Button("Generate Buildings", GUILayout.Width(200)))
        {
            grid.CreateBuildings();
            EditorUtility.SetDirty(grid); // mark dirty so it saves
        }

        if (GUILayout.Button("Remove Generated Buildings", GUILayout.Width(200)))
        {
            grid.RemoveGeneratedBuildings();
            EditorUtility.SetDirty(grid); // mark dirty so it saves
        }
        GUILayout.EndVertical();

        
        GUILayout.BeginVertical();
        GUILayout.Label("Manual Placement Tools", GUILayout.Width(200));

        if (GUILayout.Button("Snap Selected Objects", GUILayout.Width(200)))
        {
            SnapSelectedObjects(grid);
        }

        if (GUILayout.Button("Remove Selected Objects", GUILayout.Width(200)))
        {
            RemoveSelectedObjects(grid);
        }

        if (GUILayout.Button("Update Tiles", GUILayout.Width(200)))
        {
            grid.UpdateGridPositions();
            EditorUtility.SetDirty(grid); // mark dirty so it saves
        }

        if (GUILayout.Button("Clear Grid", GUILayout.Width(200)))
        {
            grid.ClearGridPositions();
            EditorUtility.SetDirty(grid); // mark dirty so it saves
        }

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Label("Debug Tools");
        if (GUILayout.Button(grid.debugButtonText, GUILayout.Width(200)))
        {
            if (grid.showGizmos == false)
            {
                grid.debugButtonText = "Hide Gizmos";
                grid.showGizmos = true;
            }
            else
            {
                grid.debugButtonText = "Show Gizmos";
                grid.showGizmos = false;
            }
        }
    }

    private static void SnapSelectedObjects(CellGridManager grid)
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            if (obj.transform.parent != null)
            {
                Undo.RecordObject(obj.transform, "Snap to Grid");
                EditorUtility.SetDirty(obj.transform);

                Vector3 localPos = obj.transform.position - grid.transform.position;
                int gridSize = grid.cellSize;

                Vector3 gridPos = localPos;

                gridPos.x = Mathf.Round(gridPos.x / gridSize);
                gridPos.y = Mathf.Round(gridPos.y / gridSize);
                gridPos.z = Mathf.Round(gridPos.z / gridSize);
                // Snap to nearest grid unit
                localPos.x = Mathf.Round(localPos.x / gridSize) * gridSize;
                localPos.y = Mathf.Round(localPos.y / gridSize) * gridSize;
                localPos.z = Mathf.Round(localPos.z / gridSize) * gridSize;

                // Reapply global position
                obj.transform.position = obj.transform.parent.position + localPos;
                //obj.GetComponent<CityBlock>().gridPosition = new Vector2Int((int)gridPos.x, (int)gridPos.z);

                if (obj.GetComponent<GridBuilding>() != null)
                {
                    GridBuilding building = obj.GetComponent<GridBuilding>();
                    building.gridPosition = new Vector2Int((int)gridPos.x, (int)gridPos.z);

                    if (!grid.buildings.Contains(building))
                    {
                        grid.buildings.Add(building);
                    }
                }
                

                Debug.Log("Snapped " + Selection.gameObjects.Length + " object(s) to grid.");
            }
            else
            {
                Debug.LogWarning("Object needs to be a child of the city generator object.");
            }

            grid.ClearGridPositions();
            grid.UpdateGridPositions();
        }
    }

    private static void RemoveSelectedObjects(CellGridManager grid)
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            if (obj.GetComponent<GridBuilding>())
            {
                Undo.RecordObject(obj.transform, "Removed Building");
                EditorUtility.SetDirty(obj.transform);

                GridBuilding building = obj.GetComponent<GridBuilding>();

                if (grid.buildings.Contains(building))
                {
                    grid.buildings.Remove(building);
                    DestroyImmediate(obj);
                }

                Debug.Log("Removed " + Selection.gameObjects.Length + " object(s) to grid.");
            }
            else
            {
                Debug.LogWarning("Object needs to be GridBuilding object to be removed");
            }
        }

        grid.ClearGridPositions();
        grid.UpdateGridPositions();
    }
}
