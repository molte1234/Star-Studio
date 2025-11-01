using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

/// <summary>
/// TIMER SERVICE - Counts down action timers
/// Each character has their OWN independent timer - no grouping!
/// Does NOT manage state, does NOT call UI directly
/// Only receives instructions from GameManager and reports back when done
/// 
/// NEW SYSTEM: INDIVIDUAL TIMERS
/// - Each character gets their own timer
/// - Bonuses calculated once at START based on full group
/// - Each character counts down independently
/// - Each character completes and gets rewards independently
/// </summary>
public class ActionManager : MonoBehaviour
{
    // ============================================
    // SINGLETON
    // ============================================

    public static ActionManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Debug.Log("✅ ActionManager initialized (Individual Timer Service)");
    }

    // ============================================
    // TIMER DATA
    // ============================================

    /// <summary>
    /// Each character has their OWN timer - no grouping
    /// Each character lives in their own timespace
    /// </summary>
    private class ActionTimer
    {
        public int characterIndex; // THIS character ONLY
        public float timeRemaining;
    }

    private List<ActionTimer> activeTimers = new List<ActionTimer>();

    // ============================================
    // ODIN INSPECTOR DEBUG
    // ============================================

    [Title("Active Timers", bold: true, horizontalLine: true)]
    [ShowInInspector, ReadOnly]
    private int ActiveTimerCount => activeTimers.Count;

    [ShowInInspector, ReadOnly]
    [ListDrawerSettings(Expanded = true)]
    private List<string> ActiveTimersDebug
    {
        get
        {
            List<string> debug = new List<string>();
            foreach (var timer in activeTimers)
            {
                debug.Add($"Character {timer.characterIndex}: {timer.timeRemaining:F1}s");
            }
            return debug;
        }
    }

    // ============================================
    // UPDATE - COUNT DOWN TIMERS INDEPENDENTLY
    // ============================================

    private void Update()
    {
        if (activeTimers.Count == 0) return;

        // Count down all timers INDEPENDENTLY
        for (int i = activeTimers.Count - 1; i >= 0; i--)
        {
            ActionTimer timer = activeTimers[i];

            // Update time remaining in GameManager for THIS character
            GameManager gm = GameManager.Instance;
            if (gm != null)
            {
                if (timer.characterIndex < gm.characterStates.Length)
                {
                    gm.characterStates[timer.characterIndex].actionTimeRemaining -= Time.deltaTime;
                }
            }

            timer.timeRemaining -= Time.deltaTime;

            // Timer finished for THIS character?
            if (timer.timeRemaining <= 0f)
            {
                Debug.Log($"⏰ Timer finished for character {timer.characterIndex}");

                // Tell GameManager: "Action complete for THIS character"
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.CompleteAction(timer.characterIndex);
                }

                activeTimers.RemoveAt(i);
            }
        }
    }

    // ============================================
    // START TIMER (called by GameManager)
    // ============================================

    /// <summary>
    /// Start individual timers for each character in the action
    /// Bonuses are calculated ONCE at the start based on the full group
    /// Then each character gets their own independent timer
    /// </summary>
    public void StartTimer(ActionData action, List<int> characterIndices)
    {
        if (action == null || characterIndices == null || characterIndices.Count == 0)
        {
            Debug.LogError("❌ ActionManager.StartTimer: Invalid parameters");
            return;
        }

        GameManager gm = GameManager.Instance;
        if (gm == null)
        {
            Debug.LogError("❌ ActionManager.StartTimer: GameManager is null");
            return;
        }

        // ============================================
        // CALCULATE DURATION ONCE (based on full group)
        // ============================================

        float duration = action.baseTime;

        if (action.requiresMembers && characterIndices.Count > 0)
        {
            // Find highest relevant stat from ALL characters in the group
            int highestStat = GetHighestRelevantStat(action.timeEfficiencyStat, characterIndices);

            // Calculate time reduction based on group bonus
            float timeReduction = (highestStat / 10f) * 0.1f;
            duration *= (1f - timeReduction);

            Debug.Log($"   ⏱️ Duration: {duration:F1}s (stat {action.timeEfficiencyStat}: {highestStat})");
        }

        // ============================================
        // STORE TIMER DATA IN GAMEMANAGER
        // ============================================

        foreach (int index in characterIndices)
        {
            gm.characterStates[index].actionTimeRemaining = duration;
            gm.characterStates[index].actionTotalDuration = duration;
        }

        // ============================================
        // CREATE INDIVIDUAL TIMERS - ONE PER CHARACTER
        // ============================================

        foreach (int index in characterIndices)
        {
            ActionTimer timer = new ActionTimer
            {
                characterIndex = index,
                timeRemaining = duration
            };

            activeTimers.Add(timer);
            Debug.Log($"   ⏰ Individual timer created for character {index}: {duration:F1}s");
        }

        Debug.Log($"✅ Created {characterIndices.Count} individual timers (each character lives independently)");
    }

    // ============================================
    // STOP TIMER (called by GameManager for cancel)
    // ============================================

    /// <summary>
    /// Stop a timer for a specific character (for cancellation)
    /// Called by GameManager after it changes state
    /// Simply removes THIS character's timer - other characters unaffected
    /// </summary>
    public void StopTimer(List<int> characterIndices)
    {
        if (characterIndices == null || characterIndices.Count == 0) return;

        Debug.Log($"🛑 ActionManager.StopTimer called for characters: [{string.Join(", ", characterIndices)}]");

        // Remove timer for each canceled character
        foreach (int canceledChar in characterIndices)
        {
            for (int i = activeTimers.Count - 1; i >= 0; i--)
            {
                if (activeTimers[i].characterIndex == canceledChar)
                {
                    activeTimers.RemoveAt(i);
                    Debug.Log($"   🗑️ Removed timer for character {canceledChar}");
                    break; // Each character has only one timer
                }
            }
        }

        Debug.Log($"✅ Timers removed - {activeTimers.Count} timers still running");
    }

    // ============================================
    // HELPER - GET HIGHEST STAT FROM GROUP
    // ============================================

    private int GetHighestRelevantStat(StatType statType, List<int> characterIndices)
    {
        GameManager gm = GameManager.Instance;
        int highest = 0;

        foreach (int index in characterIndices)
        {
            SlotData data = gm.characterStates[index].slotData;
            int statValue = 0;

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
}