using System.Collections.Generic;
using UnityEngine;

public class HUDToggle : MonoBehaviour
{
    public ControlSchemeManager controlScheme;
    public bool showHUD;
    public List<GameObject> gameObjectstoDisable = new List<GameObject>();
    public List<GameObject> gameObjectstoHide = new List<GameObject>();

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(controlScheme.toggleHUD))
        {
            if (showHUD)
            {
                SetHUDElements(false);
                showHUD = false;
            }
            else
            {
                SetHUDElements(true);
                showHUD = true;
            }
        }
    }

    void SetHUDElements(bool state)
    {
        if (gameObjectstoDisable.Count > 0)
        {
            foreach (GameObject element in gameObjectstoDisable)
            {
                element.SetActive(state);
            }
        }
        if (gameObjectstoHide.Count > 0)
        {
            foreach (GameObject element in gameObjectstoHide)
            {
                if (state == false)
                {
                    if (element.GetComponent<SkinnedMeshRenderer>() != null)
                    {
                        element.GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                    }
                    else
                    {
                        element.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                    }
                }
                else
                {
                    if (element.GetComponent<SkinnedMeshRenderer>() != null)
                    {
                        element.GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                    }
                    else
                    {
                        element.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                    }
                }
            }
        }
    }
}
