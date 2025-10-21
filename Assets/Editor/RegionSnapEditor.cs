using UnityEditor;
using UnityEngine;

public class RegionSnapEditor : EditorWindow
{
    [MenuItem("Tools/Region Grid Snapper")]
    public static void ShowWindow()
    {
        GetWindow<RegionSnapEditor>("Region Grid Snapper");
    }

    private void OnGUI()
    {
        GUILayout.Label("Region Grid Settings", EditorStyles.boldLabel);

        if (GUILayout.Button("Snap Selected Objects"))
        {
            SnapSelectedObjects();
        }
    }

    private static void SnapSelectedObjects()
    {

        foreach (GameObject obj in Selection.gameObjects)
        {
            if (obj.transform.parent != null)
            {
                Undo.RecordObject(obj.transform, "Snap to Grid");
                EditorUtility.SetDirty(obj.transform);

                Vector3 localPos = obj.transform.position - obj.transform.parent.position;
                int gridSize = 3;

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

                Debug.Log("Snapped " + Selection.gameObjects.Length + " object(s) to grid.");
            }
            else
            {
                Debug.LogWarning("Object needs to be a child of the city generator object.");
            }

        }
    }
}
