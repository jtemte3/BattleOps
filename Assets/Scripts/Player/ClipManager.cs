using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClipManager : MonoBehaviour
{
    public Camera cam;
    public Animator animator;
    public float distance;
    public LayerMask collisionLayers;
    public bool isClipped;

    // Update is called once per frame
    void FixedUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, distance))
        {
            isClipped = true;
            animator.SetBool("isClipped", true);
        }
        else
        {
            isClipped = false;
            animator.SetBool("isClipped", false);
        }
    }

    public bool GetIsClipped()
    {
        return isClipped;
    }
}
