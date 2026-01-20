using UnityEngine;

public class IKPlayerAnimator : MonoBehaviour
{
    public PlayerController playerController;
    public Animator animator;

    // Update is called once per frame
    void Update()
    {
        if (playerController.GetMountStatus())
        {
            animator.SetBool("mounted", true);
            animator.SetBool("clipped", true);
            animator.SetBool("walk", false);
            animator.SetBool("jump", false);
            animator.SetBool("croutch", false);
            animator.SetFloat("speed", 0);
        }
        else
        {
            animator.SetBool("mounted", false);

            if (playerController.GetMovementState().Equals("idle"))
            {
                animator.SetBool("walk", false);
                animator.SetBool("jump", false);
                animator.SetFloat("speed", 0);
            }
            else if (playerController.GetMovementState().Equals("jump"))
            {
                animator.SetBool("jump", true);
                animator.SetBool("walk", false);
            }
            else
            {
                animator.SetBool("walk", true);
                animator.SetBool("jump", false);
                animator.SetFloat("speed", playerController.GetCurrentSpeed());
            }

            if (playerController.GetStanceState().Equals("standing"))
            {
                animator.SetBool("croutch", false);
            }
            else
            {
                animator.SetBool("croutch", true);
            }
        }
    }
}
