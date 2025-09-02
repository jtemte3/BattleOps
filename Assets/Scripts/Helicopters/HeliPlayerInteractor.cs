using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeliPlayerInteractor : MonoBehaviour
{

    public Transform HeliBase;
    public bool isHeliDecended = false;
    public bool canPlayerExit = false;
    public bool hasPlayerExited = false;
    public float groundCheckDistance = 1.0f;

    public ControlSchemeManager controlSchemeManager;
    public JetpackPlayerController controller;
    public List<GameObject> gameObjectsToManage;
    public GameObject interactionUI;

    // Start is called before the first frame update
    void Start()
    {
        controller.canMove = false;
        interactionUI.gameObject.SetActive(false);

        foreach (GameObject go in gameObjectsToManage)
        {
            go.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        // This determines if the player is able to hop out of the Helicopter
        if (Physics.Raycast(HeliBase.position, HeliBase.TransformDirection(Vector3.down), out hit, groundCheckDistance))
        {
            Debug.DrawRay(HeliBase.position, HeliBase.TransformDirection(Vector3.down) * groundCheckDistance, Color.yellow);
            //Debug.Log("On the ground");
            canPlayerExit = true;
        }
        else
        {
            Debug.DrawRay(HeliBase.position, HeliBase.TransformDirection(Vector3.down) * groundCheckDistance, Color.white);
            //Debug.Log("Not on the ground");
            canPlayerExit = false;
        }
        
        if (canPlayerExit && isHeliDecended)
        {
            interactionUI.gameObject.SetActive(true);

            if (Input.GetKeyDown(controlSchemeManager.interact))
            {
                ActivatePlayer();
                interactionUI.gameObject.SetActive(false);
                this.enabled = false;
            }
        }
        
    }

    void ActivatePlayer()
    {
        controller.canMove = true;
        foreach (GameObject go in gameObjectsToManage)
        {
            go.SetActive(true);
        }

        controller.transform.localPosition += new Vector3(1, 0, 0);
        controller.gameObject.transform.parent = null;
        hasPlayerExited = true;
    }
}
