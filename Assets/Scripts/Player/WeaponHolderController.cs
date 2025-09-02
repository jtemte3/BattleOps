using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WeaponHolderController : MonoBehaviour
{
    public JetpackPlayerController playerController;
    public Animator animator;

    public int fovRegular;
    public int fovAds;
    public float visReg;
    public float visAds;
    public float transitionSpeed;

    public Volume volume;
    Vignette vignette;
    public Camera cam;

    private void Start()
    {
        volume.profile.TryGet(out vignette);
    }

    // Update is called once per frame
    void Update()
    {
        string movementState = playerController.GetMovementState();
        string aimState = playerController.GetAimState();

        switch (movementState)
        {
            case "walking":
                animator.SetBool("isRunning", false);
                break;
            case "idle":
                animator.SetBool("isRunning", false);
                break;
            case "sprinting":
                animator.SetBool("isRunning", true);
                break;
        }

        switch (aimState)
        {
            case "basic":
                animator.SetBool("isADS", false);
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fovRegular, transitionSpeed * Time.deltaTime);
                vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, visReg, transitionSpeed *Time.deltaTime);
                break;
            case "ads":
                animator.SetBool("isADS", true);
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fovAds, transitionSpeed * Time.deltaTime);
                vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, visAds, transitionSpeed * Time.deltaTime);
                break;
        }
    }
}
