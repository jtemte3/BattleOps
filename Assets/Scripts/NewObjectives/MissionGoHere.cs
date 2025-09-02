using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MissionGoHere : MissionEvent
{
    public JetpackPlayerController player;
    public bool firstPass = true;
    public float completionDistance;

    void Awake()
    {
        MissionManager manager = FindAnyObjectByType<MissionManager>();

        if (objectiveType.Equals(ObjectiveType.Main))
        {
            manager.mainEvent = this;
        }
        if (objectiveType.Equals(ObjectiveType.Prerequisite))
        {
            manager.prerequisiteEvents.Add(this);
        }
        if (objectiveType.Equals(ObjectiveType.Extraction))
        {
            manager.Extraction = this;
        }
    }

    // Update is called once per frame
    public override void Engage()
    {
        if (player == null)
        {
            player = FindObjectOfType<JetpackPlayerController>();
        }
        else
        {
            float distance = Vector3.Distance(player.transform.position, transform.position);
            if (distance <= completionDistance)
            {
                isObjCompleted = true;
                compassPoint.SetPointActive(false);
                isObjActive = false;
            }
        }
    }
}
