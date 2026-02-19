using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class LeanController : MonoBehaviour
{
    public MultiRotationConstraint chestIK;
    public ControlSchemeManager controlScheme;
    public SwayAndBobIK swayAndBobIK;
    public ADSManager adsManager;

    public float leanInDegrees;
    public float leanSpeed;

    public float currentLean = 0f;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(controlScheme.leanLeft))
        {
            currentLean = DetermineLeanOffset(leanInDegrees);
        }
        else if (Input.GetKey(controlScheme.leanRight))
        {
            currentLean = DetermineLeanOffset(-leanInDegrees);
        }
        else
        {
            currentLean = DetermineLeanOffset(0);
        }

        DetermineSwayLeanState();
    }
    private float DetermineLeanOffset(float targetDegree)
    {
        float lean = Mathf.Lerp(chestIK.data.offset.z, targetDegree, Time.deltaTime * leanSpeed);
        chestIK.data.offset = new Vector3(0, 0, lean);
        return lean;
    }
    private void DetermineSwayLeanState()
    {
        switch (adsManager.GetAdsState())
        {
            case false:
                if (Input.GetKey(controlScheme.leanLeft))
                {
                    swayAndBobIK.SetOffsetType("idleLeanLeft");
                }
                else if (Input.GetKey(controlScheme.leanRight))
                {
                    swayAndBobIK.SetOffsetType("idleLeanRight");
                }
                else
                {
                    swayAndBobIK.SetOffsetType("idleCenter");
                }
                break;
            case true:
                if (Input.GetKey(controlScheme.leanLeft))
                {
                    swayAndBobIK.SetOffsetType("adsLeanLeft");
                }
                else if (Input.GetKey(controlScheme.leanRight))
                {
                    swayAndBobIK.SetOffsetType("adsLeanRight");
                }
                else
                {
                    swayAndBobIK.SetOffsetType("adsCenter");
                }
                break;
        }
    }
}
