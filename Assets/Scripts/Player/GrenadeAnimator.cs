using UnityEngine;

public class GrenadeAnimator : MonoBehaviour
{
    public ControlSchemeManager controlScheme;
    public Animator animator;

    public GrenadeThrower grenadeThrower;

    // Update is called once per frame
    void Update()
    {
        if (grenadeThrower.canThrow)
        {
            if (grenadeThrower.isRecharging == true)
            {
                animator.SetBool(AnimParams.clipped, true);
            }
            if (grenadeThrower.isRecharging == false)
            {
                animator.SetBool(AnimParams.clipped, false);
            }
            if (Input.GetKeyDown(controlScheme.weaponFire) && grenadeThrower.isRecharging == false)
            {
                animator.SetBool(AnimParams.actionBool, true);
            }
            if (Input.GetKeyUp(controlScheme.weaponFire) && grenadeThrower.isRecharging == true)
            {
                animator.SetBool(AnimParams.actionBool, false);
            }
        }
        else
        {
            animator.SetBool(AnimParams.clipped, true);
        }

        
    }
}
