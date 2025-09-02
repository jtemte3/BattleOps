using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassthroughGameData : MonoBehaviour
{
    public int riskFactor;
    public int intelFactor;
    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void SetPersistantData(int newRiskFactor, int newIntelFactor)
    {
        riskFactor = newRiskFactor;
        intelFactor = newIntelFactor;
    }

    public void DestroyPersistantDataObject()
    {
        Destroy(this.gameObject);
    }
}
