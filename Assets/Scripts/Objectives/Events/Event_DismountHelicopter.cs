using UnityEngine;
using UnityEngine.Events;

public class Event_DismountHelicopter : MissionEvent
{
    public HelicopterSequence helicopterSequence;

    [Tooltip("Events to trigger on event completion")]
    public UnityEvent OnIntro;
    public UnityEvent onIntro => OnIntro;
    public override void Engage()
    {
        if (helicopterSequence.stage == HelicopterSequence.FlightStage.Descent)
        {
            OnIntro.Invoke();
        }
        if (helicopterSequence.interactor.isPlayerInVehicle == false)
        {
            TriggerCompletion();
        }
    }
}
