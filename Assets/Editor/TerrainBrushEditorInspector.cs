using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainBrushEditor))]
public class TerrainBrushEditorInspector : Editor
{
    bool painting = false;
    double lastTime;

    void OnSceneGUI()
    {
        var brushEditor = (TerrainBrushEditor)target;
        Event e = Event.current;

        // Scroll adjustments
        // Handle scroll wheel adjustments
        if (e.type == EventType.ScrollWheel)
        {
            float delta = e.delta.y * 0.1f; // adjust sensitivity as needed

            if (e.alt)
            {
                // Adjust brush radius
                brushEditor.brushRadius = Mathf.Max(0.1f, brushEditor.brushRadius - delta);
                e.Use();
            }
            else if (e.control)
            {
                // Adjust brush strength
                brushEditor.brushStrength = Mathf.Max(0.01f, brushEditor.brushStrength - delta);
                e.Use();
            }
        }

        // Determine brush color
        Color discColor = Color.white;
        switch (brushEditor.brushMode)
        {
            case TerrainBrushEditor.BrushMode.RaiseLower:
                discColor = e.shift ? Color.red : Color.blue;  // Shift flips raise/lower
                break;
            case TerrainBrushEditor.BrushMode.SetHeight:
                discColor = Color.yellow;
                break;
            case TerrainBrushEditor.BrushMode.Smooth:
                discColor = Color.green;
                break;
            default:
                discColor = Color.white;
                break;
        }

        if (brushEditor.brushMode != TerrainBrushEditor.BrushMode.None)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 1000f, brushEditor.terrainMask)) return;

            // Ctrl + Shift = Sample height in SetHeight mode
            if (brushEditor.brushMode == TerrainBrushEditor.BrushMode.SetHeight &&
                e.type == EventType.MouseDown && e.button == 0 && e.control)
            {
                brushEditor.SampleTargetHeight(hit.point);
                e.Use();
            }

            Handles.color = discColor;
            Handles.DrawWireDisc(hit.point, hit.normal, brushEditor.brushRadius);

            double now = EditorApplication.timeSinceStartup;
            float deltaTime = painting ? (float)(now - lastTime) : 0f;
            lastTime = now;

            // Determine if Raise/Lower direction should be inverted by Shift
            bool invertRaise = brushEditor.brushMode == TerrainBrushEditor.BrushMode.RaiseLower && e.shift;

            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt && !(e.control && e.shift))
            {
                painting = true;
                e.Use();
            }
            if (painting && (e.type == EventType.MouseDrag || e.type == EventType.Repaint))
            {
                // Temporarily flip strength for Raise/Lower if shift pressed
                float originalStrength = brushEditor.brushStrength;
                if (invertRaise) brushEditor.brushStrength *= -1f;

                brushEditor.ApplyBrush(hit.point, deltaTime);

                if (invertRaise) brushEditor.brushStrength = originalStrength; // restore
                e.Use();
            }
            if (e.type == EventType.MouseUp && e.button == 0)
            {
                painting = false; e.Use();
            }
        }

        // Overlay
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(10, 10, 300, 120), "Brush Info", "Window");

        
        if (brushEditor.brushMode == TerrainBrushEditor.BrushMode.RaiseLower && !e.shift)
        {
            GUILayout.Label($"<color=white><b>Mode:</b></color> <color=yellow>Raise/Lower</color>     <color=grey>(toggle with shift)</color>", new GUIStyle() { richText = true });
            //GUILayout.Label("<color=grey>(toggle with shift)</color>", new GUIStyle() { richText = true });
        }
        else if (brushEditor.brushMode == TerrainBrushEditor.BrushMode.RaiseLower && e.shift)
        {
            GUILayout.Label($"<color=white><b>Mode:</b></color> <color=yellow>Raise/Lower</color>     <color=grey>(toggle with shift)</color>", new GUIStyle() { richText = true });
        }
        else if (brushEditor.brushMode == TerrainBrushEditor.BrushMode.SetHeight)
        {
            GUILayout.Label($"<color=white><b>Mode:</b></color> <color=yellow>{brushEditor.brushMode}</color>     <color=grey>(set height with ctrl + left click)</color>", new GUIStyle() { richText = true });
        }
        else if (brushEditor.brushMode == TerrainBrushEditor.BrushMode.Smooth)
        {
            GUILayout.Label($"<color=white><b>Mode:</b></color> <color=yellow>{brushEditor.brushMode}</color>", new GUIStyle() { richText = true });
        }
        else
        {
            GUILayout.Label($"<color=white><b>Mode:</b></color> <color=yellow>{brushEditor.brushMode}</color>", new GUIStyle() { richText = true });
        }
        GUILayout.Label($"<color=white>Radius: {brushEditor.brushRadius:F2}</color>     <color=grey>(alt + scroll)</color>", new GUIStyle() { richText = true });
        GUILayout.Label($"<color=white>Strength: {brushEditor.brushStrength:F2}</color>     <color=grey>(ctrl + scroll)</color>", new GUIStyle() { richText = true });
        GUILayout.Label($"<color=white>Target Height: {brushEditor.targetHeight:F2}</color>", new GUIStyle() { richText = true });
        GUILayout.EndArea();
        Handles.EndGUI();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var brushEditor = (TerrainBrushEditor)target;

        GUILayout.Space(10);
        GUILayout.Label("Brush Modes", EditorStyles.boldLabel);
        if (GUILayout.Button("None")) brushEditor.brushMode = TerrainBrushEditor.BrushMode.None;
        if (GUILayout.Button("Raise/Lower")) brushEditor.brushMode = TerrainBrushEditor.BrushMode.RaiseLower;
        if (GUILayout.Button("Set Height")) brushEditor.brushMode = TerrainBrushEditor.BrushMode.SetHeight;
        if (GUILayout.Button("Smooth")) brushEditor.brushMode = TerrainBrushEditor.BrushMode.Smooth;

        GUILayout.Space(10);
        GUILayout.Label("Heightmap Save/Load", EditorStyles.boldLabel);

        if (GUILayout.Button("Save Heightmap PNG"))
        {
            string path = EditorUtility.SaveFilePanel("Save Terrain Heightmap", "", "terrain_heightmap.png", "png");
            if (!string.IsNullOrEmpty(path))
            {
                brushEditor.SaveHeightMap(path);
                EditorUtility.DisplayDialog("Save Complete", "Heightmap PNG and JSON saved successfully.", "OK");
            }
        }

        if (GUILayout.Button("Load Heightmap PNG"))
        {
            string path = EditorUtility.OpenFilePanel("Load Terrain Heightmap", "", "png");
            if (!string.IsNullOrEmpty(path))
            {
                brushEditor.LoadHeightMap(path, true);
                EditorUtility.DisplayDialog("Load Complete", "Heightmap PNG loaded and vertex heights restored.", "OK");
            }
        }
    }
}
