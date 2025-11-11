using UnityEditor;
using UnityEngine;

public class Event_DestroyItem : MissionEvent
{
    public override void Engage()
    {
        compassPoint.UpdatePointPosition(transform.position);
    }

#if (UNITY_EDITOR)
    void OnDrawGizmos()
    {
        Handles.color = Color.red;
        Handles.DrawWireDisc(transform.position, new Vector3(0, 1, 0), 1.0f);
        Handles.color = Color.white;
        Handles.DrawLine(transform.position, transform.position + (Vector3.up * 5));
    }
#endif
}
