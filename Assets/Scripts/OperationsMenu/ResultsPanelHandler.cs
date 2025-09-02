using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResultsPanelHandler : MonoBehaviour
{
    public TMP_Text lblEventTitle;
    public TMP_Text lblResult;
    public TMP_Text lblHeartsChange;
    public TMP_Text lblMindsChange;
    public TMP_Text lblFundsChange;
    public TMP_Text lblPeaceChange;
    public TMP_Text lblGoodCasualties;
    public TMP_Text lblCivCasualties;
    public TMP_Text lblBadCasualties;

    public void SetPanelData(MissionRecordComponent record)
    {
        lblEventTitle.text = string.Format("{0}", record.title);
        lblResult.text = string.Format("Result: {0}", record.result);
        lblHeartsChange.text = string.Format("{0}", record.heartsChange);
        lblMindsChange.text = string.Format("{0}", record.mindsChange);
        lblFundsChange.text = string.Format("{0}", record.fundsChange);
        lblPeaceChange.text = string.Format("{0}", record.peaceChange);
        lblGoodCasualties.text = string.Format("TaskForce Casualties: {0}", record.goodCasualties);
        lblCivCasualties.text = string.Format("Civilian Casualties: {0}", record.civCasualties);
        lblBadCasualties.text = string.Format("Insurgent Casualties: {0}", record.badCasualties);
    }
}
