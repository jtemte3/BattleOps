using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TargetAdjustor))]
public class TargetAdjustorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TargetAdjustor adjustor = (TargetAdjustor)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Shoot Once", GUILayout.Width(200)))
        {
            adjustor.ShootOnce();
        }
    }
}
