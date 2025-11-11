using UnityEngine;

public class TimedDelete : MonoBehaviour
{
    public float destroyTimer = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Destroy(this.gameObject, destroyTimer);
    }
}
