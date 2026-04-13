using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class OperationUILoader : MonoBehaviour
{
    public OperationSaveLoader loader;
    public ScrollToElement scroller;
    public GameObject timelineContentParent;
    public MissionHandler missionHandler;
    public TMP_Text lbl_day;
    public TMP_Text lbl_hearts;
    public TMP_Text lbl_minds;
    //public TMP_Text lbl_funds;
    public TMP_Text lbl_peace;
    public GameObject eventNodePrefab;
    public GameObject currentNodePrefab;
    public GameObject MissionPrefab;
    public GameObject ResultsPrefab;
    public Color sucessColor;
    public Color failureColor;

    [Header("Peace Score Settings")]
    public float hWeight = 1.5f;
    public float mWeight = 0.85f;
    public float dWeight = 10f;

    public void Update()
    {
        if (loader.hasLoaded)
        {
            PopulateUI();
            enabled = false;
        }
    }

    public void PopulateUI()
    {
        lbl_day.text = string.Format("Day {0}/{1}", loader.currentData.operationDay, loader.currentData.operationDuration);
        lbl_hearts.text = string.Format("{0}", loader.currentData.heartsScore);
        lbl_minds.text = string.Format("{0}", loader.currentData.mindsScore);
        //lbl_funds.text = string.Format("{0}", loader.currentData.funds);

        int opDay = loader.currentData.operationDay;
        int opDuration = loader.currentData.operationDuration;

        int hearts = loader.currentData.heartsScore;
        int minds = loader.currentData.mindsScore;

        int peacescore = (int)Mathf.Clamp((((hearts * hWeight) + (minds * mWeight)) / 2) - ((opDay / opDuration) * dWeight), 0, 100);


        lbl_peace.text = string.Format("{0}", peacescore);

        foreach(MissionRecord opEvent in loader.currentData.pastMissions)
        {
            GameObject node = Instantiate(eventNodePrefab, timelineContentParent.transform);
            node.GetComponentInChildren<TMP_Text>().text = string.Format("Day {0}", opEvent.day);
            MissionRecordComponent record = node.AddComponent<MissionRecordComponent>();
            record.ImportData(opEvent);

            switch (opEvent.result)
            {
                case "sucess":
                    {
                            node.GetComponent<Image>().color = sucessColor;
                            break;
                    }
                case "failure":
                    {
                        node.GetComponent<Image>().color = failureColor;
                        break;
                    }
            }

            node.GetComponent<Button>().onClick.AddListener(() => ResultsPrefab.GetComponent<ResultsPanelHandler>().SetPanelData(record));
            node.GetComponent<Button>().onClick.AddListener(() => ResultsPrefab.SetActive(true));
            node.GetComponent<Button>().onClick.AddListener(() => MissionPrefab.SetActive(false));
        }

        GameObject currentNode = Instantiate(currentNodePrefab, timelineContentParent.transform);
        currentNode.GetComponentInChildren<TMP_Text>().text = string.Format("Day {0}", loader.currentData.operationDay);
        currentNode.GetComponent<CurrentEventManager>().SetMissionsOptions(loader.currentData.currentMissionOptions, MissionPrefab, ResultsPrefab, missionHandler);
        currentNode.GetComponentInChildren<Button>().onClick.AddListener(() => MissionPrefab.SetActive(false));
        currentNode.GetComponentInChildren<Button>().onClick.AddListener(() => ResultsPrefab.SetActive(false));

        scroller.targetElement = currentNode.GetComponent<RectTransform>();
    }
}
