using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractionTextManager : MonoBehaviour
{
    public TMP_Text textElement;

    public void SetTextState(bool state)
    {
        textElement.enabled = state;
    }

    public void SetTextValue(string value)
    {
        textElement.text = value;
    }
}
