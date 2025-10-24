using UnityEngine;
using System.Collections.Generic;

public class EventManager : MonoBehaviour
{
    [Header("Event Database")]
    public List<EventData> allEvents; // All possible events
    private EventData currentEvent;

    [Header("References")]
    public UIManager uiManager;

    public void CheckForEvents()
    {
        // Why: Called each quarter, see if any event should trigger
        // This runs after each action

        // Check for time-based events first
        CheckQuarterEvents();

        // Check for flag-based events
        CheckFlagEvents();

        // Random chance for a random event
        if (Random.value < 0.3f)
        { // 30% chance per quarter
            TriggerRandomEvent();
        }
    }

    private void CheckQuarterEvents()
    {
        // Why: Some events trigger at specific quarters
        foreach (EventData evt in allEvents)
        {
            if (evt.triggerType == TriggerType.Quarter)
            {
                if (evt.triggerQuarter == GameManager.Instance.currentQuarter)
                {
                    TriggerEvent(evt);
                    return; // Only one event per quarter
                }
            }
        }
    }

    private void CheckFlagEvents()
    {
        // Why: Some events require story flags to be set
        foreach (EventData evt in allEvents)
        {
            if (evt.triggerType == TriggerType.Flag)
            {
                if (GameManager.Instance.flags.Contains(evt.requiredFlag))
                {
                    TriggerEvent(evt);
                    return;
                }
            }
        }
    }

    private void TriggerRandomEvent()
    {
        // Why: Pick a random event from the pool
        List<EventData> randomEvents = allEvents.FindAll(e => e.triggerType == TriggerType.Random);
        if (randomEvents.Count > 0)
        {
            EventData evt = randomEvents[Random.Range(0, randomEvents.Count)];
            TriggerEvent(evt);
        }
    }

    private void TriggerEvent(EventData evt)
    {
        // Why: Show this event to the player
        currentEvent = evt;
        uiManager.ShowEvent(evt);
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

        // Show outcome text briefly (optional)
        uiManager.HideEvent();
        currentEvent = null;
    }
}