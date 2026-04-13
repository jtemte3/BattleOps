using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HelicopterSequence : MonoBehaviour
{
    public GameObject helicopter;
    public TiltOnTurn tiltUtil;
    public HeliPlayerInteractor interactor;
    public AudioSource helicopterAudio;
    public int routId;
    public List<HeliRoute> RoutePositions = new();

    public VehicleType vehicleType;

    public float speed;
    public float flyingSpeed;
    public float approachSpeed;
    public float decentSpeed;
    public float exitSpeed;
    public float decentHeight;
    public float rotationalSpeed;
    public float nodeRange;
    public int nextNode = 1;
    public GameObject nextNodeObject;

    public bool beginFlying;
    public bool beginTakeoff = false;
    public float takeOffTimer = 3.0f;
    private float timeToTakeOff = 0.0f;
    private float currentAccelerationRate = 0.0f;
    public enum FlightStage { Approach, Descent, Holding, Ascending, Exit}
    public FlightStage stage = FlightStage.Approach;
    //private Vector3 someVelocityThing = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        //beginFlying = false;
        routId = Random.Range(0, RoutePositions.Count);

        GameObject startingPos = RoutePositions[routId].infilApproachRoute[0];

        helicopter.transform.position = startingPos.transform.position;
        helicopter.transform.LookAt(RoutePositions[routId].infilApproachRoute[1].transform);
        //helicopter.transform.Rotate(startingPos.transform.rotation.eulerAngles);
        speed = flyingSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (beginFlying == true)
        {
            switch (stage)
            {
                case FlightStage.Approach:
                    OnApproach();
                    tiltUtil.ApplyTilt(flyingSpeed, nextNodeObject.transform.position);
                    break;
                case FlightStage.Descent:
                    OnDecent();
                    //tiltUtil.ApplyTilt(decentSpeed, nextNodeObject.transform.position);
                    break;
                case FlightStage.Holding:
                    OnHold();
                    break;
                case FlightStage.Ascending:
                    OnAscent();
                    break;
                case FlightStage.Exit:
                    OnExit();
                    tiltUtil.ApplyTilt(flyingSpeed, nextNodeObject.transform.position);
                    break;
            }
        }
    }

    void OnApproach()
    {
        nextNodeObject = RoutePositions[routId].infilApproachRoute[nextNode];

        float dist = Vector3.Distance(helicopter.transform.position, nextNodeObject.transform.position);
        

        if (nextNodeObject.Equals(RoutePositions[routId].beginDecentNode))
        {
            speed = approachSpeed;
            if (dist > .25f)
            {
                Move(nextNodeObject.transform.position);
            }
            if (dist <= .25f)
            {
                stage = FlightStage.Descent;
                speed = decentSpeed;
                nextNode++;
            }
        }
        else
        {
            speed = flyingSpeed;
            if (dist > nodeRange)
            {
                Move(nextNodeObject.transform.position);
            }
            if (dist <= nodeRange)
            {
                nextNode++;
            }
        }
        
    }

    void OnDecent()
    {
        nextNodeObject = RoutePositions[routId].infilApproachRoute[nextNode];

        float dist = Vector3.Distance(helicopter.transform.position, nextNodeObject.transform.position);
        if (dist > decentHeight)
        {
            Decend(nextNodeObject.transform.rotation);
        }

        if(dist <= decentHeight)
        {
            stage = FlightStage.Holding;
        }
    }

    void OnHold()
    {
        if (interactor.isHeliDecended == false)
        {
            interactor.isHeliDecended = true;
        }

        if (vehicleType == VehicleType.Infill)
        {
            if (interactor.isPlayerInVehicle == false && !beginTakeoff)
            {
                timeToTakeOff = Time.time + takeOffTimer;
                beginTakeoff = true;
            }

            if (beginTakeoff)
            {
                if (Time.time > timeToTakeOff)
                {
                    stage = FlightStage.Ascending;
                }
            }
        }
        if (vehicleType == VehicleType.Exfill)
        {
            if (interactor.isPlayerInVehicle == true && !beginTakeoff)
            {
                timeToTakeOff = Time.time + takeOffTimer;
                beginTakeoff = true;
            }

            if (beginTakeoff)
            {
                if (Time.time > timeToTakeOff)
                {
                    stage = FlightStage.Ascending;
                }
            }
        }
        
    }

    void OnAscent()
    {
        if (interactor.isHeliDecended == true)
        {
            interactor.isHeliDecended = false;
        }

        nextNode = 0;
        nextNodeObject = RoutePositions[routId].infilExitRoute[nextNode];

        float dist = Vector3.Distance(helicopter.transform.position, nextNodeObject.transform.position);

        if (dist > decentHeight)
        {
            Ascend();
        }

        if (dist <= decentHeight)
        {
            nextNode++;
            stage = FlightStage.Exit;
        }
    }

    void OnExit()
    {
        nextNodeObject = RoutePositions[routId].infilExitRoute[nextNode];

        float dist = Vector3.Distance(helicopter.transform.position, nextNodeObject.transform.position);

        speed = Mathf.Lerp(decentSpeed, exitSpeed, currentAccelerationRate);

        currentAccelerationRate += Time.deltaTime;

        if (dist > nodeRange)
        {
            Move(nextNodeObject.transform.position);
        }
        if (dist <= nodeRange)
        {
            beginFlying = false;

            if(vehicleType != VehicleType.Exfill)
            {
                helicopter.SetActive(false);
            }
        }
    }

    void Move(Vector3 targetDir)
    {
        /*Vector3 newDir = Vector3.RotateTowards(helicopter.transform.forward, targetDir, rotationalSpeed * Time.deltaTime, 0.0f);
        //Vector3 dampedDir = Vector3.SmoothDamp(helicopter.transform.forward, newDir, ref someVelocityThing, dampSpeed);

        helicopter.transform.LookAt(targetDir);*/


        Vector3 dir = targetDir - helicopter.transform.position;
        //dir.y = 0; // keep the direction strictly horizontal
        Quaternion rot = Quaternion.LookRotation(dir);
        // slerp to the desired rotation over time
        helicopter.transform.rotation = Quaternion.Slerp(helicopter.transform.rotation, rot, rotationalSpeed * Time.deltaTime);


        helicopter.transform.position += helicopter.transform.forward * Time.deltaTime * speed;
    }

    void Decend(Quaternion targetRotation)
    {
        helicopter.transform.rotation = Quaternion.Lerp(helicopter.transform.rotation, targetRotation, Time.deltaTime);

        //helicopter.transform.position = Vector3.Lerp(helicopter.transform.position, nextNodeObject.transform.position, Time.deltaTime * speed);
        helicopter.transform.position = Vector3.MoveTowards(helicopter.transform.position, nextNodeObject.transform.position, Time.deltaTime * speed);
        //helicopter.transform.position -= helicopter.transform.up * Time.deltaTime * speed;
    }

    void Ascend()
    {
        helicopter.transform.position = Vector3.MoveTowards(helicopter.transform.position, nextNodeObject.transform.position, Time.deltaTime * speed);
    }

    public void StartAudio()
    {
        helicopterAudio.Play();
    }

    public void SetFlyingState(bool value)
    {
        this.gameObject.SetActive(value);
        beginFlying = value;
        StartAudio();
    }

    /*private void OnDrawGizmos()
    {
        
    }*/
}
