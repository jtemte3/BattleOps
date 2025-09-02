using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrouchController : MonoBehaviour
{
    public Vector3 standingPosition;
    public Vector3 crouchPosition;
    Vector3 currentPosition;
    public float lerpSpeed;
    public GameObject cameraParent;
    public JetpackPlayerController playerController;
    public CapsuleCollider hitbox;
    // Start is called before the first frame update
    void Start()
    {
        cameraParent.transform.localPosition = standingPosition;
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        SetStance();
    }

    void SetStance()
    {
        string stanceState = playerController.GetStanceState();

        if (stanceState.Equals("standing"))
        {
            cameraParent.transform.localPosition = Vector3.Lerp(cameraParent.transform.localPosition, standingPosition, lerpSpeed * Time.deltaTime);
            hitbox.center = new Vector3(0, 0, 0);
            hitbox.height = 2;
        }
        else
        {
            cameraParent.transform.localPosition = Vector3.Lerp(cameraParent.transform.localPosition, crouchPosition, lerpSpeed * Time.deltaTime);
            hitbox.center = new Vector3(0, -.375f, 0);
            hitbox.height = 1.25f;
        }
    }
}
