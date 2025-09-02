using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MisssionPanelHandler : MonoBehaviour
{
    public TMP_Text lbl_missionTitle;
    public TMP_Text lbl_missionDescription;
    public TMP_Text lbl_expectedInsurgents;
    public TMP_Text lbl_expectedCivilians;
    public TMP_Text lbl_risklevel;
    public TMP_Text lbl_intelLevel;
    public TMP_Text lbl_baseHearts;
    public TMP_Text lbl_baseMinds;
    public TMP_Text lbl_basePeace;
    public void SetPanelDetails(MissionTemplate mission, MissionOption missionDetails)
    {
        lbl_missionTitle.text = mission.missionTitle;
        lbl_missionDescription.text = mission.missionDescription;

        lbl_expectedInsurgents.text = string.Format("Expected Insurgents: {0}", mission.expectedCombatants);
        lbl_expectedCivilians.text = string.Format("Civilian Present: {0}", mission.civsAllowed);
        lbl_risklevel.text = string.Format("Risk Level: {0}", missionDetails.baseRiskFactor);
        lbl_intelLevel.text = string.Format("Intel Level: {0}", missionDetails.baseIntelFactor);

        lbl_baseHearts.text = string.Format("{0}", mission.baseHeartsChange);
        lbl_baseMinds.text = string.Format("{0}", mission.baseMindsChange);
        lbl_basePeace.text = string.Format("{0}", mission.basePeaceChange);
    }
}
