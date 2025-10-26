using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages story events - checking triggers and displaying events to the player
/// Attached to GameManager in Bootstrap scene
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

    public void ShowWelcomeScreen()
    {
        // Why: Display welcome screen at game start (called from UIController_Game)
        if (welcomeScreenEvent == null)
        {
            Debug.LogError("❌ Welcome Screen Event not assigned in EventManager!");
            return;
        }

        Debug.Log("🎉 Showing welcome screen!");
        TriggerEvent(welcomeScreenEvent);

        // Mark welcome screen as triggered so it won't show again
        if (!triggeredEvents.Contains(welcomeScreenEvent))
        {
            triggeredEvents.Add(welcomeScreenEvent);
        }
    }

    public void CheckForEvents()
    {
        // Why: Called each quarter, see if any event should trigger
        // Check ALL conditions for each event

        int currentYear = GameManager.Instance.currentYear;
        int displayQuarter = (GameManager.Instance.currentQuarter % 4) + 1;

        Debug.Log($"🔍 Checking for events... Year: {currentYear}, Quarter: {displayQuarter}");
        Debug.Log($"📋 Total events in database: {allEvents.Count}");

        foreach (EventData evt in allEvents)
        {
            // ✅ FIX: NEVER check the welcome screen event in normal flow
            if (evt == welcomeScreenEvent)
            {
                Debug.Log($"   ⏭️ Skipping '{evt.eventTitle}' - this is the welcome screen (use ShowWelcomeScreen instead)");
                continue;
            }

            // ✅ FIX: Skip if this event already triggered
            if (triggeredEvents.Contains(evt))
            {
                Debug.Log($"   ⏭️ Skipping '{evt.eventTitle}' - already triggered");
                continue;
            }

            if (CheckAllConditions(evt))
            {
                Debug.Log($"✅ All conditions met for: {evt.eventTitle}");
                TriggerEvent(evt);

                // ✅ FIX: Mark this event as triggered so it won't show again
                triggeredEvents.Add(evt);

                return; // Only show one event per quarter
            }
        }

        Debug.Log("ℹ️ No events triggered this quarter");
    }

    private bool CheckAllConditions(EventData evt)
    {
        // Why: Check if ALL enabled conditions are met for this event

        Debug.Log($"   🎯 Checking event: {evt.eventTitle}");

        // Check Year/Quarter condition
        if (evt.requireYearQuarter)
        {
            int currentYear = GameManager.Instance.currentYear;
            int displayQuarter = (GameManager.Instance.currentQuarter % 4) + 1;

            if (evt.triggerYear != currentYear || evt.triggerQuarter != displayQuarter)
            {
                Debug.Log($"      ❌ Year/Quarter mismatch: Need Y{evt.triggerYear} Q{evt.triggerQuarter}, Current Y{currentYear} Q{displayQuarter}");
                return false;
            }
            Debug.Log($"      ✅ Year/Quarter match: Y{currentYear} Q{displayQuarter}");
        }

        // Check Flag condition
        if (evt.requireFlag)
        {
            if (!GameManager.Instance.flags.Contains(evt.requiredFlag))
            {
                Debug.Log($"      ❌ Required flag missing: {evt.requiredFlag}");
                return false;
            }
            Debug.Log($"      ✅ Flag present: {evt.requiredFlag}");
        }

        // Check Stat condition
        if (evt.requireStat)
        {
            if (!CheckStatCondition(evt))
            {
                Debug.Log($"      ❌ Stat condition not met");
                return false;
            }
            Debug.Log($"      ✅ Stat condition met");
        }

        // Check Random Chance (always checked)
        float roll = Random.Range(0f, 100f);
        if (roll > evt.randomChance)
        {
            Debug.Log($"      ❌ Random chance failed: Rolled {roll:F1}%, needed ≤{evt.randomChance}%");
            return false;
        }
        Debug.Log($"      ✅ Random chance passed: Rolled {roll:F1}% ≤ {evt.randomChance}%");

        // All conditions met!
        Debug.Log($"   ✅ ALL CONDITIONS PASSED!");
        return true;
    }

    private bool CheckStatCondition(EventData evt)
    {
        // ✅ UPDATED: Check if stat condition is met using NEW 8-stat system
        int statValue = 0;

        // Get the stat value based on StatToCheck enum
        switch (evt.statToCheck)
        {
            case StatToCheck.Money:
                statValue = GameManager.Instance.money;
                break;
            case StatToCheck.Fans:
                statValue = GameManager.Instance.fans;
                break;
            // ✅ NEW 8-STAT SYSTEM:
            case StatToCheck.Charisma:
                statValue = GameManager.Instance.charisma;
                break;
            case StatToCheck.StagePerformance:
                statValue = GameManager.Instance.stagePerformance;
                break;
            case StatToCheck.Vocal:
                statValue = GameManager.Instance.vocal;
                break;
            case StatToCheck.Instrument:
                statValue = GameManager.Instance.instrument;
                break;
            case StatToCheck.Songwriting:
                statValue = GameManager.Instance.songwriting;
                break;
            case StatToCheck.Production:
                statValue = GameManager.Instance.production;
                break;
            case StatToCheck.Management:
                statValue = GameManager.Instance.management;
                break;
            case StatToCheck.Practical:
                statValue = GameManager.Instance.practical;
                break;
            case StatToCheck.Unity:
                statValue = GameManager.Instance.unity;
                break;
        }

        // Check based on comparison type
        switch (evt.comparison)
        {
            case ComparisonType.GreaterThan:
                return statValue > evt.statValue;
            case ComparisonType.LessThan:
                return statValue < evt.statValue;
            case ComparisonType.EqualTo:
                return statValue == evt.statValue;
            case ComparisonType.GreaterOrEqual:
                return statValue >= evt.statValue;
            case ComparisonType.LessOrEqual:
                return statValue <= evt.statValue;
            default:
                return false;
        }
    }

    public void TriggerEvent(EventData eventData)
    {
        // Why: Show this event to the player
        Debug.Log($"🎉 TRIGGERING EVENT: {eventData.eventTitle}");
        currentEvent = eventData;

        // Handle custom audio
        HandleEventAudio(eventData);

        // Show event through EventPanel
        if (eventPanel != null)
        {
            Debug.Log($"   📺 Showing event via EventPanel");
            eventPanel.ShowEvent(eventData);
        }
        else
        {
            Debug.LogWarning("   ⚠️ EventPanel reference missing! Assign it in Inspector.");
        }
    }

    private void HandleEventAudio(EventData evt)
    {
        // Why: Handle event audio - pause current music and play event music/sfx
        AudioManager audioMgr = AudioManager.Instance;
        if (audioMgr == null) return;

        // STEP 1: Pause current music (with fade out)
        audioMgr.PauseMusic();

        // STEP 2: Play popup SFX if provided (this happens on separate source, won't interrupt)
        if (evt.eventPopupSFX != null)
        {
            audioMgr.PlaySFX(evt.eventPopupSFX);
        }

        // STEP 3: Play event music if provided (on dedicated event music source)
        if (evt.eventMusic != null)
        {
            audioMgr.PlayEventMusic(evt.eventMusic);
        }
    }

    public void PlayerChoseOption(int choiceIndex)
    {
        // Why: Player clicked a choice button, apply effects
        if (currentEvent == null) return;

        ChoiceData choice = currentEvent.choices[choiceIndex];

        // Apply resource changes
        GameManager.Instance.money += choice.moneyChange;
        GameManager.Instance.fans += choice.fansChange;
        GameManager.Instance.unity += choice.unityChange;

        // ✅ UPDATED: Apply NEW 8-stat system changes
        // Note: ChoiceData also needs to be updated to have these new stat fields
        // For now, keeping the old fields but you'll need to add new ones to ChoiceData
        GameManager.Instance.charisma += choice.charismaChange;
        GameManager.Instance.stagePerformance += choice.stagePerformanceChange;
        GameManager.Instance.vocal += choice.vocalChange;
        GameManager.Instance.instrument += choice.instrumentChange;
        GameManager.Instance.songwriting += choice.songwritingChange;
        GameManager.Instance.production += choice.productionChange;
        GameManager.Instance.management += choice.managementChange;
        GameManager.Instance.practical += choice.practicalChange;

        // Add flags if any
        foreach (string flag in choice.flagsToAdd)
        {
            GameManager.Instance.flags.Add(flag);
        }

        // Resume regular music (fade out event music, fade in paused music)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ResumeMusic();
        }

        // Hide event
        if (eventPanel != null)
        {
            eventPanel.HideEvent();
        }

        currentEvent = null;

        // Refresh UI to show stat changes
        GameManager.Instance.ForceRefreshUI();
    }

    public void ResetTriggeredEvents()
    {
        // Why: Clear the history of triggered events (used when starting new game)
        triggeredEvents.Clear();
        Debug.Log("🔄 Event history cleared");
    }
}