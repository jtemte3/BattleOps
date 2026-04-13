using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GridVoronoiCity cityParent;
    public MaskedPerlinTerrain terrainObject;
    public HelicopterSequence infilVehicle;
    public GameObject LoadingScreen;
    public AudioSource musicPlayer;

    public GameObject playerController;
    //public MissionManager missionManager;
    public bool landInCity = false;
    public bool reGenerateTerrain = true;

    public float timer;
    float startTime;
    float endTime;
    bool isTimerSet = false;

    private bool hasStarted = false;

    private void Start()
    {
        if (reGenerateTerrain)
        {
            terrainObject.SetRandomNoiseOffset();
            terrainObject.Generate();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (cityParent.loadingPercentage.Equals(1))
        {
            if (isTimerSet.Equals(false))
            {
                startTime = Time.time;
                endTime = startTime + timer;
                isTimerSet = true;

                List<GameObject> sequence = infilVehicle.RoutePositions[infilVehicle.routId].infilApproachRoute;

                GameObject endObj = sequence[sequence.Count - 1];
                GameObject hoverObj = sequence[sequence.Count - 2];

                if (landInCity)
                {
                    (Vector3 landingPos, Vector3 directionPos) = cityParent.GetLandingZoneRoadGen();

                    Vector3 hoverPos = landingPos;
                    hoverPos.y += 30.1f;

                    endObj.transform.position = landingPos;
                    hoverObj.transform.position = hoverPos;

                    endObj.transform.LookAt(directionPos);
                }
            }

            if (Time.time >= endTime)
            {
                if (hasStarted == false)
                {
                    hasStarted = true;
                    playerController.SetActive(true);
                    LoadingScreen.SetActive(false);
                    infilVehicle.beginFlying = true;
                    infilVehicle.StartAudio();
                    musicPlayer.Play();

                    /*if (missionManager)
                    {
                        missionManager.isMissionLoaded = true;
                    }*/
                }
            }
        }
    }
}
