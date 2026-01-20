using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "NewMissionTemplate", menuName = "Operations/Mission Template")]
public class MissionTemplate : ScriptableObject
{
    [Header("Basic Info")]
    public string missionId;
    public string missionTitle;
    public Sprite icon;
    [TextArea(2, 5)]
    public string missionDescription;
    public List<string> scenes;

    [Header("Impact Estimates")]
    public int baseHeartsChange;
    public int baseMindsChange;
    //public int estimatedFundsChange; Funds increase should be based on a formula based on mission stats
    public int basePeaceChange;

    [Header("Gameplay Info")]
    /*public RiskLevel risk;   //Should be randomly generated variables that effect mission
    public IntelLevel intel;*/

    /*
     * Risk Level manages the range of enemies and challenges that appear on the map
     * Intel Level manages the variance of expected unit counts and the ratio of enemy spawns?
     */
    [Tooltip("Non Mission combatants")]
    public int expectedCombatants;
    public int expectedTechnicals;
    public bool civsAllowed;
    public int expectedCivilians;
    //public MissionCategory category;

    public string GetRandomScene()
    {
        return scenes[Random.Range(0, scenes.Count -1)];
    }
}