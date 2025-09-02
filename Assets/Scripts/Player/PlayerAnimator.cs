using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    public JetpackPlayerController playerController;
    public Animator animator;

    // Update is called once per frame
    void Update()
    {
        if (playerController.GetMovementState().Equals("idle"))
        {
            animator.SetBool("isRunning", false);
            animator.SetFloat("speed", 0);
        }
        else
        {
            animator.SetBool("isRunning", true);
            animator.SetFloat("speed", playerController.GetCurrentSpeed());
        }

        if (playerController.GetStanceState().Equals("standing"))
        {
            animator.SetBool("isCrouching", false);
        }
        else
        {
            animator.SetBool("isCrouching", true);
        }
    }
}
