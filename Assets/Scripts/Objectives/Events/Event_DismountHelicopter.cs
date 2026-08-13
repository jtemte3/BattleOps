using UnityEngine;

public class Event_DismountHelicopter : MissionEvent
{
    public HelicopterSequence helicopterSequence;
    public CinematicController cinemaController;
    public override void Engage()
    {
        if (helicopterSequence.stage == HelicopterSequence.FlightStage.Descent)
        {
            cinemaController.SetFadeState(false);
        }
        if (helicopterSequence.interactor.isPlayerInVehicle == false)
        {
            TriggerCompletion();
        }
    }
}
