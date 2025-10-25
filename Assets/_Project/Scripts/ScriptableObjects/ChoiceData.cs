using UnityEngine;

[System.Serializable]
public class ChoiceData
{
    public string choiceText;

    [Header("Effects")]
    public int moneyChange;
    public int fansChange;
    public int unityChange;
    public int technicalChange;
    public int performanceChange;
    public int charismaChange;

    [Header("Story Flags")]
    public string[] flagsToAdd;

    [TextArea(2, 4)]
    public string outcomeText; // Optional feedback
}