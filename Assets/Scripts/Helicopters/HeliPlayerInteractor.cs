using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeliPlayerInteractor : MonoBehaviour
{

    public Transform HeliBase;
    public bool isPlayerInVehicle = false;
    public bool isHeliDecended = false;
    public bool canPlayerInteract = false;
    public bool canPlayerMount = true;
    public float groundCheckDistance = 1.0f;
    public float interactionDistance = 5.0f;
    public GameObject playerSeat;
    public Vector3 playerSeatRotation;

    public ControlSchemeManager controlSchemeManager;
    public PlayerController controller;
    //public List<GameObject> gameObjectsToManage;
    public InteractionTextManager interactionManager;

    // Start is called before the first frame update
    void Start()
    {
        if (isPlayerInVehicle == true)
        {
            DeactivatePlayer();
            interactionManager.SetTextState(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Check Helicopter distance to the ground
        RaycastHit hit;
        // This determines if the player is able to hop out of the Helicopter
        if (Physics.Raycast(HeliBase.position, HeliBase.TransformDirection(Vector3.down), out hit, groundCheckDistance))
        {
            Debug.DrawRay(HeliBase.position, HeliBase.TransformDirection(Vector3.down) * groundCheckDistance, Color.yellow);
            //Debug.Log("On the ground");
            canPlayerInteract = true;
        }
        else
        {
            Debug.DrawRay(HeliBase.position, HeliBase.TransformDirection(Vector3.down) * groundCheckDistance, Color.white);
            //Debug.Log("Not on the ground");
            canPlayerInteract = false;
        }

        //manage player interactions
        if (isPlayerInVehicle == true)
        {
            ManagePlayerPosition();

            if (canPlayerInteract && isHeliDecended)
            {
                interactionManager.SetTextValue("Press " + controlSchemeManager.interact + " to Exit Helicopter");
                interactionManager.SetTextState(true);

                if (Input.GetKeyDown(controlSchemeManager.interact))
                {
                    ActivatePlayer();
                    interactionManager.SetTextState(false);
                }
            }
            else
            {
                interactionManager.SetTextState(false);
            }
        }
        else
        {
            if (canPlayerMount == true && isHeliDecended)
            {
                float distance = Vector3.Distance(controller.transform.position, this.transform.position);

                if (distance < interactionDistance)
                {
                    interactionManager.SetTextValue("Press " + controlSchemeManager.interact + " to Enter Helicopter");
                    interactionManager.SetTextState(true);
                }

                if (Input.GetKeyDown(controlSchemeManager.interact))
                {
                    DeactivatePlayer();
                    interactionManager.SetTextState(false);
                }
            }
        }
    }

    void ManagePlayerPosition()
    {
        controller.gameObject.transform.localPosition = Vector3.zero;
    }

    void ActivatePlayer()
    {
        controller.isMounted = false;

        controller.gameObject.transform.parent = null;
        isPlayerInVehicle = false;
    }

    void DeactivatePlayer()
    {
        controller.isMounted = true;

        controller.gameObject.transform.parent = playerSeat.transform;
        controller.gameObject.transform.localRotation = Quaternion.Euler(playerSeatRotation);
        isPlayerInVehicle = true;
    }
}
