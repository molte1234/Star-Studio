using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// ScriptableObject that defines a single story event
/// Create instances via: Assets > Create > Band Manager > Event Data
/// </summary>
[CreateAssetMenu(fileName = "NewEvent", menuName = "Band Manager/Event Data")]
public class EventData : ScriptableObject
{
    [Header("Event Info")]
    [TextArea(2, 4)]
    public string eventTitle;

    [TextArea(4, 10)]
    public string eventDescription;

    [Header("Optional Visual")]
    [Tooltip("Optional sprite/illustration for this event (shows in EventPanel)")]
    public Sprite eventSprite;

    [Header("Trigger Conditions")]
    [Tooltip("Enable to require specific year/quarter")]
    public bool requireYearQuarter = false;

    [ShowIf("requireYearQuarter")]
    [Range(1, 10)]
    public int triggerYear = 1;

    [ShowIf("requireYearQuarter")]
    [Range(1, 4)]
    public int triggerQuarter = 1;

    [Space(10)]
    [Tooltip("Enable to require a specific story flag")]
    public bool requireFlag = false;

    [ShowIf("requireFlag")]
    public string requiredFlag;

    [Space(10)]
    [Tooltip("Enable to require a stat threshold (e.g. Fans > 100)")]
    public bool requireStat = false;

    [ShowIf("requireStat")]
    public StatToCheck statToCheck;

    [ShowIf("requireStat")]
    public ComparisonType comparison;

    [ShowIf("requireStat")]
    public int statValue;

    [Space(10)]
    [Range(0f, 100f)]
    [Tooltip("Random chance this event triggers (0-100%). Even if all conditions met, this is checked.")]
    public float randomChance = 100f;

    [Header("Audio")]
    [Tooltip("Music to play during this event. Leave empty to keep current music.")]
    public AudioClip eventMusic;

    [Tooltip("SFX to play when event pops up. Leave empty for default popup sound.")]
    public AudioClip eventPopupSFX;

    [Header("Choices")]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "choiceText")]
    public ChoiceData[] choices; // 2-3 choices
}

/// <summary>
/// ✅ UPDATED: Which stat to check for stat-based triggers
/// Unity stat completely removed from the enum
/// </summary>
public enum StatToCheck
{
    Money,
    Fans,
    // 8-STAT SYSTEM (Unity removed):
    Charisma,        // Social, look, fan appeal
    StagePerformance, // Live show entertainment
    Vocal,           // Singing ability
    Instrument,      // Playing instrument
    Songwriting,     // Creating music
    Production,      // Studio/technical skills
    Management,      // Business/organization
    Practical        // General utility/getting stuff done
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