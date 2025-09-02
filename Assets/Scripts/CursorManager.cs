using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{

    private void Start()
    {
        SetCursorState(false);
    }

    /*// Update is called once per frame
    void Update()
    {
        //Check for cursor settings
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetCursor();
        }
    }*/

    public void SetCursor()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            //Unlock the Cursor
            Cursor.lockState = CursorLockMode.None;
            //Set Cursor to be visible
            Cursor.visible = true;
        }
        if (Cursor.lockState == CursorLockMode.None)
        {
            //Lock the Cursor
            Cursor.lockState = CursorLockMode.Locked;
            //Set Cursor to not be visible
            Cursor.visible = false;
        }
    }

    public void SetCursorState(bool state)
    {
        if (state == true)
        {
            //Unlock the Cursor
            Cursor.lockState = CursorLockMode.None;
            //Set Cursor to be visible
            Cursor.visible = true;
        }
        if (state == false)
        {
            //Lock the Cursor
            Cursor.lockState = CursorLockMode.Locked;
            //Set Cursor to not be visible
            Cursor.visible = false;
        }
    }
}
