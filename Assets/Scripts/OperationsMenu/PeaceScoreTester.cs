using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PeaceScoreTester : MonoBehaviour
{
    public OperationUILoader loader;
    public bool hasLoaded = false;
    public int day;
    public int opDays;
    public int hearts;
    public int minds;
    public int peace;

    // Update is called once per frame
    void Update()
    {
        if (loader.dataHandler.hasLoaded && hasLoaded == false)
        {
            day = loader.dataHandler.currentData.operationDay;
            opDays = loader.dataHandler.currentData.operationDuration;
            hearts = loader.dataHandler.currentData.heartsScore;
            minds = loader.dataHandler.currentData.mindsScore;

            hasLoaded = true;
        }

        peace = loader.CalculatePeaceScoreManually(day, opDays, hearts, minds);

    }
}
