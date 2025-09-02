using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class CompassVisual : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public bool isDirectionPoint;
    public Vector3 pointPosition;
}
