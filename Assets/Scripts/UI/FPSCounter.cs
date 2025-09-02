using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPSCounter : UIElement
{
    public TMP_Text fpsLabel;
    private string fpsLabelFormat;
    public float refreshRate = .5f;
    private float timer = 0;

    // Start is called before the first frame update
    void Start()
    {
        fpsLabelFormat = fpsLabel.text;
    }

    public override void Activate()
    {
        if (Time.unscaledTime > timer)
        {
            int fps = (int)(1f / Time.unscaledDeltaTime);
            fpsLabel.text = string.Format(fpsLabelFormat, fps);
            timer = Time.unscaledTime + refreshRate;
        }
    }
}
