using UnityEngine;

public class Event_MountHelicopter : MissionEvent
{
    public HelicopterSequence helicopterSequence;
    public override void Engage()
    {
        if (helicopterSequence.beginTakeoff)
        {
            TriggerCompletion();
        }
    }
}
