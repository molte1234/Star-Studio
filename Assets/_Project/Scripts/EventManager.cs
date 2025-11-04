using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages story events - checking triggers and displaying events to the player
/// Attached to a GameObject in the Game scene
/// Registers itself with GameManager on Start()
/// </summary>
public class EventManager : MonoBehaviour
{
    [Header("Event Database")]
    [Tooltip("All possible events in the game")]
    public List<EventData> allEvents = new List<EventData>();

    [Header("Special Events")]
    [Tooltip("Welcome screen event - shown once at game start")]
    public EventData welcomeScreenEvent;

    [Header("UI Reference")]
    public EventPanel eventPanel; // Reference to the EventPanel UI

    private List<EventData> triggeredEvents = new List<EventData>(); // Track which events already triggered
    private EventData currentEvent; // Currently displayed event

    void Start()
    {
        // Why: Register this EventManager with GameManager (they're in different scenes)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterEventManager(this);
            Debug.Log("✅ EventManager registered with GameManager");
        }
        else
        {
            Debug.LogError("❌ EventManager: GameManager.Instance is null!");
        }
    }

    public void ShowWelcomeScreen()
    {
        // Why: Display welcome screen at game start
        if (welcomeScreenEvent == null)
        {
            Debug.LogError("❌ Welcome Screen Event not assigned in EventManager!");
            return;
        }

        Debug.Log("🎉 Showing welcome screen!");
        TriggerEvent(welcomeScreenEvent);
    }

    public void CheckForEvents()
    {
        // Why: Called each quarter to see if any events should trigger
        Debug.Log($"🔍 Checking for events in Y{GameManager.Instance.currentYear} Q{(GameManager.Instance.currentQuarter % 4) + 1}");

        foreach (EventData evt in allEvents)
        {
            // Skip if already triggered
            if (triggeredEvents.Contains(evt))
            {
                continue;
            }

            // Check if all conditions met
            if (ShouldTrigger(evt))
            {
                Debug.Log($"   ✅ Triggering event: {evt.eventTitle}");
                TriggerEvent(evt);
                triggeredEvents.Add(evt);
                break; // Only trigger one event per quarter
            }
            else
            {
                // Debug.Log($"   ❌ '{evt.eventTitle}' conditions not met");
            }
        }

        Debug.Log("   No events triggered this quarter");
    }

    private bool ShouldTrigger(EventData evt)
    {
        // Why: Check all conditions to see if event should fire
        GameManager gm = GameManager.Instance;

        // ✅ CHECK 1: Year/Quarter requirement
        if (evt.requireYearQuarter)
        {
            int displayQuarter = (gm.currentQuarter % 4) + 1;

            if (gm.currentYear != evt.triggerYear)
                return false;

            if (displayQuarter != evt.triggerQuarter)
                return false;
        }

        // ✅ CHECK 2: Story flag requirement
        if (evt.requireFlag)
        {
            if (!gm.flags.Contains(evt.requiredFlag))
                return false;
        }

        // ✅ CHECK 3: Stat threshold requirement
        if (evt.requireStat)
        {
            int currentValue = GetStatValue(evt.statToCheck, gm);
            if (!CheckComparison(currentValue, evt.comparison, evt.statValue))
                return false;
        }

        // ✅ CHECK 4: Random chance
        if (evt.randomChance < 100f)
        {
            float roll = Random.Range(0f, 100f);
            if (roll > evt.randomChance)
            {
                Debug.Log($"   🎲 Random check failed: rolled {roll:F1}% vs {evt.randomChance}%");
                return false;
            }
        }

        return true;
    }

    private int GetStatValue(StatToCheck stat, GameManager gm)
    {
        // Why: Get the current value of a stat from GameManager
        // ✅ UPDATED: Removed Unity stat case
        switch (stat)
        {
            case StatToCheck.Money: return gm.money;
            case StatToCheck.Fans: return gm.fans;
            case StatToCheck.Charisma: return gm.charisma;
            case StatToCheck.StagePerformance: return gm.stagePerformance;
            case StatToCheck.Vocal: return gm.vocal;
            case StatToCheck.Instrument: return gm.instrument;
            case StatToCheck.Songwriting: return gm.songwriting;
            case StatToCheck.Production: return gm.production;
            case StatToCheck.Management: return gm.management;
            case StatToCheck.Practical: return gm.practical;
            default: return 0;
        }
    }

    private bool CheckComparison(int currentValue, ComparisonType comparison, int targetValue)
    {
        // Why: Check if the comparison passes
        switch (comparison)
        {
            case ComparisonType.GreaterThan: return currentValue > targetValue;
            case ComparisonType.LessThan: return currentValue < targetValue;
            case ComparisonType.EqualTo: return currentValue == targetValue;
            case ComparisonType.GreaterOrEqual: return currentValue >= targetValue;
            case ComparisonType.LessOrEqual: return currentValue <= targetValue;
            default: return false;
        }
    }

    private void TriggerEvent(EventData evt)
    {
        // Why: Show the event popup
        currentEvent = evt;

        if (eventPanel == null)
        {
            Debug.LogError("❌ EventPanel is not assigned! Cannot show event.");
            return;
        }

        // Handle audio
        HandleEventAudio(evt);

        // Show the popup
        eventPanel.ShowEvent(evt);
    }

    private void HandleEventAudio(EventData evt)
    {
        // Why: Handle event audio - pause current music and play event music/sfx
        AudioManager audioMgr = AudioManager.Instance;
        if (audioMgr == null) return;

        // Pause current music (with fade out)
        audioMgr.PauseMusic();

        // Play popup SFX if provided
        if (evt.eventPopupSFX != null)
        {
            audioMgr.PlaySFX(evt.eventPopupSFX);
        }

        // Play event music if provided - FIXED: Using correct method name
        if (evt.eventMusic != null)
        {
            audioMgr.PlayEventMusic(evt.eventMusic);
        }
    }

    // FIXED: Using correct method name that EventPanel calls
    public void PlayerChoseOption(int choiceIndex)
    {
        // Why: Player clicked a choice button, apply effects
        if (currentEvent == null)
        {
            Debug.LogError("❌ No current event!");
            return;
        }

        if (choiceIndex < 0 || choiceIndex >= currentEvent.choices.Length)
        {
            Debug.LogError($"❌ Invalid choice index: {choiceIndex}");
            return;
        }

        ChoiceData choice = currentEvent.choices[choiceIndex];
        Debug.Log($"💬 Player selected: '{choice.choiceText}'");

        // Apply effects
        GameManager gm = GameManager.Instance;

        // Apply money change
        if (choice.moneyChange != 0)
        {
            gm.money += choice.moneyChange;
            Debug.Log($"   💰 Money {(choice.moneyChange > 0 ? "+" : "")}{choice.moneyChange} → Total: {gm.money}");
        }

        // FIXED: Using correct property name 'fansChange' not 'fanChange'
        if (choice.fansChange != 0)
        {
            gm.fans += choice.fansChange;
            Debug.Log($"   👥 Fans {(choice.fansChange > 0 ? "+" : "")}{choice.fansChange} → Total: {gm.fans}");
        }

        // REMOVED: Unity stat change (no longer exists)

        // Apply stat changes
        if (choice.charismaChange != 0) gm.charisma += choice.charismaChange;
        if (choice.stagePerformanceChange != 0) gm.stagePerformance += choice.stagePerformanceChange;
        if (choice.vocalChange != 0) gm.vocal += choice.vocalChange;
        if (choice.instrumentChange != 0) gm.instrument += choice.instrumentChange;
        if (choice.songwritingChange != 0) gm.songwriting += choice.songwritingChange;
        if (choice.productionChange != 0) gm.production += choice.productionChange;
        if (choice.managementChange != 0) gm.management += choice.managementChange;
        if (choice.practicalChange != 0) gm.practical += choice.practicalChange;

        // FIXED: Using correct property 'flagsToAdd' (array) not 'flagToAdd'
        if (choice.flagsToAdd != null && choice.flagsToAdd.Length > 0)
        {
            foreach (string flag in choice.flagsToAdd)
            {
                if (!string.IsNullOrEmpty(flag) && !gm.flags.Contains(flag))
                {
                    gm.flags.Add(flag);
                    Debug.Log($"   🚩 Added flag: '{flag}'");
                }
            }
        }

        // Resume regular music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ResumeMusic();
        }

        // Hide event panel
        eventPanel.HideEvent();
        currentEvent = null;

        // Refresh UI to show stat changes
        GameManager.Instance.ForceRefreshUI();
    }

    public void ResetTriggeredEvents()
    {
        // Why: Clear history when restarting game
        triggeredEvents.Clear();
        currentEvent = null;
        Debug.Log("✅ Event history cleared");
    }
}