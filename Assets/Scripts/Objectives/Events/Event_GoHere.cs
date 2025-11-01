using UnityEditor;
using UnityEngine;

public class Event_GoHere : MissionEvent
{
    public JetpackPlayerController player;
    public float completionDistance;

    // Update is called once per frame
    public override void Engage()
    {
        if (player == null)
        {
            player = FindObjectOfType<JetpackPlayerController>();
        }
        else
        {
            compassPoint.UpdatePointPosition(transform.position);

            float distance = Vector3.Distance(player.transform.position, transform.position);

            if (distance <= completionDistance)
            {
                TriggerCompletion();
            }
        }
    }

#if (UNITY_EDITOR)
    void OnDrawGizmos()
    {
        Handles.DrawWireDisc(transform.position, new Vector3(0, 1, 0), 1.0f);
        Handles.DrawLine(transform.position, transform.position + (Vector3.up * 5));
    }
#endif
}
