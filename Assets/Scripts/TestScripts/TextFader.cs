using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextFader : MonoBehaviour
{
    public TMP_Text textToFade;
    public float fadeTime;
    public bool isVisable;
    public float fadePercentage;
    public float textduration;
    private float timer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (isVisable)
        {
            fadePercentage = 100;
            textToFade.gameObject.SetActive(true);
        }
        else
        {
            fadePercentage = 0;
            textToFade.gameObject.SetActive(false);
        }

        ApplyFade(fadePercentage);
    }

    // Update is called once per frame
    void Update()
    {
        if (isVisable == true && fadePercentage != 100)
        {
            if (textToFade.gameObject.activeSelf == false)
            {
                textToFade.gameObject.SetActive(true);
            }

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
        
        if (isVisable == true && fadePercentage == 100)
        {
            if (timer == 0)
            {
                timer = Time.time + textduration;
            }

            if (Time.time >= timer)
            {
                isVisable = false;
                timer = 0;
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

                textToFade.gameObject.SetActive(false);
            }

        }
    }

    void ApplyFade(float value)
    {
        float percent = value / 100;

        Color currentColor = textToFade.color;
        currentColor.a = percent;

        textToFade.color = currentColor;
    }

    public void TriggerUIEvent()
    {
        isVisable = true;
    }
}
