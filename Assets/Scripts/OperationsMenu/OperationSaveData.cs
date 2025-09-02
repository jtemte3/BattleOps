using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OperationSaveData
{
    public int operationDay;
    public int operationDuration;
    public int heartsScore;
    public int mindsScore;
    public int funds;
    public int peaceScore;
    public int goodCasualties;
    public int civCasualties;
    public int badCasualties;

    public List<MissionRecord> pastMissions;
    public List<MissionOption> currentMissionOptions;
}

[System.Serializable]
public class MissionRecord
{
    public int day;
    public int duration;
    public string title;
    public string result; // "Success", "Failure", etc.
    public int heartsChange;
    public int mindsChange;
    public int fundsChange;
    public int peaceChange;
    public int goodCasualties;
    public int civCasualties;
    public int badCasualties;
}

[System.Serializable]
public class MissionOption
{
    public string missionId;
    public int baseRiskFactor;
    public int baseIntelFactor;
}

public class MissionRecordComponent : MonoBehaviour
{
    public int day;
    public int duration;
    public string title;
    public string result; // "success", "failure"
    public int heartsChange;
    public int mindsChange;
    public int fundsChange;
    public int peaceChange;
    public int goodCasualties;
    public int civCasualties;
    public int badCasualties;

    public void ImportData(MissionRecord missionRecord)
    {
        day = missionRecord.day;
        title = missionRecord.title;
        result = missionRecord.result;
        heartsChange = missionRecord.heartsChange;
        mindsChange = missionRecord.mindsChange;
        fundsChange = missionRecord.fundsChange;
        peaceChange = missionRecord.peaceChange;
        goodCasualties = missionRecord.goodCasualties;
        civCasualties = missionRecord.civCasualties;
        badCasualties = missionRecord.badCasualties;
    }
}
