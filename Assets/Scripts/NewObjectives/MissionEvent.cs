using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public abstract class MissionEvent : MonoBehaviour
{
    public ObjectiveType objectiveType;
    public CompassPoint compassPoint;

    [Header("Objective Settings")]
    public string objectiveShortDescription;
    [Multiline]
    public string objectiveDescription;
    public bool isObjActive;
    public bool isObjCompleted;

    public abstract void Engage();

#if (UNITY_EDITOR)
    void OnDrawGizmos()
    {
        Handles.DrawWireDisc(transform.position, new Vector3(0, 1, 0), 1.0f);
        Handles.DrawLine(transform.position, transform.position + (Vector3.up * 5));
    }
#endif
}
