using System.IO;
using UnityEngine;
using static UnityEngine.InputManagerEntry;
using static UnityEngine.Rendering.HDROutputUtils;

public class SaveDataHandler : MonoBehaviour
{
    public MissionStatsTracker missionStatsTracker;

    public string saveFileName = "OperationSave.json";
    public OperationSaveData currentData;

    public void Awake()
    {
        LoadSaveFile();
    }
    public void UpdateSaveData()
    {
        AddNewMission();
        currentData.operationDay++;
        WriteSaveFile();
    }

    public void LoadSaveFile()
    {
        string path = Path.Combine(Application.streamingAssetsPath, saveFileName);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            currentData = JsonUtility.FromJson<OperationSaveData>(json);
            Debug.Log("Save loaded: Day " + currentData.operationDay);
        }
        else
        {
            Debug.LogWarning("Save file not found at: " + path);
        }
    }

    public void AddNewMission()
    {
        (string title, bool sucess, int heartsScore, int mindsScore, int funds, int peaceScoreChange, int squadKills, int civKilles, int badKills) = missionStatsTracker.getMissionResults();

        currentData.heartsScore += heartsScore;
        currentData.mindsScore += mindsScore;
        currentData.funds += funds;
        currentData.goodCasualties += squadKills;
        currentData.civCasualties += civKilles;
        currentData.badCasualties += badKills;
        currentData.peaceScore += peaceScoreChange;

        MissionRecord missionRecord = new MissionRecord();
        missionRecord.day = currentData.operationDay;
        missionRecord.title = title;

        if (sucess)
        {
            missionRecord.result = "sucess";
        }
        else
        {
            missionRecord.result = "failuer";
        }
        
        missionRecord.heartsChange = heartsScore;
        missionRecord.mindsChange = mindsScore;
        missionRecord.fundsChange = funds;
        missionRecord.peaceChange = peaceScoreChange;
        missionRecord.goodCasualties = squadKills;
        missionRecord.civCasualties = civKilles;
        missionRecord.badCasualties = badKills;

        currentData.pastMissions.Add(missionRecord);
    }

    public void WriteSaveFile()
    {
        string path = Path.Combine(Application.streamingAssetsPath, saveFileName);

        if (File.Exists(path))
        {
            string json = JsonUtility.ToJson(currentData);
            File.WriteAllText(path, json);
            Debug.Log("Save Written: Day " + currentData.operationDay);
            /*string json = File.ReadAllText(path);
            currentData = JsonUtility.FromJson<OperationSaveData>(json);
            Debug.Log("Save loaded: Day " + currentData.operationDay);*/
        }
        else
        {
            Debug.LogWarning("Save file not found at: " + path);
        }
    }
}
