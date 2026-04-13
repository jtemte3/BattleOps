using UnityEngine;

public class Event_DismountHelicopter : MissionEvent
{
    public HelicopterSequence helicopterSequence;
    public override void Engage()
    {
        if (helicopterSequence.stage == HelicopterSequence.FlightStage.Descent)
        {
            TriggerCompletion();
        }
    }
}
