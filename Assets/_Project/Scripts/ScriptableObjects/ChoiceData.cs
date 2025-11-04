using UnityEngine;

/// <summary>
/// ✅ UPDATED: Data for a single choice in an event
/// Unity stat completely removed from the system
/// </summary>
[System.Serializable]
public class ChoiceData
{
    public string choiceText;

    [Header("Resource Effects")]
    public int moneyChange;
    public int fansChange;
    // REMOVED: unityChange (no longer in the game)

    [Header("Stat Effects - 8-Stat System")]
    public int charismaChange;
    public int stagePerformanceChange;
    public int vocalChange;
    public int instrumentChange;
    public int songwritingChange;
    public int productionChange;
    public int managementChange;
    public int practicalChange;

    [Header("Story Flags")]
    public string[] flagsToAdd;

    [TextArea(2, 4)]
    public string outcomeText; // Optional feedback
}