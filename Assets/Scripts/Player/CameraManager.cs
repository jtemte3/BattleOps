using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public JetpackPlayerController playerController;
    public Animator animator;

    // Update is called once per frame
    void Update()
    {
        string state = playerController.GetCameraState();

        switch (state)
        {
            case "right":
                animator.SetBool("right", true);
                animator.SetBool("left", false);
                break;
            case "center":
                animator.SetBool("right", false);
                animator.SetBool("left", false);
                break;
            case "left":
                animator.SetBool("right", false);
                animator.SetBool("left", true);
                break;
        }
    }
}
