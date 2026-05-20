using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public string DefaultFileName = "OperationSave - Start.json";

    public GameObject btn_start;
    public GameObject btn_continue;

    public TMP_InputField inp_first;
    public TMP_InputField inp_second;

    void Start()
    {
        bool hasUnfinishedSave = false;

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
                    hasUnfinishedSave = true;
                    break;
                }
            }
            catch
            {
                // Ignore invalid or corrupted json files
                Debug.LogWarning("Failed to load save file: " + file);
            }
        }

        btn_start.SetActive(!hasUnfinishedSave);
        btn_continue.SetActive(hasUnfinishedSave);
    }

    public void LoadScene(int sceneNumber)
    {
        SceneManager.LoadScene(sceneNumber);
    }
    public void ExitApplication()
    {
        Application.Quit();
    }

    public void CreateNewOperation()
    {
        string operationName = inp_first.text + " " + inp_second.text;
        CreateNewOperationFile(operationName);
    }

    public void GetRandomOpFirstWord()
    {
        inp_first.text = OperationWords.GetRandomFirstWord();
    }
    public void GetRandomOpSecondWord()
    {
        inp_second.text = OperationWords.GetRandomSecondWord();
    }

    public void CreateNewOperationFile( string operationName)
    {

        string defaultFilepath = Path.Combine(Application.streamingAssetsPath,"Default", DefaultFileName);

        OperationSaveData newOperationData = new();

        if (File.Exists(defaultFilepath))
        {
            string jsonString = File.ReadAllText(defaultFilepath);
            newOperationData = JsonUtility.FromJson<OperationSaveData>(jsonString);
        }

        newOperationData.operationName = operationName;

        newOperationData.lastSaveDate = DateTime.Now.Day + "/" + DateTime.Now.Month + "/" + DateTime.Now.Year;

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

        newOperationData.lastSaveTime = hour + ":" + DateTime.Now.Minute + " " + meridiem;

        string json = JsonUtility.ToJson(newOperationData);

        string operationFileName = operationName + "-" + Guid.NewGuid().ToString() + ".json";
        string path = Path.Combine(Application.streamingAssetsPath, operationFileName);

        File.WriteAllText(path, json);

        Debug.Log("New Operation Save Written: " + path);
    }
}
