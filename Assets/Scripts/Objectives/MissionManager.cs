using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MissionManager : MonoBehaviour
{
    public List<MissionEvent> missionEvents;

    public bool isMissionLoaded = false;
    public bool isMissionComplete = false;

    [Tooltip("Events to trigger on mission completion")]
    public UnityEvent OnCompletion;
    public UnityEvent onCompletion => OnCompletion;

    public TMP_Text objTextLine;

    public void Update()
    {
        if (isMissionLoaded)
        {
            if (!isMissionComplete)
            {
                foreach (MissionEvent missionEvent in missionEvents)
                {
                    if (missionEvent.isObjActive && !missionEvent.isObjCompleted)
                    {
                        missionEvent.Engage();

                        if (!string.IsNullOrEmpty(missionEvent.objectiveShortDescription))
                        {
                            objTextLine.text = missionEvent.objectiveShortDescription;
                        }
                    }
                }
            }
        }
    }

    public void SetMissionCompleted()
    {
        isMissionComplete = true;
    }

    public void SetMissionLoaded()
    {
        isMissionLoaded = true;
    }

    public void TriggerOnCompletion()
    {
        OnCompletion.Invoke();
    }
}
