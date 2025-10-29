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

        // Mark welcome screen as triggered so it won't show again
        if (!triggeredEvents.Contains(welcomeScreenEvent))
        {
            triggeredEvents.Add(welcomeScreenEvent);
        }
    }

    public void CheckForEvents()
    {
        // Why: Called each quarter, see if any event should trigger
        int currentYear = GameManager.Instance.currentYear;
        int displayQuarter = (GameManager.Instance.currentQuarter % 4) + 1;

        Debug.Log($"🔍 Checking for events... Year: {currentYear}, Quarter: {displayQuarter}");
        Debug.Log($"📋 Total events in database: {allEvents.Count}");

        foreach (EventData evt in allEvents)
        {
            // Skip welcome screen (it's triggered manually)
            if (evt == welcomeScreenEvent)
            {
                Debug.Log($"   ⏭️ Skipping '{evt.eventTitle}' - this is the welcome screen");
                continue;
            }

            // Skip if already triggered
            if (triggeredEvents.Contains(evt))
            {
                Debug.Log($"   ⏭️ Skipping '{evt.eventTitle}' - already triggered");
                continue;
            }

            // Check if this event should trigger
            if (ShouldTrigger(evt))
            {
                Debug.Log($"   ✅ TRIGGERING EVENT: {evt.eventTitle}");
                TriggerEvent(evt);
                triggeredEvents.Add(evt);
                return; // Only show one event at a time
            }
            else
            {
                Debug.Log($"   ❌ '{evt.eventTitle}' conditions not met");
            }
        }

        Debug.Log("   No events triggered this quarter");
    }

    private bool ShouldTrigger(EventData evt)
    {
        // Why: Check all conditions to see if event should fire
        GameManager gm = GameManager.Instance;

        // Check year/quarter
        int displayQuarter = (gm.currentQuarter % 4) + 1;

        if (evt.triggerYear > 0 && gm.currentYear != evt.triggerYear)
            return false;

        if (evt.triggerQuarter > 0 && displayQuarter != evt.triggerQuarter)
            return false;

        // Check resource thresholds
        if (evt.minMoney > 0 && gm.money < evt.minMoney)
            return false;

        if (evt.maxMoney > 0 && gm.money > evt.maxMoney)
            return false;

        if (evt.minFans > 0 && gm.fans < evt.minFans)
            return false;

        // Check required flags
        foreach (string flag in evt.requiredFlags)
        {
            if (!gm.flags.Contains(flag))
                return false;
        }

        // Check forbidden flags
        foreach (string flag in evt.forbiddenFlags)
        {
            if (gm.flags.Contains(flag))
                return false;
        }

        return true;
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

        // Play event music if provided
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

        // Apply stat changes
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

        // Resume regular music
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