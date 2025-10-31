using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

/// <summary>
/// Manages all character actions (Practice, Tour, Record, etc.)
/// Handles starting actions, updating timers, and completing/canceling actions
/// 
/// WHY SEPARATE FROM GAMEMANAGER:
/// - GameManager holds GAME STATE (money, fans, character slots)
/// - ActionManager handles ACTION LOGIC (timers, costs, rewards)
/// - Keeps code organized and easier to understand
/// </summary>
public class ActionManager : MonoBehaviour
{
    // ============================================
    // SINGLETON
    // ============================================

    public static ActionManager Instance { get; private set; }

    private void Awake()
    {
        // Why: Singleton pattern - only one ActionManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // No DontDestroyOnLoad - ActionManager only exists in Game scene

        Debug.Log("✅ ActionManager initialized");
    }

    // ============================================
    // ODIN INSPECTOR - VISUAL DEBUG VIEW
    // ============================================

    [Title("Action Status Monitor", bold: true, horizontalLine: true)]
    [InfoBox("Live view of all character actions. Updates in real-time during play mode.", InfoMessageType.Info)]
    [PropertySpace(10)]
    [ShowInInspector, ReadOnly, PropertyOrder(-1)]
    [ListDrawerSettings(HideAddButton = true, HideRemoveButton = true, DraggableItems = false, ShowIndexLabels = true)]
    private List<CharacterActionDebugView> CharacterActionStates
    {
        get
        {
            List<CharacterActionDebugView> debugViews = new List<CharacterActionDebugView>();

            if (!Application.isPlaying || GameManager.Instance == null)
            {
                return debugViews; // Return empty list in edit mode
            }

            GameManager gm = GameManager.Instance;

            for (int i = 0; i < gm.characterStates.Length; i++)
            {
                CharacterSlotState charState = gm.characterStates[i];

                if (charState == null || charState.slotData == null)
                {
                    debugViews.Add(new CharacterActionDebugView
                    {
                        slotIndex = i,
                        characterName = "--- EMPTY ---",
                        status = "Empty Slot"
                    });
                }
                else
                {
                    debugViews.Add(new CharacterActionDebugView
                    {
                        slotIndex = i,
                        characterName = charState.slotData.displayName,
                        status = charState.isBusy ? "🔒 BUSY" : "✅ Available",
                        currentAction = charState.isBusy ? charState.currentAction.actionName : "---",
                        timeRemaining = charState.isBusy ? charState.actionTimeRemaining : 0f,
                        progress = charState.isBusy && charState.currentAction != null
                            ? 1f - (charState.actionTimeRemaining / charState.currentAction.baseTime)
                            : 0f,
                        groupedWith = charState.isBusy ? string.Join(", ", charState.groupedWithSlots) : "---"
                    });
                }
            }

            return debugViews;
        }
    }

    /// <summary>
    /// Helper class for Odin Inspector display
    /// Shows each character's action state in a nice readable format
    /// </summary>
    [System.Serializable]
    private class CharacterActionDebugView
    {
        [HideLabel, DisplayAsString, HorizontalGroup("Row", 60)]
        public int slotIndex;

        [HideLabel, DisplayAsString, HorizontalGroup("Row", 150)]
        public string characterName;

        [HideLabel, DisplayAsString, HorizontalGroup("Row", 100)]
        public string status;

        [HideLabel, DisplayAsString, HorizontalGroup("Row", 150)]
        public string currentAction;

        [HideLabel, ProgressBar(0, 1, ColorGetter = "GetProgressColor"), HorizontalGroup("Row", 150)]
        public float progress;

        [HideLabel, DisplayAsString, HorizontalGroup("Row", 80)]
        [PropertyTooltip("Time remaining in seconds")]
        public float timeRemaining;

        [HideLabel, DisplayAsString, HorizontalGroup("Row")]
        [PropertyTooltip("Slot indices of other characters in this action group")]
        public string groupedWith;

        // Color getter for progress bar
        private Color GetProgressColor()
        {
            if (progress <= 0f) return Color.gray;
            if (progress < 0.5f) return Color.yellow;
            if (progress < 0.9f) return Color.cyan;
            return Color.green;
        }
    }

    [Title("Action Statistics", bold: true)]
    [PropertySpace(10)]
    [ShowInInspector, ReadOnly, DisplayAsString]
    private string ActionStats
    {
        get
        {
            if (!Application.isPlaying || GameManager.Instance == null)
            {
                return "Play mode required";
            }

            GameManager gm = GameManager.Instance;
            int busyCount = 0;
            int availableCount = 0;
            int emptySlots = 0;

            for (int i = 0; i < gm.characterStates.Length; i++)
            {
                CharacterSlotState charState = gm.characterStates[i];

                if (charState == null || charState.slotData == null)
                {
                    emptySlots++;
                }
                else if (charState.isBusy)
                {
                    busyCount++;
                }
                else
                {
                    availableCount++;
                }
            }

            return $"🔒 Busy: {busyCount}  |  ✅ Available: {availableCount}  |  ⬜ Empty: {emptySlots}";
        }
    }

