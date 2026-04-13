using UnityEngine;
using UnityEngine.Events;

public class Event_WaitThenTrigger : MonoBehaviour
{
    public float timer;
    float triggerTime = 0.0f;
    public bool isTimerEnabled = false;
    [Space]
    [Tooltip("Events to trigger on event completion")]
    public UnityEvent OnCompletion;
    public UnityEvent onCompletion => OnCompletion;
    public void Update()
    {
        if (isTimerEnabled)
        {
            if (triggerTime == 0.0f)
            {
                triggerTime = Time.time + timer;
            }
            else
            {
                if (Time.time > triggerTime)
                {
                    isTimerEnabled = false;
                    TriggerCompletion();
                }
            }
        }
    }

    public void TriggerCompletion()
    {
        OnCompletion.Invoke();
    }

    public void StartTimer()
    {
        isTimerEnabled = true;
    }
}
