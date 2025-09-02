using UnityEditor;
using UnityEngine;

public class GridSnapEditor : EditorWindow
{
    //private static Transform cityParent;
    //private static float gridSize = 3f;

    [MenuItem("Tools/Grid Snapper")]
    public static void ShowWindow()
    {
        GetWindow<GridSnapEditor>("Grid Snapper");
    }

    private void OnGUI()
    {
        GUILayout.Label("Grid Snap Settings", EditorStyles.boldLabel);

        //gridSize = EditorGUILayout.FloatField("Grid Size", gridSize);
        //cityParent = (Transform)EditorGUILayout.ObjectField("City Parent", cityParent, typeof(Transform), true);

        if (GUILayout.Button("Snap Selected Objects"))
        {
            SnapSelectedObjects();
        }
    }

    private static void SnapSelectedObjects()
    {
        /*if (cityParent == null)
        {
            Debug.LogWarning("City Parent not assigned.");
            return;
        }*/

        foreach (GameObject obj in Selection.gameObjects)
        {
            if (obj.transform.parent != null)
            {
                Undo.RecordObject(obj.GetComponent<CityBlock>(), "Snap to Grid");
                EditorUtility.SetDirty(obj.transform);

                //obj.transform.parent = cityParent;
                // Calculate local position relative to city parent
                Vector3 localPos = obj.transform.position - obj.transform.parent.position;
                int gridSize = obj.transform.parent.GetComponent<CoroutineCityGen>().gridScale;

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
                obj.GetComponent<CityBlock>().gridPosition = new Vector2Int((int)gridPos.x, (int)gridPos.z);

                Debug.Log("Snapped " + Selection.gameObjects.Length + " object(s) to grid.");
            }
            else
            {
                Debug.LogWarning("Object needs to be a child of the city generator object.");
            }
            
        }
    }
}
