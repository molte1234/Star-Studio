using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

/// <summary>
/// TIMER SERVICE - Counts down action timers
/// Does NOT manage state, does NOT call UI directly
/// Only receives instructions from GameManager and reports back when done
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
        Debug.Log("✅ ActionManager initialized (Timer Service)");
    }

    // ============================================
    // TIMER DATA
    // ============================================

    private class ActionTimer
    {
        public int characterIndex; // Primary character for this timer
        public List<int> groupedCharacters; // All characters in this action group
        public float timeRemaining;
    }

    private List<ActionTimer> activeTimers = new List<ActionTimer>();

    // ============================================
    // ODIN INSPECTOR DEBUG
    // ============================================

    [Title("Active Timers", bold: true, horizontalLine: true)]
    [ShowInInspector, ReadOnly]
    private int ActiveTimerCount => activeTimers.Count;

    // ============================================
    // UPDATE - COUNT DOWN TIMERS
    // ============================================

    private void Update()
    {
        if (activeTimers.Count == 0) return;

        // Count down all timers
        for (int i = activeTimers.Count - 1; i >= 0; i--)
        {
            ActionTimer timer = activeTimers[i];

            // Update time remaining in GameManager
            GameManager gm = GameManager.Instance;
            if (gm != null)
            {
                foreach (int charIndex in timer.groupedCharacters)
                {
                    if (charIndex < gm.characterStates.Length)
                    {
                        gm.characterStates[charIndex].actionTimeRemaining -= Time.deltaTime;
                    }
                }
            }

            timer.timeRemaining -= Time.deltaTime;

            // Timer finished?
            if (timer.timeRemaining <= 0f)
            {
                Debug.Log($"⏰ Timer finished for character {timer.characterIndex}");

                // Tell GameManager: "Action complete"
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
    /// Start a timer for an action
    /// Called by GameManager after it changes state
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
        // CALCULATE DURATION (read stats from GameManager)
        // ============================================

        float duration = action.baseTime;

        if (action.requiresMembers && characterIndices.Count > 0)
        {
            // Find highest relevant stat
            int highestStat = GetHighestRelevantStat(action.timeEfficiencyStat, characterIndices);

            // Calculate time reduction
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
            gm.characterStates[index].groupedWithSlots = new List<int>(characterIndices);
        }

        // ============================================
        // CREATE TIMER
        // ============================================

        ActionTimer timer = new ActionTimer
        {
            characterIndex = characterIndices[0], // Use first character as primary
            groupedCharacters = new List<int>(characterIndices),
            timeRemaining = duration
        };

        activeTimers.Add(timer);
        Debug.Log($"   ⏰ Timer started: {duration:F1}s for {characterIndices.Count} characters");
    }

    // ============================================
    // STOP TIMER (called by GameManager)
    // ============================================

    /// <summary>
    /// Stop a timer (for cancellation)
    /// Called by GameManager after it changes state
    /// UPDATED: Removes only the canceled character, keeps timer running if others remain
    /// </summary>
    public void StopTimer(List<int> characterIndices)
    {
        if (characterIndices == null || characterIndices.Count == 0) return;

        Debug.Log($"🛑 ActionManager.StopTimer called for characters: [{string.Join(", ", characterIndices)}]");

        // Find timers that involve any of these characters
        for (int i = activeTimers.Count - 1; i >= 0; i--)
        {
            ActionTimer timer = activeTimers[i];

            // Check if this timer involves any of the cancelled characters
            bool timerInvolved = false;
            foreach (int canceledChar in characterIndices)
            {
                if (timer.groupedCharacters.Contains(canceledChar))
                {
                    timerInvolved = true;
                    break;
                }
            }

            if (!timerInvolved) continue; // This timer doesn't involve canceled characters

            Debug.Log($"   📍 Found timer with {timer.groupedCharacters.Count} characters: [{string.Join(", ", timer.groupedCharacters)}]");

            // ============================================
            // REMOVE CANCELED CHARACTERS FROM TIMER
            // ============================================

            foreach (int canceledChar in characterIndices)
            {
                if (timer.groupedCharacters.Contains(canceledChar))
                {
                    timer.groupedCharacters.Remove(canceledChar);
                    Debug.Log($"      ➖ Removed character {canceledChar} from timer");
                }
            }

            // ============================================
            // CHECK IF TIMER SHOULD BE DELETED
            // ============================================

            if (timer.groupedCharacters.Count == 0)
            {
                // No characters left - remove timer completely
                activeTimers.RemoveAt(i);
                Debug.Log($"      🗑️ Timer removed - no characters remaining");
            }
            else
            {
                // Characters still working - keep timer running!
                Debug.Log($"      ⏰ Timer continues with {timer.groupedCharacters.Count} characters: [{string.Join(", ", timer.groupedCharacters)}]");
            }
        }
    }

    // ============================================
    // HELPER - GET HIGHEST STAT
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