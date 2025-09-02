using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MissionHandler : MonoBehaviour
{
    public Button btn_engage;
    public string activeMissionScene;

    public void Awake()
    {
        btn_engage.interactable = false;
    }

    public void LoadMissionScene()
    {
        Debug.Log("Loading Mission: "+activeMissionScene);
        SceneManager.LoadScene(activeMissionScene);
    }

    public void SetData(MissionTemplate mission, MissionOption options)
    {
        activeMissionScene = mission.GetRandomScene();

        PassthroughGameData dataObj = FindAnyObjectByType<PassthroughGameData>();
        dataObj.riskFactor = options.baseRiskFactor;
        dataObj.intelFactor = options.baseIntelFactor;
        btn_engage.interactable = true;
    }
}