using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ADSManager : MonoBehaviour
{
    public ControlSchemeManager controlScheme;
    public RecoilControllerIK recoilController;

    public int fovRegular;
    public int fovAds;
    public float visReg;
    public float visAds;
    public float transitionSpeed;

    public Volume volume;
    Vignette vignette;
    public Camera cam;

    private bool adsState = false;

    private void Start()
    {
        volume.profile.TryGet(out vignette);
    }

    // Update is called once per frame
    void Update()
    {
        DetectAdsState();
        HandleAdsState();
    }

    public void SetAdsState(bool state)
    {
        adsState = state;
    }

    public bool GetAdsState()
    {
        return adsState;
    }

    private void DetectAdsState()
    {
        bool isAds = Input.GetKey(controlScheme.weaponAimDownSights);

        if (isAds)
        {
            SetAdsState(true);
            recoilController.isAds = true;
        }
        else
        {
            SetAdsState(false);
            recoilController.isAds = false;
        }
    }

    private void HandleAdsState()
    {
        switch (adsState)
        {
            case false:
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fovRegular, transitionSpeed * Time.deltaTime);
                vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, visReg, transitionSpeed * Time.deltaTime);
                break;
            case true:
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fovAds, transitionSpeed * Time.deltaTime);
                vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, visAds, transitionSpeed * Time.deltaTime);
                break;
        }
    }
}
