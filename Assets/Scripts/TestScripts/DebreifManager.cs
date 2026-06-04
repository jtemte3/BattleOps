using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebreifManager : MonoBehaviour
{
    public MissionStatsTracker missionStatsTracker;
    [Space]
    public GameObject sucessBanner;
    public GameObject failureBanner;
    public TMP_Text lblHeartsScore;
    public TMP_Text lblMindsScore;
    public TMP_Text lblPeaceScore;
    public TMP_Text lblFundsScore;
    public TMP_Text lblSquadDeaths;
    public TMP_Text lblCivilianDeaths;
    public TMP_Text lblBaddieDeaths;

    public void PopulateDebriefScreen()
    {
        missionStatsTracker.generateScoreChanges();
        (string title, bool sucess,int heartsScore,int mindsScore, int funds, int peaceScore, int squadKills, int civKilles, int badKills) = missionStatsTracker.getMissionResults();

        if (sucess)
        {
            sucessBanner.SetActive(true);
            failureBanner.SetActive(false);
        }
        else
        {
            failureBanner.SetActive(true);
            sucessBanner.SetActive(false);
        }

        if (heartsScore >= 0)
        {
            lblHeartsScore.text = "+" + heartsScore.ToString();
        }
        else
        {
            lblHeartsScore.text = "-" + heartsScore.ToString();
        }

        if (mindsScore >= 0)
        {
            lblMindsScore.text = "+" + mindsScore.ToString();
        }
        else
        {
            lblMindsScore.text = "-" + mindsScore.ToString();
        }

        if (peaceScore >= 0)
        {
            lblPeaceScore.text = "+" + peaceScore.ToString();
        }
        else
        {
            lblPeaceScore.text = "-" + peaceScore.ToString();
        }

        lblFundsScore.text = "+" +funds.ToString();
        lblSquadDeaths.text = squadKills.ToString();
        lblCivilianDeaths.text = civKilles.ToString();
        lblBaddieDeaths.text = badKills.ToString();
    }
}
