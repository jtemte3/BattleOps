using System.IO;
using UnityEngine;

public class OperationSaveLoader : MonoBehaviour
{
    public string saveFileName = "OperationSave.json";
    public bool hasLoaded = false;
    public OperationSaveData currentData;
    

    void Awake()
    {
        LoadSaveFile();
        hasLoaded = true;
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
}
