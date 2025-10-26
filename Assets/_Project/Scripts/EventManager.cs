using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages event triggering and display
/// Checks for events each quarter and handles player choices
/// Now supports year + quarter checking and custom audio
/// </summary>
public class EventManager : MonoBehaviour
{
    [Header("Event Database")]
    public List<EventData> allEvents; // All possible events
    private EventData currentEvent;
    private AudioClip previousMusic; // Store music to restore after event

    [Header("References")]
    public UIController_Game uiController;
    public EventPanel eventPanel; // NEW: Direct reference to EventPanel script

    private void Start()
    {
        // Why: Auto-register with GameManager when scene loads
        if (GameManager.Instance != null)
        {
            GameManager.Instance.eventManager = this;
            Debug.Log("✅ EventManager connected to GameManager");
        }
        else
        {
            Debug.LogError("❌ GameManager not found! Make sure Bootstrap scene is loaded first.");
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
            if (CheckAllConditions(evt))
            {
                Debug.Log($"✅ All conditions met for: {evt.eventTitle}");
                TriggerEvent(evt);
                return; // Only one event per quarter
            }
        }

        Debug.Log("⚠️ No events matched all conditions this quarter");
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
        return true;
    }

    private bool CheckStatCondition(EventData evt)
    {
        // Why: Compare the specified stat against the required value
        GameManager gm = GameManager.Instance;
        int statValue = 0;

        // Get the stat value based on which stat to check
        switch (evt.statToCheck)
        {
            case StatToCheck.Money:
                statValue = gm.money;
                break;
            case StatToCheck.Fans:
                statValue = gm.fans;
                break;
            case StatToCheck.Technical:
                statValue = gm.technical;
                break;
            case StatToCheck.Performance:
                statValue = gm.performance;
                break;
            case StatToCheck.Charisma:
                statValue = gm.charisma;
                break;
            case StatToCheck.Unity:
                statValue = gm.unity;
                break;
        }

        // Compare based on comparison type
        bool result = false;
        switch (evt.comparison)
        {
            case ComparisonType.GreaterThan:
                result = statValue > evt.statValue;
                Debug.Log($"         {evt.statToCheck} ({statValue}) > {evt.statValue}? {result}");
                break;
            case ComparisonType.LessThan:
                result = statValue < evt.statValue;
                Debug.Log($"         {evt.statToCheck} ({statValue}) < {evt.statValue}? {result}");
                break;
            case ComparisonType.EqualTo:
                result = statValue == evt.statValue;
                Debug.Log($"         {evt.statToCheck} ({statValue}) == {evt.statValue}? {result}");
                break;
            case ComparisonType.GreaterOrEqual:
                result = statValue >= evt.statValue;
                Debug.Log($"         {evt.statToCheck} ({statValue}) >= {evt.statValue}? {result}");
                break;
            case ComparisonType.LessOrEqual:
                result = statValue <= evt.statValue;
                Debug.Log($"         {evt.statToCheck} ({statValue}) <= {evt.statValue}? {result}");
                break;
        }

        return result;
    }

    private void TriggerEvent(EventData evt)
    {
        // Why: Show this event to the player
        Debug.Log($"🎉 TRIGGERING EVENT: {evt.eventTitle}");
        currentEvent = evt;

        // Handle custom audio
        HandleEventAudio(evt);

        // Show event through EventPanel
        if (eventPanel != null)
        {
            Debug.Log($"   📺 Showing event via EventPanel");
            eventPanel.ShowEvent(evt);
        }
        else
        {
            Debug.LogWarning("   ⚠️ EventPanel reference missing! Assign it in Inspector.");
        }

        // OLD: Legacy support if using UIController_Game directly
        if (uiController != null)
        {
            uiController.ShowEvent(evt);
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

        // OLD: Legacy support
        if (uiController != null)
        {
            uiController.HideEvent();
        }

        currentEvent = null;

        // Refresh UI to show stat changes
        GameManager.Instance.ForceRefreshUI();
    }
}