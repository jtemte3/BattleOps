using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ObjWaypoint : ObjectiveEvent
{
    [Header("Waypoint Settings")]
    public CompassPoint compassPoint;
    public float completionDistance;
    private GameObject playerObject;
    private float distance;

    private void Awake()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player");
        if (isObjActive == false)
        {
            compassPoint.SetPointActive(false);
        }
    }
    public override void Engage()
    {
        distance = Vector3.Distance(playerObject.transform.position, transform.position);
        if (distance <= completionDistance)
        {
            isObjCompleted = true;
            DeactivateObjective();

            if (onCompletion != null)
            {
                onCompletion.Invoke();
            }
        }
    }

    public override void ActivateObjective()
    {
        isObjActive = true;
        compassPoint.SetPointActive(true);
    }

    public override void DeactivateObjective()
    {
        isObjActive = false;
        compassPoint.SetPointActive(false);
    }

#if (UNITY_EDITOR)
    void OnDrawGizmos()
    {
        Handles.DrawWireDisc(transform.position, new Vector3(0, 1, 0), completionDistance);
        Handles.DrawLine(transform.position, transform.position + (Vector3.up * 5));
    }
#endif
}
