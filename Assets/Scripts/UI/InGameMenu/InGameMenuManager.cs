using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameMenuManager : MonoBehaviour
{
    public CursorManager cursorManager;
    public ControlSchemeManager controlScheme;
    public GameObject inGameMenuPanel;
    public List<GameObject> uiElementsToDisable;
    public GunFire gunScript;
    public JetpackPlayerController playerController;
    public bool isMenuActive;

    // Start is called before the first frame update
    void Start()
    {
        inGameMenuPanel.SetActive(false);
        isMenuActive = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(controlScheme.escapeMenu))
        {
            if (isMenuActive)
            {
                isMenuActive = false;
            }
            else
            {
                isMenuActive = true;
            }

            ManagePanel();
        }

        
    }

    void ManagePanel()
    {
        if (isMenuActive)
        {
            DisableUIElements();
            gunScript.enabled = false;
            playerController.enabled = false;
            inGameMenuPanel.SetActive(true);
            cursorManager.SetCursorState(true);
        }
        else
        {
            inGameMenuPanel.SetActive(false);
            cursorManager.SetCursorState(false);
            EnableUIElements();
            gunScript.enabled = true;
            playerController.enabled = true;
        }
    }

    void DisableUIElements()
    {
        foreach(GameObject element in uiElementsToDisable)
        {
            element.SetActive(false);
        }
    }

    void EnableUIElements()
    {
        foreach (GameObject element in uiElementsToDisable)
        {
            element.SetActive(true);
        }
    }

    public void LoadScene(int sceneNumber)
    {
        SceneManager.LoadScene(sceneNumber);
    }

    public void ExitApplication()
    {
        Application.Quit();
    }
}
