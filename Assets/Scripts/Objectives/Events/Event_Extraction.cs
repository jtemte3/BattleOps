using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Event_Extraction : MissionEvent
{
    public PlayerController player;
    public LayerMask TriggerMask;
    //public ControlSchemeManager controlSchemeManager;
    public InteractionTextManager interactionManager;

    public float activationDistance;
    public bool isActivated = false;
    
    public override void Engage()
    {
        if (player == null)
        {
            player = FindAnyObjectByType<PlayerController>();
        }
        else
        {
            compassPoint.UpdatePointPosition(transform.position);

            float distance = Vector3.Distance(player.transform.position, transform.position);

            if (distance <= activationDistance)
            {
                if (!isActivated)
                {
                    //interactionManager.SetTextValue("Press " + controlSchemeManager.interact + " to Signal Extraction");
                    interactionManager.SetTextValue("Throw a Smoke Grenade to Signal Extraction");
                    interactionManager.SetTextState(true);

                    Collider[] triggerColliders = Physics.OverlapSphere(this.transform.position, activationDistance, TriggerMask);
                    if (triggerColliders.Length > 0)
                    {
                        isActivated = true;
                        interactionManager.SetTextState(false);

                        //Call Heli-Sequence here, remove the temp completion call
                        TriggerCompletion();
                    }

                    /*if (Input.GetKeyDown(controlSchemeManager.interact))
                    {
                        isActivated = true;
                        interactionManager.SetTextState(false);

                        //Call Heli-Sequence here, remove the temp completion call
                        TriggerCompletion();
                    }*/
                }
            }
            else if (distance >= activationDistance && !isActivated)
            {
                interactionManager.SetTextState(false);
            }
        }
    }

#if (UNITY_EDITOR)
    void OnDrawGizmos()
    {
        Handles.DrawWireDisc(transform.position, new Vector3(0, 1, 0), activationDistance);
        Handles.DrawLine(transform.position, transform.position + (Vector3.up * 5));
    }
#endif
}
