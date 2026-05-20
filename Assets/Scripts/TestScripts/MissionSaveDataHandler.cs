using System;
using System.IO;
using UnityEngine;

public class MissionSaveDataHandler : MonoBehaviour
{
    public MissionStatsTracker missionStatsTracker;

    public string saveFileName = "OperationSave.json";
    public OperationSaveData currentData;

    public void Awake()
    {
        LoadCurrentOperationFile();
        //LoadSaveFile();
    }
    public void UpdateSaveData()
    {
        AddNewMission();
        currentData.operationDay++;
        WriteSaveFile();
    }

    public void LoadCurrentOperationFile()
    {
        // Get all json files in the save directory
        string[] saveFiles = Directory.GetFiles(Application.streamingAssetsPath, "*.json");

        foreach (string file in saveFiles)
        {
            try
            {
                string json = File.ReadAllText(file);

                // Try to deserialize
                OperationSaveData saveData = JsonUtility.FromJson<OperationSaveData>(json);

                // Verify the object is valid and unfinished
                if (saveData != null && saveData.isFinished == false)
                {
                    saveFileName = Path.GetFileName(file);
                    currentData = saveData;
                    break;
                }
            }
            catch
            {
                // Ignore invalid or corrupted json files
                Debug.LogWarning("Failed to load save file: " + file);
            }
        }
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
            missionRecord.result = "success";
        }
        else
        {
            missionRecord.result = "failure";
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

        currentData.lastSaveDate = DateTime.Now.Day + "/" + DateTime.Now.Month + "/" + DateTime.Now.Year;

        int hour = DateTime.Now.Hour;
        string meridiem = "AM";

        if (hour >= 12)
        {
            meridiem = "PM";

            if ((hour % 12) != 0)
            {
                hour = hour - 12;
            }
        }

        currentData.lastSaveTime = hour + ":" + DateTime.Now.Minute + " " + meridiem;

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
