using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages event triggering and display
/// Checks for events each quarter and handles player choices
/// Now supports year + quarter checking and custom audio
/// </summary>
public class EventManager : MonoBehaviour
{
    [Header("Welcome Screen")]
    [Tooltip("Special event that shows at game start - bypasses all trigger conditions")]
    public EventData welcomeScreenEvent;

    [Header("Event Database")]
    public List<EventData> allEvents; // All possible events
    private EventData currentEvent;
    private List<EventData> triggeredEvents = new List<EventData>(); // Track which events already happened

    [Header("References")]
    public EventPanel eventPanel; // Direct reference to EventPanel script

    private void Awake()
    {
        // Why: Register with GameManager as early as possible
        Debug.Log("🔧 EventManager.Awake() called!");

        if (GameManager.Instance == null)
        {
            Debug.LogError("❌ GameManager.Instance is NULL in EventManager.Awake()!");
            return;
        }

        Debug.Log("✅ GameManager.Instance found, connecting...");
        GameManager.Instance.eventManager = this;
        Debug.Log("✅ EventManager connected to GameManager");
    }

    private void OnEnable()
    {
        // Why: Scene was activated - check for welcome screen
        // OnEnable runs every time the Game scene activates (not just first load)
        Debug.Log("🔧 EventManager.OnEnable() - scene activated!");
        StartCoroutine(CheckForStartingEventsDelayed());
    }

    private System.Collections.IEnumerator CheckForStartingEventsDelayed()
    {
        // Why: Wait one frame to ensure scene is fully loaded
        Debug.Log("🔧 Coroutine: Waiting one frame...");
        yield return null;

        Debug.Log("🔧 Coroutine: Frame passed, calling OnGameSceneLoaded...");
        // Check if this is a new game start (Y1 Q1 events)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameSceneLoaded();
        }
        else
        {
            Debug.LogError("❌ Coroutine: GameManager.Instance is null!");
        }
    }

    /// <summary>
    /// Resets the triggered events list - call this when starting a new game
    /// </summary>
    public void ResetTriggeredEvents()
    {
        triggeredEvents.Clear();
        Debug.Log("🔄 Event history cleared - all events can trigger again");
    }

    /// <summary>
    /// Shows the welcome screen event - bypasses all trigger conditions
    /// Called at the start of a new game
    /// </summary>
    public void ShowWelcomeScreen()
    {
        if (welcomeScreenEvent == null)
        {
            Debug.LogWarning("⚠️ Welcome screen event not assigned in EventManager!");
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
        // Why: Check if stat condition is met based on stat type and comparison
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
            case StatToCheck.Technical:
                statValue = GameManager.Instance.technical;
                break;
            case StatToCheck.Performance:
                statValue = GameManager.Instance.performance;
                break;
            case StatToCheck.Charisma:
                statValue = GameManager.Instance.charisma;
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

        // Apply effects
        GameManager.Instance.money += choice.moneyChange;
        GameManager.Instance.fans += choice.fansChange;
        GameManager.Instance.unity += choice.unityChange;

        // Apply stat changes if any
        GameManager.Instance.technical += choice.technicalChange;
        GameManager.Instance.performance += choice.performanceChange;
        GameManager.Instance.charisma += choice.charismaChange;

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
}