    // ============================================
    // UPDATE - TICK DOWN ALL ACTIVE TIMERS
    // ============================================

    private void Update()
    {
        // Why: Every frame, count down all active action timers
        UpdateAllActions();
    }

    private void UpdateAllActions()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        // Check each character slot for active actions
        for (int i = 0; i < gm.characterStates.Length; i++)
        {
            CharacterSlotState charState = gm.characterStates[i];

            // Skip empty slots or characters not doing anything
            if (charState == null || !charState.isBusy) continue;

            // Count down the timer
            charState.actionTimeRemaining -= Time.deltaTime;

            // Action complete?
            if (charState.actionTimeRemaining <= 0f)
            {
                CompleteAction(i);
            }
        }

        // Why: Refresh UI every frame to update timers on portraits
        if (gm.uiController != null)
        {
            gm.uiController.RefreshUI();
        }
    }

    // ============================================
    // START ACTION
    // ============================================

    /// <summary>
    /// Start an action for selected characters
    /// </summary>
    /// <param name="action">The ActionData defining the action</param>
    /// <param name="selectedCharacterIndices">List of character slot indices (0-5)</param>
    public void StartAction(ActionData action, List<int> selectedCharacterIndices)
    {
        GameManager gm = GameManager.Instance;
        if (gm == null)
        {
            Debug.LogError("❌ ActionManager: GameManager is null!");
            return;
        }

        Debug.Log($"🎬 Starting action: {action.actionName} with {selectedCharacterIndices.Count} characters");

        // ============================================
        // VALIDATE
        // ============================================

        // Check minimum/maximum members
        if (selectedCharacterIndices.Count < action.minMembers)
        {
            Debug.LogWarning($"⚠️ Not enough members! Need at least {action.minMembers}");
            // TODO: Show error message to player
            return;
        }

        if (action.maxMembers > 0 && selectedCharacterIndices.Count > action.maxMembers)
        {
            Debug.LogWarning($"⚠️ Too many members! Maximum {action.maxMembers}");
            // TODO: Show error message to player
            return;
        }

        // Check if all selected characters are available
        foreach (int index in selectedCharacterIndices)
        {
            CharacterSlotState charState = gm.characterStates[index];
            if (charState == null || !charState.IsAvailable())
            {
                Debug.LogWarning($"⚠️ Character at slot {index} is not available!");
                // TODO: Show error message to player
                return;
            }
        }

        // ============================================
        // CALCULATE COSTS
        // ============================================

        int totalMoneyCost = action.baseCost + (action.costPerCharacter * selectedCharacterIndices.Count);
        int totalUnityCost = action.unityCost;
        int totalMoraleCost = action.moraleCostPerCharacter * selectedCharacterIndices.Count;

        // Check if player can afford it
        if (gm.money < totalMoneyCost)
        {
            Debug.LogWarning($"⚠️ Not enough money! Need ${totalMoneyCost}, have ${gm.money}");
            // TODO: Show error message to player
            return;
        }

        if (gm.unity < totalUnityCost)
        {
            Debug.LogWarning($"⚠️ Not enough unity! Need {totalUnityCost}, have {gm.unity}");
            // TODO: Show error message to player
            return;
        }

        // ============================================
        // CALCULATE TIME DURATION
        // ============================================

        float actionDuration = CalculateActionDuration(action, selectedCharacterIndices);

        // ============================================
        // DEDUCT COSTS
        // ============================================

        gm.money -= totalMoneyCost;
        gm.unity -= totalUnityCost;

        // Deduct morale from each participating character
        foreach (int index in selectedCharacterIndices)
        {
            CharacterSlotState charState = gm.characterStates[index];
            if (charState != null && charState.slotData != null)
            {
                charState.slotData.morale -= action.moraleCostPerCharacter;
                charState.slotData.morale = Mathf.Max(0, charState.slotData.morale); // Don't go below 0
            }
        }

        Debug.Log($"💰 Costs deducted: ${totalMoneyCost} money, {totalUnityCost} unity, {totalMoraleCost} morale");

        // ============================================
        // LOCK CHARACTERS & START ACTION
        // ============================================

        foreach (int index in selectedCharacterIndices)
        {
            CharacterSlotState charState = gm.characterStates[index];
            if (charState != null)
            {
                charState.StartAction(action, actionDuration, selectedCharacterIndices);
                Debug.Log($"   🔒 Locked character slot {index} for {actionDuration:F1} seconds");
            }
        }

        // ============================================
        // REFRESH UI
        // ============================================

        if (gm.uiController != null)
        {
            gm.uiController.RefreshUI();
        }

        Debug.Log($"✅ Action started successfully!");
    }

    // ============================================
    // CALCULATE ACTION DURATION
    // ============================================

    private float CalculateActionDuration(ActionData action, List<int> selectedCharacterIndices)
    {
        // Why: Start with base time
        float duration = action.baseTime;

        // Why: If time reduction is enabled, find highest relevant stat
        if (action.timeReductionPerStatPoint > 0f)
        {
            int highestStat = GetHighestStatInGroup(action.timeEfficiencyStat, selectedCharacterIndices);
            float timeReduction = highestStat * action.timeReductionPerStatPoint;
            duration -= timeReduction;

            Debug.Log($"⏱️ Time calculation: {action.baseTime}s base - ({highestStat} × {action.timeReductionPerStatPoint}s) = {duration}s");
        }

        // Why: Enforce minimum time
        duration = Mathf.Max(duration, action.minTime);

        return duration;
    }

    private int GetHighestStatInGroup(StatType statType, List<int> selectedCharacterIndices)
    {
        // Why: Find the highest value of the specified stat among selected characters
        GameManager gm = GameManager.Instance;
        int highest = 0;

        foreach (int index in selectedCharacterIndices)
        {
            CharacterSlotState charState = gm.characterStates[index];
            if (charState == null || charState.slotData == null) continue;

            SlotData data = charState.slotData;
            int statValue = 0;

            // Get the appropriate stat value
            switch (statType)
            {
                case StatType.Charisma: statValue = data.charisma; break;
                case StatType.StagePerformance: statValue = data.stagePerformance; break;
                case StatType.Vocal: statValue = data.vocal; break;
                case StatType.Instrument: statValue = data.instrument; break;
                case StatType.Songwriting: statValue = data.songwriting; break;
                case StatType.Production: statValue = data.production; break;
                case StatType.Management: statValue = data.management; break;
                case StatType.Practical: statValue = data.practical; break;
            }

            if (statValue > highest)
            {
                highest = statValue;
            }
        }

        return highest;
    }

    // ============================================
    // COMPLETE ACTION
    // ============================================

    private void CompleteAction(int characterIndex)
    {
        GameManager gm = GameManager.Instance;
        CharacterSlotState charState = gm.characterStates[characterIndex];

        if (charState == null || charState.currentAction == null)
        {
            Debug.LogError($"❌ CompleteAction: Invalid state for character {characterIndex}");
            return;
        }

        ActionData action = charState.currentAction;
        List<int> groupIndices = new List<int>(charState.groupedWithSlots);

        Debug.Log($"✅ Action complete: {action.actionName} for character {characterIndex}");

        // ============================================
        // APPLY REWARDS
        // ============================================

        ApplyRewards(action, groupIndices);

        // ============================================
        // UNLOCK ALL CHARACTERS IN THIS ACTION GROUP
        // ============================================

        foreach (int index in groupIndices)
        {
            CharacterSlotState state = gm.characterStates[index];
            if (state != null)
            {
                state.CompleteAction();
                Debug.Log($"   🔓 Unlocked character slot {index}");
            }
        }

        // ============================================
        // REFRESH UI
        // ============================================

        if (gm.uiController != null)
        {
            gm.uiController.RefreshUI();
        }
    }

    private void ApplyRewards(ActionData action, List<int> groupIndices)
    {
        GameManager gm = GameManager.Instance;

        // ============================================
        // CALCULATE MULTIPLIERS
        // ============================================

        float multiplier = 1f;

        // Full team bonus (if all 6 band members participated)
        int totalBandMembers = 0;
        for (int i = 0; i < gm.characterStates.Length; i++)
        {
            if (gm.characterStates[i] != null && gm.characterStates[i].slotData != null)
            {
                totalBandMembers++;
            }
        }

        bool isFullTeam = (groupIndices.Count == totalBandMembers);
        if (isFullTeam && action.fullTeamBonusMultiplier > 1f)
        {
            multiplier = action.fullTeamBonusMultiplier;
            Debug.Log($"🌟 FULL TEAM BONUS! {multiplier}x multiplier");
        }

        // ============================================
        // APPLY RESOURCE REWARDS
        // ============================================

        int moneyGained = Mathf.RoundToInt((action.rewardMoney + action.rewardMoneyPerCharacter * groupIndices.Count) * multiplier);
        int fansGained = Mathf.RoundToInt((action.rewardFans + action.rewardFansPerCharacter * groupIndices.Count) * multiplier);
        int unityGained = action.rewardUnity;

        gm.money += moneyGained;
        gm.fans += fansGained;
        gm.unity += unityGained;

        if (moneyGained > 0) Debug.Log($"💰 +${moneyGained} money");
        if (fansGained > 0) Debug.Log($"👥 +{fansGained} fans");
        if (unityGained > 0) Debug.Log($"🤝 +{unityGained} unity");

        // ============================================
        // APPLY STAT REWARDS TO CHARACTERS
        // ============================================

        if (action.statGainType != StatGainType.None)
        {
            foreach (int index in groupIndices)
            {
                ApplyStatGain(gm.characterStates[index], action);
            }
        }

        // ============================================
        // APPLY MORALE REWARDS
        // ============================================

        if (action.improveMorale)
        {
            foreach (int index in groupIndices)
            {
                CharacterSlotState charState = gm.characterStates[index];
                if (charState != null && charState.slotData != null)
                {
                    charState.slotData.morale += action.moraleGainAmount;
                    charState.slotData.morale = Mathf.Min(10, charState.slotData.morale); // Cap at 10
                    Debug.Log($"😊 {charState.slotData.displayName} gained +{action.moraleGainAmount} morale");
                }
            }
        }
    }

    private void ApplyStatGain(CharacterSlotState charState, ActionData action)
    {
        if (charState == null || charState.slotData == null) return;

        SlotData data = charState.slotData;

        switch (action.statGainType)
        {
            case StatGainType.Specific:
                // Improve one specific stat
                ImproveStat(data, action.specificStatToImprove, action.statGainAmount);
                break;

            case StatGainType.Random:
                // Improve one random stat
                StatType randomStat = (StatType)Random.Range(0, 8);
                ImproveStat(data, randomStat, action.statGainAmount);
                break;

            case StatGainType.AllStatsSmall:
                // Improve all stats by a small amount
                for (int i = 0; i < 8; i++)
                {
                    ImproveStat(data, (StatType)i, action.statGainAmount);
                }
                break;
        }
    }

    private void ImproveStat(SlotData data, StatType statType, int amount)
    {
        // Why: Improve the specified stat and cap at 10
        switch (statType)
        {
            case StatType.Charisma:
                data.charisma = Mathf.Min(10, data.charisma + amount);
                Debug.Log($"📈 {data.displayName} Charisma +{amount} → {data.charisma}");
                break;
            case StatType.StagePerformance:
                data.stagePerformance = Mathf.Min(10, data.stagePerformance + amount);
                Debug.Log($"📈 {data.displayName} Stage Performance +{amount} → {data.stagePerformance}");
                break;
            case StatType.Vocal:
                data.vocal = Mathf.Min(10, data.vocal + amount);
                Debug.Log($"📈 {data.displayName} Vocal +{amount} → {data.vocal}");
                break;
            case StatType.Instrument:
                data.instrument = Mathf.Min(10, data.instrument + amount);
                Debug.Log($"📈 {data.displayName} Instrument +{amount} → {data.instrument}");
                break;
            case StatType.Songwriting:
                data.songwriting = Mathf.Min(10, data.songwriting + amount);
                Debug.Log($"📈 {data.displayName} Songwriting +{amount} → {data.songwriting}");
                break;
            case StatType.Production:
                data.production = Mathf.Min(10, data.production + amount);
                Debug.Log($"📈 {data.displayName} Production +{amount} → {data.production}");
                break;
            case StatType.Management:
                data.management = Mathf.Min(10, data.management + amount);
                Debug.Log($"📈 {data.displayName} Management +{amount} → {data.management}");
                break;
            case StatType.Practical:
                data.practical = Mathf.Min(10, data.practical + amount);
                Debug.Log($"📈 {data.displayName} Practical +{amount} → {data.practical}");
                break;
        }
    }

    // ============================================
    // CANCEL ACTION
    // ============================================

    /// <summary>
    /// Cancel an action for a specific character (and their group)
    /// </summary>
    public void CancelAction(int characterIndex)
    {
        GameManager gm = GameManager.Instance;
        CharacterSlotState charState = gm.characterStates[characterIndex];

        if (charState == null || !charState.isBusy)
        {
            Debug.LogWarning($"⚠️ Cannot cancel - character {characterIndex} is not busy");
            return;
        }

        ActionData action = charState.currentAction;
        List<int> groupIndices = new List<int>(charState.groupedWithSlots);

        Debug.Log($"❌ Canceling action: {action.actionName} for character {characterIndex}");

        // Why: Unlock all characters in this action group
        foreach (int index in groupIndices)
        {
            CharacterSlotState state = gm.characterStates[index];
            if (state != null)
            {
                state.CancelAction();
                Debug.Log($"   🔓 Unlocked character slot {index}");
            }
        }

        // Why: Refresh UI
        if (gm.uiController != null)
        {
            gm.uiController.RefreshUI();
        }

        // TODO: Decide if we should refund costs when canceling
    }
}