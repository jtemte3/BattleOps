using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public abstract class MissionEvent : MonoBehaviour
{
    public CompassPoint compassPoint;

    [Header("Objective Settings")]
    public string objectiveShortDescription;
    [Multiline]
    public string objectiveDescription;
    public bool isObjActive;
    public bool isObjCompleted;
    private bool isInitialized = false;

    public abstract void Engage();

    [Tooltip("Events to trigger on event completion")]
    public UnityEvent OnCompletion;
    public UnityEvent onCompletion => OnCompletion;

    private void Update()
    {
        if (compassPoint.compassVisualPrefab != null && isInitialized == false)
        {
            if (isObjActive && !isObjCompleted)
            {
                SetEventActive();
            }
            if (!isObjActive)
            {
                compassPoint.SetPointActive(false);
            }

            isInitialized = true;
        }
    }

    public void SetEventActive()
    {
        isObjActive = true;
        compassPoint.SetPointActive(true);
    }

    public void TriggerCompletion()
    {
        isObjCompleted = true;
        isObjActive= false;
        compassPoint.SetPointActive(false);
        OnCompletion.Invoke();
    }
}
