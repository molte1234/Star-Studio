using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// ScriptableObject that defines an event - story moments, choices, consequences
/// Events can have multiple conditions - ALL must be met for event to trigger
/// </summary>
[CreateAssetMenu(fileName = "NewEvent", menuName = "Band Manager/Event Data")]
public class EventData : ScriptableObject
{
    [Header("Trigger Conditions - ALL must be true")]
    [InfoBox("Event will trigger when ALL enabled conditions are met. Disable checkboxes for conditions you don't need.")]

    [FoldoutGroup("Time Condition")]
    [ToggleLeft]
    [Tooltip("Enable to require specific year and quarter")]
    public bool requireYearQuarter = false;

    [FoldoutGroup("Time Condition")]
    [ShowIf("requireYearQuarter")]
    [Range(1, 20)]
    [Tooltip("Year when event should trigger (1-20)")]
    public int triggerYear = 1;

    [FoldoutGroup("Time Condition")]
    [ShowIf("requireYearQuarter")]
    [Range(1, 4)]
    [Tooltip("Quarter when event should trigger (1=Q1, 2=Q2, 3=Q3, 4=Q4)")]
    public int triggerQuarter = 1;

    [FoldoutGroup("Flag Condition")]
    [ToggleLeft]
    [Tooltip("Enable to require a story flag")]
    public bool requireFlag = false;

    [FoldoutGroup("Flag Condition")]
    [ShowIf("requireFlag")]
    [Tooltip("Story flag that must be set for this event to trigger")]
    public string requiredFlag;

    [FoldoutGroup("Random Chance")]
    [Range(0f, 100f)]
    [Tooltip("Chance this event triggers when conditions are met (100% = always, 0% = never)")]
    public float randomChance = 100f;

    [FoldoutGroup("Stat Condition")]
    [ToggleLeft]
    [Tooltip("Enable to require specific stat values")]
    public bool requireStat = false;

    [FoldoutGroup("Stat Condition")]
    [ShowIf("requireStat")]
    [EnumToggleButtons]
    public StatToCheck statToCheck;

    [FoldoutGroup("Stat Condition")]
    [ShowIf("requireStat")]
    [EnumToggleButtons]
    public ComparisonType comparison;

    [FoldoutGroup("Stat Condition")]
    [ShowIf("requireStat")]
    [Tooltip("Value to compare against")]
    public int statValue;

    [Header("Event Content")]
    [Title("Visual & Text", bold: false)]
    public string eventTitle;

    [TextArea(3, 6)]
    public string eventDescription;

    [PreviewField(80, ObjectFieldAlignment.Left)]
    public Sprite eventSprite;

    [Header("Audio (Optional)")]
    [Tooltip("Custom music to play during this event. Leave empty to keep current music.")]
    public AudioClip eventMusic;

    [Tooltip("SFX to play when event pops up. Leave empty for default popup sound.")]
    public AudioClip eventPopupSFX;

    [Header("Choices")]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "choiceText")]
    public ChoiceData[] choices; // 2-3 choices
}

/// <summary>
/// Which stat to check for stat-based triggers
/// </summary>
public enum StatToCheck
{
    Money,
    Fans,
    Technical,
    Performance,
    Charisma,
    Unity
}

/// <summary>
/// How to compare the stat value
/// </summary>
public enum ComparisonType
{
    GreaterThan,      // >
    LessThan,         // <
    EqualTo,          // ==
    GreaterOrEqual,   // >=
    LessOrEqual       // <=
}