using UnityEngine;

[CreateAssetMenu(fileName = "NewEvent", menuName = "Band Manager/Event Data")]
public class EventData : ScriptableObject
{
    [Header("Trigger Settings")]
    public TriggerType triggerType;
    public int triggerQuarter; // If TriggerType.Quarter
    public string requiredFlag; // If TriggerType.Flag

    [Header("Event Content")]
    public string eventTitle;
    [TextArea(3, 6)]
    public string eventDescription;
    public Sprite eventSprite;

    [Header("Choices")]
    public ChoiceData[] choices; // 2-3 choices
}

public enum TriggerType
{
    Random,
    Quarter,
    Flag,
    Stat
}