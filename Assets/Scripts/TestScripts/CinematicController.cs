using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CinematicController : MonoBehaviour
{
    public List<Image> imagesToFade;
    public float fadeTime;
    public bool isVisable;
    public float fadePercentage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (isVisable)
        {
            fadePercentage = 100;
        }
        else
        {
            fadePercentage = 0;
        }

        ApplyFade(fadePercentage);
    }

    // Update is called once per frame
    void Update()
    {
        if (isVisable == true && fadePercentage != 100)
        {
            if (Mathf.Round(fadePercentage) != 100)
            {
                fadePercentage = Mathf.Lerp(fadePercentage, 100, fadeTime);
                ApplyFade(fadePercentage);
            }
            else
            {
                fadePercentage = 100;
                ApplyFade(100);
            }

        }

        if (isVisable == false && fadePercentage != 0)
        {
            if (Mathf.Round(fadePercentage) != 0)
            {
                fadePercentage = Mathf.Lerp(fadePercentage, 0, fadeTime);
                ApplyFade(fadePercentage);
            }
            else
            {
                fadePercentage = 0;
                ApplyFade(0);
            }
            
        }
    }

    void ApplyFade(float value)
    {
        float percent = value / 100;
        foreach (Image image in imagesToFade)
        {
            Color currentColor = image.color;
            currentColor.a = percent;

            image.color = currentColor;
        }
    }

    public void SetFadeState(bool state)
    {
        isVisable = state;
    }
}
