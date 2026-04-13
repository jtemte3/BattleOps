using UnityEngine;
using static UnityEngine.InputManagerEntry;
using static UnityEngine.Rendering.HDROutputUtils;

public class MissionStatsTracker : MonoBehaviour
{
    public MissionTemplate missionTemplate;
    public SaveDataHandler saveDataHandler;
    [Space]
    bool isMissionCompleted;
    bool hasExtracted;

    [Space]
    int baddiesKilled = 0;
    int civiliansKilled = 0;
    int squadKilled = 0;

    [Space]
    int heartsChange = 0; //Local Opinion
    int mindsChange = 0; //International Opinion
    int fundsChange = 0; //Money
    int peaceScoreChange = 0;

    [Space]
    [Header("Peace Score Weights")]
    [Header("Peace Score Settings")]
    public float hWeight = 1.5f;
    public float mWeight = 0.85f;
    public float dWeight = 10f;

    public void generateScoreChanges()
    {
        if (isMissionCompleted)
        {
            heartsChange = missionTemplate.baseHeartsChange;
            mindsChange = missionTemplate.baseMindsChange;
            fundsChange = 500;
        }
        
        if (hasExtracted)
        {
            mindsChange -= squadKilled;
            fundsChange += 500;
        }
        else
        {
            mindsChange -= squadKilled;
        }

        if (civiliansKilled > 0)
        {
            mindsChange -= Mathf.RoundToInt(civiliansKilled * .25f);
            heartsChange -= Mathf.RoundToInt(civiliansKilled * .5f);
        }
        else
        {
            heartsChange += 1;
            mindsChange += 1;
        }

        if (baddiesKilled >= missionTemplate.expectedCombatants)
        {
            heartsChange += 5;
            fundsChange += 500;
        }

        if (mindsChange > 0)
        {
            fundsChange += (mindsChange * 50) + Random.Range(0,9);
        }
        OperationSaveData currentData = saveDataHandler.currentData;

        int newPeaceScore = (int)Mathf.Clamp(((((currentData.heartsScore + heartsChange) * hWeight) + ((currentData.mindsScore + mindsChange) * mWeight)) / 2) - ((currentData.operationDay / currentData.operationDuration) * dWeight), 0, 100);
        peaceScoreChange = newPeaceScore - currentData.peaceScore;
    }

    public (string, bool, int, int, int, int, int, int, int) getMissionResults()
    {
        return (missionTemplate.missionTitle, isMissionCompleted, heartsChange, mindsChange, fundsChange, peaceScoreChange, squadKilled, civiliansKilled, baddiesKilled);
    }

    public void SetMissionComplete()
    {
        isMissionCompleted = true;
    }

    public void SetHasExtracted()
    {
        hasExtracted = true;
    }

    public void IncrementBaddieDeath()
    {
        baddiesKilled++;
    }

    public void IncrementCivilianDeath()
    {
        civiliansKilled++;
    }

    public void IncrementSquadKilled()
    {
        squadKilled++;
    }
}
