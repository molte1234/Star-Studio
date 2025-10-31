using UnityEngine;

/// <summary>
/// ScriptableObject for action definitions (Practice, Tour, Record, etc.)
/// Defines costs, time, rewards, and requirements for each action
/// 
/// Design Philosophy:
/// - Base values + per-character modifiers
/// - Stat-based time efficiency (higher skill = faster completion)
/// - Flexible reward system (money, fans, stats, unity, etc.)
/// </summary>
[CreateAssetMenu(fileName = "NewAction", menuName = "Band Manager/Action Data")]
public class ActionData : ScriptableObject
{
    // ============================================
    // BASIC INFO
    // ============================================

    [Header("Basic Info")]
    [Tooltip("Action name shown in UI (e.g., 'Practice', 'Tour', 'Record')")]
    public string actionName;

    [Tooltip("Description of what this action does")]
    [TextArea(3, 5)]
    public string actionDescription;

    // ============================================
    // CHARACTER REQUIREMENTS
    // ============================================

    [Header("Character Requirements")]
    [Tooltip("Does this action require selecting characters? (If false, action starts immediately when clicked)")]
    public bool requiresMembers = true;

    [Tooltip("Minimum number of characters required (only used if requiresMembers = true)")]
    public int minMembers = 1;

    [Tooltip("Maximum number of characters allowed (only used if requiresMembers = true)")]
    public int maxMembers = 6;

    // ============================================
    // COSTS
    // ============================================

    [Header("Costs")]
    [Tooltip("Base cost in money to start this action (paid upfront)")]
    public int baseCost = 0;

    [Tooltip("Additional cost per character selected (paid upfront)")]
    public int costPerCharacter = 0;

    [Tooltip("Unity cost to start this action (paid upfront)")]
    public int unityCost = 0;

    [Tooltip("Morale cost per participating character (paid upfront, reduces each character's morale)")]
    public int moraleCostPerCharacter = 0;

    // ============================================
    // TIME DURATION
    // ============================================

    [Header("Time Duration")]
    [Tooltip("Base time in seconds for this action to complete")]
    public float baseTime = 30f;

    [Tooltip("Which stat affects time efficiency? (Higher stat = faster completion)")]
    public StatType timeEfficiencyStat = StatType.Practical;

    [Tooltip("Time reduction per point of the efficiency stat (in seconds). Set to 0 to disable time modifiers.")]
    public float timeReductionPerStatPoint = 0f;

    [Tooltip("Minimum time allowed (prevents actions from becoming instant)")]
    public float minTime = 5f;

    // ============================================
    // REWARDS (On Completion)
    // ============================================

    [Header("Rewards - Resources")]
    [Tooltip("Base money gained on completion")]
    public int rewardMoney = 0;

    [Tooltip("Money gained per character on completion")]
    public int rewardMoneyPerCharacter = 0;

    [Tooltip("Base fans gained on completion")]
    public int rewardFans = 0;

    [Tooltip("Fans gained per character on completion")]
    public int rewardFansPerCharacter = 0;

    [Tooltip("Unity gained on completion")]
    public int rewardUnity = 0;

    [Header("Rewards - Character Stats")]
    [Tooltip("Which stat(s) should improve for participating characters?")]
    public StatGainType statGainType = StatGainType.None;

    [Tooltip("Amount to increase the stat(s) by (only used if statGainType is not None)")]
    public int statGainAmount = 1;

    [Tooltip("If statGainType = Specific, which stat to improve?")]
    public StatType specificStatToImprove = StatType.Charisma;

    // ============================================
    // BONUSES
    // ============================================

    [Header("Bonuses")]
    [Tooltip("Bonus multiplier if ALL band members participate (e.g., 1.5 = +50% rewards)")]
    public float fullTeamBonusMultiplier = 1f;

    [Tooltip("Should this action improve morale for participating characters?")]
    public bool improveMorale = false;

    [Tooltip("Morale gain amount (if improveMorale = true)")]
    public int moraleGainAmount = 1;
}

// ============================================
// ENUMS
// ============================================

/// <summary>
/// Which stat to use for calculations (time efficiency, rewards, etc.)
/// </summary>
public enum StatType
{
    Charisma,
    StagePerformance,
    Vocal,
    Instrument,
    Songwriting,
    Production,
    Management,
    Practical
}

/// <summary>
/// What kind of stat improvement does this action provide?
/// </summary>
public enum StatGainType
{
    None,               // No stat improvement
    Specific,           // Improve one specific stat (set in specificStatToImprove)
    Random,             // Improve one random stat
    AllStatsSmall       // Improve all 8 stats by a small amount
}