using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public abstract class ObjectiveEvent : MonoBehaviour
{
    [Header("Objective Settings")]
    public string objectiveShortDescription;
    [Multiline]
    public string objectiveDescription;
    public bool isObjActive;
    public bool isObjCompleted;

    [Tooltip("Events to trigger on event completion")]
    public UnityEvent OnCompletion;
    public UnityEvent onCompletion => OnCompletion;

    public abstract void Engage();
    public abstract void ActivateObjective();
    public abstract void DeactivateObjective();
}
