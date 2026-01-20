using UnityEngine;
using UnityEngine.Animations.Rigging;

public class LeanController : MonoBehaviour
{
    public MultiRotationConstraint chestIK;
    public ControlSchemeManager controlScheme;

    public float leanInDegrees;
    public float leanSpeed;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(controlScheme.leanLeft))
        {
            chestIK.data.offset = new Vector3(0,0, Mathf.Lerp(chestIK.data.offset.z, leanInDegrees, Time.deltaTime * leanSpeed));
        }
        else if (Input.GetKey(controlScheme.leanRight))
        {
            chestIK.data.offset = new Vector3(0, 0, Mathf.Lerp(chestIK.data.offset.z, -leanInDegrees, Time.deltaTime * leanSpeed));
        }
        else
        {
            chestIK.data.offset = new Vector3(0, 0, Mathf.Lerp(chestIK.data.offset.z, 0, Time.deltaTime * leanSpeed));
        }
    }
}
