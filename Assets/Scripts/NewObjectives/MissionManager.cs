using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public CompassManager compassManager;
    public MissionEvent mainEvent;
    public List<MissionEvent> prerequisiteEvents;
    public int completedPrerequisiteEvents;
    public MissionEvent Extraction;
    public bool isMissionComplete = false;
    public bool isMissionLoaded = false;
    bool initialPass;

    void Update()
    {
        if (isMissionLoaded)
        {
            if (initialPass == false)
            {
                mainEvent.compassPoint.SetPointActive(false);
                mainEvent.compassPoint.UpdatePointPosition(mainEvent.transform.position);

                foreach (MissionEvent eventObj in prerequisiteEvents)
                {
                    eventObj.compassPoint.SetPointActive(true);
                    eventObj.compassPoint.UpdatePointPosition(eventObj.transform.position);
                }

                Extraction.compassPoint.SetPointActive(false);
                Extraction.compassPoint.UpdatePointPosition(Extraction.transform.position);

                initialPass = true;
            }

            completedPrerequisiteEvents = prerequisiteEvents.Count(x => x.isObjCompleted);

            if (prerequisiteEvents.Count > 0 && completedPrerequisiteEvents < prerequisiteEvents.Count)
            {
                if (mainEvent.isObjActive)
                {
                    mainEvent.isObjActive = false;
                    mainEvent.compassPoint.SetPointActive(false);
                }

                foreach (MissionEvent eventObj in prerequisiteEvents)
                {
                    if (eventObj.isObjCompleted == false)
                    {
                        eventObj.Engage();
                        eventObj.compassPoint.SetPointActive(true);
                    }
                }
            }

            if (completedPrerequisiteEvents == prerequisiteEvents.Count && mainEvent.isObjCompleted == false)
            {
                mainEvent.isObjActive = true;
                mainEvent.compassPoint.SetPointActive(true);
                mainEvent.Engage();
            }

            if (completedPrerequisiteEvents == prerequisiteEvents.Count && mainEvent.isObjCompleted == true && Extraction.isObjCompleted == false)
            {
                Extraction.isObjActive = true;
                Extraction.compassPoint.SetPointActive(true);
                Extraction.Engage();
            }

            if (completedPrerequisiteEvents == prerequisiteEvents.Count && mainEvent.isObjCompleted == true && Extraction.isObjCompleted == true)
            {
                isMissionComplete = true;
            }
        }
    }
}
