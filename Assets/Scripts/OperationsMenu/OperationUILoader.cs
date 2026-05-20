using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OperationUILoader : MonoBehaviour
{
    [Header("References")]
    public OperationSaveHandler dataHandler;
    public ScoreWeights scoreWeights;
    public ScrollToElement scroller;
    public GameObject timelineContentParent;
    public MissionHandler missionHandler;

    [Header("Labels")]
    public TMP_Text lbl_day;
    public TMP_Text lbl_hearts;
    public TMP_Text lbl_minds;
    public TMP_Text lbl_funds;
    public TMP_Text lbl_peace;
    public TMP_Text lbl_operationName;

    [Header("Timeline")]
    public GameObject eventNodePrefab;
    public GameObject currentNodePrefab;

    [Header("Panels")]
    public GameObject MissionPrefab;
    public GameObject ResultsPrefab;
    public GameObject backgroundDimmer;

    [Header("Operation End UI")]
    public GameObject operationEndPanel;
    public TMP_Text lbl_OperationFinishTitle;
    public TMP_Text lbl_OperationEndDescription;
    public Button btn_finishOperation;
    public Button btn_extendOperation;

    [Header("Operation Settings")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Colors")]
    public Color successColor;
    public Color failureColor;
    public Color opSuccessColor;
    public Color opFailureColor;

    public void Update()
    {
        if (dataHandler.hasLoaded)
        {
            PopulateUI();
            enabled = false;
        }
    }

    public void PopulateUI()
    {
        lbl_day.text = string.Format("Day {0}/{1}", dataHandler.currentData.operationDay, dataHandler.currentData.operationDuration);
        lbl_hearts.text = string.Format("{0}", dataHandler.currentData.heartsScore);
        lbl_minds.text = string.Format("{0}", dataHandler.currentData.mindsScore);
        lbl_funds.text = string.Format("{0:n0}", dataHandler.currentData.funds);
        lbl_operationName.text = string.Format("Operation: {0}", dataHandler.currentData.operationName);

        int opDay = dataHandler.currentData.operationDay;
        int opDuration = dataHandler.currentData.operationDuration;

        int hearts = dataHandler.currentData.heartsScore;
        int minds = dataHandler.currentData.mindsScore;

        int peacescore = CalculatePeaceScore();


        lbl_peace.text = string.Format("{0}", peacescore);

        foreach (MissionRecord opEvent in dataHandler.currentData.pastMissions)
        {
            GameObject node = Instantiate(eventNodePrefab, timelineContentParent.transform);
            node.GetComponentInChildren<TMP_Text>().text = string.Format("Day {0}", opEvent.day);

            MissionRecordComponent record = node.AddComponent<MissionRecordComponent>();
            record.ImportData(opEvent);

            switch (opEvent.result)
            {
                case "sucess":
                    {
                        node.GetComponent<Image>().color = successColor;
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

        //If operation is complete, do not generate another current mission node.
        if (dataHandler.currentData.operationDay > dataHandler.currentData.operationDuration)
        {
            ShowOperationEndPanel(peacescore);
            return;
        }

        GameObject currentNode = Instantiate(currentNodePrefab, timelineContentParent.transform);
        currentNode.GetComponentInChildren<TMP_Text>().text = string.Format("Day {0}", dataHandler.currentData.operationDay);

        currentNode.GetComponent<CurrentEventManager>().SetMissionsOptions(
            dataHandler.currentData.currentMissionOptions,
            MissionPrefab,
            ResultsPrefab,
            missionHandler
        );

        currentNode.GetComponentInChildren<Button>().onClick.AddListener(() => MissionPrefab.SetActive(false));
        currentNode.GetComponentInChildren<Button>().onClick.AddListener(() => ResultsPrefab.SetActive(false));

        scroller.targetElement = currentNode.GetComponent<RectTransform>();
    }

    public int CalculatePeaceScore()
    {
        int opDay = dataHandler.currentData.operationDay;
        int opDuration = dataHandler.currentData.operationDuration;

        int hearts = dataHandler.currentData.heartsScore;
        int minds = dataHandler.currentData.mindsScore;

        return (int)Mathf.Clamp(
            (((hearts * scoreWeights.hearts) + (minds * scoreWeights.minds)) / 2f)
            - (((float)opDay / (float)opDuration) * scoreWeights.day),
            0,
            100
        );
    }

    public int CalculatePeaceScoreManually(int m_opDay, int m_opDuration, int m_hearts, int m_minds)
    {
        return (int)Mathf.Clamp(
            (((m_hearts * scoreWeights.hearts) + (m_minds * scoreWeights.minds)) / 2f)
            - (((float)m_opDay / (float)m_opDuration) * scoreWeights.day),
            0,
            100
        );
    }

    public void ShowOperationEndPanel(int peaceScore)
    {
        operationEndPanel.SetActive(true);
        backgroundDimmer.SetActive(true);

        bool operationSuccess = peaceScore >= OperationParams.requiredPeaceScore;

        if (operationSuccess)
        {
            lbl_OperationFinishTitle.text = "Operation Successful";
            lbl_OperationEndDescription.text =
                "You generated enough peace to stabilize the region. You may finish the operation or continue to improve the situation.";

            lbl_OperationFinishTitle.color = opSuccessColor;
            btn_finishOperation.image.color = successColor;
        }
        else
        {
            lbl_OperationFinishTitle.text = "Operation Failed";
            lbl_OperationEndDescription.text =
                "The operation ended before enough peace was achieved. You may finish the operation or extend the deployment by one week.";
            lbl_OperationFinishTitle.color = opFailureColor;
            btn_finishOperation.image.color = failureColor;
        }

        btn_finishOperation.onClick.RemoveAllListeners();
        btn_extendOperation.onClick.RemoveAllListeners();

        btn_finishOperation.onClick.AddListener(FinishOperation);
        btn_extendOperation.onClick.AddListener(ExtendOperation);
    }

    public void FinishOperation()
    {
        dataHandler.SetOperationFinished();
        dataHandler.WriteSaveFile();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void ExtendOperation()
    {
        dataHandler.currentData.operationDuration += OperationParams.operationExtensionDays;
        dataHandler.currentData.mindsScore -= OperationParams.operationExtensionMindPenalty;

        //Prevent negative values.
        dataHandler.currentData.mindsScore = Mathf.Max(dataHandler.currentData.mindsScore, 0);

        operationEndPanel.SetActive(false);
        backgroundDimmer.SetActive(false);

        //Generate another set of missions for the extended operation.
        dataHandler.GenerateNewMissions();

        //Clear old timeline nodes.
        foreach (Transform child in timelineContentParent.transform)
        {
            Destroy(child.gameObject);
        }

        PopulateUI();
    }
}