using UnityEngine;

[CreateAssetMenu(fileName = "NewSlot", menuName = "Band Manager/Slot Data")]
public class SlotData : ScriptableObject
{
    [Header("Basic Info")]
    [Tooltip("Stage name / Artist name / Item name (what players see in game)")]
    public string displayName;

    [Tooltip("Real name (for future use)")]
    public string realName;

    public Sprite sprite;
    public SlotType slotType;

    [Header("Core Stats")]
    [Range(0, 10)]
    [Tooltip("Social skills, looks, fan appeal")]
    public int charisma = 0;

    [Range(0, 10)]
    [Tooltip("Live show entertainment, stage presence")]
    public int stagePerformance = 0;

    [Range(0, 10)]
    [Tooltip("Singing ability")]
    public int vocal = 0;

    [Range(0, 10)]
    [Tooltip("Playing instrument skill")]
    public int instrument = 0;

    [Range(0, 10)]
    [Tooltip("Creating music, composing")]
    public int songwriting = 0;

    [Range(0, 10)]
    [Tooltip("Studio work, technical skills")]
    public int production = 0;

    [Range(0, 10)]
    [Tooltip("Business, organization")]
    public int management = 0;

    [Range(0, 10)]
    [Tooltip("General utility, getting stuff done")]
    public int practical = 0;

    [Header("Character State")]
    [Range(0, 10)]
    [Tooltip("Current morale (0-10) - only for Person type")]
    public int morale = 5;

    [Tooltip("One-liner personality/trait description")]
    public string trait;

    [Header("Costs")]
    [Tooltip("Upkeep cost per quarter - paid automatically each quarter")]
    public int upkeepCost = 0;

    [Tooltip("One-time hire/purchase cost")]
    public int hireCost = 0;

    [Header("Description")]
    [TextArea(3, 6)]
    [Tooltip("Character biography or item description")]
    public string description;
}

public enum SlotType
{
    Person,
    Item
}