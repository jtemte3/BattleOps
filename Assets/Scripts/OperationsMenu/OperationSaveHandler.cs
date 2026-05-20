using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class OperationSaveHandler : MonoBehaviour
{
    public string saveFileName = "OperationSave.json";
    public bool hasLoaded = false;
    public OperationSaveData currentData;
    public List<MissionTemplate> missionTemplates = new();

    void Awake()
    {
        LoadCurrentOperationFile();
        //LoadSaveFile();
        hasLoaded = true;
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

            GenerateNewMissions();
        }
        else
        {
            Debug.LogWarning("Save file not found at: " + path);
        }
    }

    public void GenerateNewMissions()
    {
        //Get list of all mission Ids
        List<string> missionIds = missionTemplates.Select(x => x.missionId).ToList();
        List<string> selectedMissionIds = new();

        //Randomly pick 3, without duplicates
        while (selectedMissionIds.Count < 3)
        {
            int pos = Random.Range(0, missionIds.Count);

            if (!selectedMissionIds.Contains(missionIds[pos]))
            {
                selectedMissionIds.Add(missionIds[pos]);
            }
        }

        //TODO: Update the risk and intel to be based on Operation stats
        //Add the selected mission ids to the current mission object and give them details
        List<MissionOption> nextMissions = new();

        foreach (string id in selectedMissionIds)
        {
            nextMissions.Add(item: new MissionOption(id, Random.Range(1,3), Random.Range(1, 3)));
        }

        currentData.currentMissionOptions = nextMissions;

    }

    public void SetOperationFinished()
    {
        currentData.isFinished = true;
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
        }
        else
        {
            Debug.LogWarning("Save file not found at: " + path);
        }
    }
}
