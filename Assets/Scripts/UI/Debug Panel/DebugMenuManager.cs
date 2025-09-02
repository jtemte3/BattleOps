using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugMenuManager : MonoBehaviour
{
    public ControlSchemeManager controlScheme;
    public GameObject debugPanel;
    public bool isPanelEnabled;

    public List<UIElement> elements = new List<UIElement>();
    // Start is called before the first frame update
    void Start()
    {
        debugPanel.SetActive(isPanelEnabled);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(controlScheme.debugMenu))
        {
            if (isPanelEnabled)
            {
                isPanelEnabled = false;
                debugPanel.SetActive(false);
            }
            else
            {
                isPanelEnabled = true;
                debugPanel.SetActive(true);
            }

        }

        if (isPanelEnabled)
        {
            foreach(UIElement element in elements)
            {
                element.Activate();
            }
        }
    }
}
