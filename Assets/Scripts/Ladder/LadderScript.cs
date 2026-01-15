using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderScript : MonoBehaviour
{
    public LayerMask PlayerMask;
    public LayerMask LadderMask;
    public GameObject startPosition;
    public GameObject endPosition;

    void FixedUpdate()
    {
        CheckCollisions();
    }

    void CheckCollisions()
    {
        // Use the OverlapBox to detect if there are any other colliders within this box area.
        // Use the GameObject's center, half the size (as a radius), and rotation. This creates an invisible box around your GameObject.
        Collider[] startHitColliders = Physics.OverlapBox(startPosition.transform.position, startPosition.transform.localScale / 2, Quaternion.identity, PlayerMask);
        Collider[] endHitColliders = Physics.OverlapBox(endPosition.transform.position, endPosition.transform.localScale / 2, Quaternion.identity, PlayerMask);

        // Check when there is a new collider coming into contact with the box
        if (startHitColliders.Length > 0)
        {
            startHitColliders[0].gameObject.GetComponent<PlayerController>().isClimbingLadder = true;

            /*RaycastHit hit;
            if (Physics.Raycast(startHitColliders[0].gameObject.transform.position, startHitColliders[0].gameObject.transform.TransformDirection(Vector3.forward), out hit, 3, LadderMask))
            {
                startHitColliders[0].gameObject.GetComponent<PlayerController>().isClimbingLadder = true;
            }*/
        }

        if (endHitColliders.Length > 0)
        {
            endHitColliders[0].gameObject.GetComponent<PlayerController>().isClimbingLadder = false;
        }
    }
#if (UNITY_EDITOR)
    // Draw the Box Overlap as a gizmo to show where it currently is testing. Click the Gizmos button to see this.
    void OnDrawGizmos()
    {
        if (startPosition != null && endPosition != null)
        {
            Gizmos.color = Color.green;
            // Check that it is being run in Play Mode, so it doesn't try to draw this in Editor mode
            //if (Application.isPlaying)
            // Draw a cube where the OverlapBox is (positioned where your GameObject is as well as a size)
            Gizmos.DrawWireCube(startPosition.transform.position, startPosition.transform.localScale);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(endPosition.transform.position, endPosition.transform.localScale);
        }
    }
#endif
}
