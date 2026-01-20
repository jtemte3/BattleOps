using UnityEngine;
using UnityEngine.Android;

public class InteractionManager : MonoBehaviour
{
    public ControlSchemeManager controlScheme;
    public PlayerController playerController;
    public Animator animator;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(controlScheme.interact))
        {
            animator.SetTrigger("interaction");
        }
    }
}
