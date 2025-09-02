using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CityBlock))]
public class CityBlockEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CityBlock block = (CityBlock)target;
        base.OnInspectorGUI();

        if (GUILayout.Button("Snap To Grid"))
        {
            SnapSelectedObjects(block);
        }
    }

    private static void SnapSelectedObjects(CityBlock block)
    {
        if (block.transform.parent == null)
        {
            Debug.LogWarning("City Parent not assigned.");
            return;
        }

        foreach (GameObject obj in Selection.gameObjects)
        {
            Undo.RecordObject(obj.GetComponent<CityBlock>(), "Snap to Grid");
            EditorUtility.SetDirty(obj.transform);

            // Calculate local position relative to city parent
            Vector3 localPos = obj.transform.position - block.transform.parent.position;

            Vector3 gridPos = localPos;

            gridPos.x = Mathf.Round(gridPos.x / block.gridScale);
            gridPos.y = Mathf.Round(gridPos.y / block.gridScale);
            gridPos.z = Mathf.Round(gridPos.z / block.gridScale);
            // Snap to nearest grid unit
            localPos.x = Mathf.Round(localPos.x / block.gridScale) * block.gridScale;
            localPos.y = Mathf.Round(localPos.y / block.gridScale) * block.gridScale;
            localPos.z = Mathf.Round(localPos.z / block.gridScale) * block.gridScale;

            // Reapply global position
            obj.transform.position = block.transform.parent.position + localPos;
            obj.GetComponent<CityBlock>().gridPosition = new Vector2Int((int)gridPos.x, (int)gridPos.z);
        }

        Debug.Log("Snapped " + Selection.gameObjects.Length + " object(s) to grid.");
    }
}
