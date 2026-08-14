using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelManager : MonoBehaviour
{
    public MissionSetupHandler missionSetupHandler;
    public bool hasLoaded = true;
    public float loadingPercentage = 100f;

    public float loadScreenHoldTimer = 3f;
    float startTime;
    float endTime;
    bool isTimerSet = false;
    public HelicopterSequence infilVehicle;


    [Tooltip("Events to trigger on event completion")]
    public UnityEvent OnStart;
    public UnityEvent onStart => OnStart;

    [Tooltip("Events to trigger on event completion")]
    public UnityEvent OnLoadFinish;
    public UnityEvent onLoadFinish => OnLoadFinish;

    

    private bool hasStarted = false;

    private void Start()
    {

        // Trigger mission setup after terrain is ready
        if (missionSetupHandler != null)
        {
            missionSetupHandler.SetupMission();
        }

        onStart.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        if (hasLoaded && isTimerSet.Equals(false))
        {
            startTime = Time.time;
            endTime = startTime + loadScreenHoldTimer;
            isTimerSet = true;

            List<GameObject> sequence = infilVehicle.RoutePositions[infilVehicle.routId].infilApproachRoute;

            GameObject endObj = sequence[sequence.Count - 1];
            GameObject hoverObj = sequence[sequence.Count - 2];
        }

        if (isTimerSet && Time.time >= endTime)
        {
            if (hasStarted == false)
            {
                hasStarted = true;

                OnLoadFinish.Invoke();
            }
        }
    }
}
