using System.Collections.Generic;
using UnityEngine;

public class HUDToggle : MonoBehaviour
{
    public ControlSchemeManager controlScheme;
    public bool showHUD;
    public List<GameObject> gameObjects = new List<GameObject>();

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
        foreach (GameObject element in gameObjects)
        {
            element.SetActive(state);
        }
    }
}
