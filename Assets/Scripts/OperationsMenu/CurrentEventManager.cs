using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CurrentEventManager : MonoBehaviour
{
    public List<MissionTemplate> missionTemplates = new();
    public List<GameObject> eventOptions = new();

    public void SetMissionsOptions(List<MissionOption> missions, GameObject missionPanel, GameObject resultsPanel, MissionHandler missionHandler)
    {
        eventOptions.ForEach(node => {
            node.SetActive(false);
            node.GetComponent<Button>().onClick.RemoveAllListeners();
        });

        int optionCount = missions.Count;

        List<MissionTemplate> nextMissions = new();
        foreach (MissionTemplate missionTemplate in missionTemplates)
        {
            for (int i = 0; i < missions.Count; i++)
            {
                if (missionTemplate.missionId == missions[i].missionId)
                {
                    nextMissions.Add(missionTemplate);
                }
            }
        }

        for (int i = 0; i < optionCount; i++)
        {
            eventOptions[i].SetActive(true);
            List<Image> images = eventOptions[i].GetComponentsInChildren<Image>().ToList();
            images[1].sprite = nextMissions[i].icon;
            MissionTemplate mission = nextMissions[i];
            MissionOption missionDetails = missions[i];
            //Set missionPanel details here
            eventOptions[i].GetComponent<Button>().onClick.AddListener(() => missionPanel.GetComponent<MisssionPanelHandler>().SetPanelDetails(mission, missionDetails));
            eventOptions[i].GetComponent<Button>().onClick.AddListener(() => missionPanel.SetActive(true));
            eventOptions[i].GetComponent<Button>().onClick.AddListener(() => resultsPanel.SetActive(false));
            eventOptions[i].GetComponent<Button>().onClick.AddListener(() => missionHandler.SetData(mission, missionDetails));
        }
    }
}
