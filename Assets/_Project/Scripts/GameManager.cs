using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Singleton GameManager - holds all game state and processes player actions
/// This is the brain of the game
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton pattern - only one GameManager exists
    public static GameManager Instance;

    [Header("Game State")]
    private bool isNewGame = false; // Why: Track if this is a fresh game start

    [Header("Band Info")]
    public string bandName;
    public SlotData[] slots = new SlotData[6]; // 3 main band members + 3 support/equipment slots

    [Header("Time")]
    public int currentQuarter = 0; // 0-39 (40 quarters = 10 years)
    public int currentYear = 1;     // 1-10

    [Header("Resources")]
    public int money = 500;
    public int fans = 50;

    [Header("Band Stats - Calculated from Members")]
    public int technical = 0;    // Sum of all 3 band members' technical
    public int performance = 0;  // Sum of all 3 band members' performance
    public int charisma = 0;     // Sum of all 3 band members' charisma
    public int unity = 100;      // Band cohesion (0-100)

    [Header("Story Flags")]
    public List<string> flags = new List<string>(); // Track story progression

    [Header("References")]
    public GameRules rules; // ScriptableObject with all the balance numbers
    public EventManager eventManager;
    public UIController_Game uiController; // Reference to game screen UI
    public AudioManager audioManager;

    void Awake()
    {
        // Why: Singleton setup - GameManager lives in Bootstrap scene
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Called from BandSetupScene when player finishes selecting their band
    /// </summary>
    public void SetupNewGame(SlotData[] selectedBand, string bandName)
    {
        // Why: Initialize game with selected band members
        this.bandName = bandName;

        // Copy all slots from setup (up to 6)
        for (int i = 0; i < selectedBand.Length && i < slots.Length; i++)
        {
            slots[i] = selectedBand[i];
        }

        // Reset game state to starting values
        currentQuarter = 0;
        currentYear = 1;

        // Use rules if available, otherwise use defaults
        if (rules != null)
        {
            money = rules.startingMoney;
            fans = rules.startingFans;
        }
        else
        {
            money = 500;
            fans = 50;
        }

        unity = 100;
        flags.Clear();

        // Calculate initial band stats from members
        RecalculateStats();

        // ✅ Mark this as a new game so Game scene can check for starting events
        isNewGame = true;

        Debug.Log($"🎸 Band '{bandName}' is ready! Starting Year 1 Quarter 1");
    }

    /// <summary>
    /// Sums up all band member stats
    /// </summary>
    public void RecalculateStats()
    {
        technical = 0;
        performance = 0;
        charisma = 0;

        // Sum stats from first 3 slots (band members)
        for (int i = 0; i < 3; i++)
        {
            if (slots[i] != null)
            {
                technical += slots[i].technical;
                performance += slots[i].performance;
                charisma += slots[i].charisma;
            }
        }
    }

    /// <summary>
    /// Main action processor - player chose one of 5 actions
    /// </summary>
    public void DoAction(ActionType action)
    {
        // Process the action
        switch (action)
        {
            case ActionType.Record:
                DoRecord();
                break;
            case ActionType.Tour:
                DoTour();
                break;
            case ActionType.Practice:
                DoPractice();
                break;
            case ActionType.Rest:
                DoRest();
                break;
            case ActionType.Release:
                DoRelease();
                break;
        }

        // Why: After action, advance time by 1 quarter
        AdvanceQuarter();
    }

    /// <summary>
    /// Called by EventManager when it first loads
    /// Checks for Y1Q1 events if this is a new game
    /// </summary>
    public void OnGameSceneLoaded()
    {
        // Why: If this is a fresh game start, check for starting events (Y1 Q1)
        if (isNewGame && eventManager != null)
        {
            Debug.Log("========================================");
            Debug.Log("🎮 NEW GAME START - CHECKING FOR Y1 Q1 EVENTS");
            Debug.Log($"   Current State: Year {currentYear}, Quarter {currentQuarter} (displays as Q{(currentQuarter % 4) + 1})");
            Debug.Log($"   EventManager has {eventManager.allEvents.Count} events in database");
            Debug.Log("========================================");

            eventManager.CheckForEvents();
            isNewGame = false; // Only check once
        }
        else if (!isNewGame)
        {
            Debug.Log("⚠️ OnGameSceneLoaded called but isNewGame is false - skipping Y1Q1 check");
        }
        else if (eventManager == null)
        {
            Debug.LogError("❌ OnGameSceneLoaded called but eventManager is null!");
        }
    }

    /// <summary>
    /// DEBUG: Manually trigger event check - useful for testing
    /// Call this from Inspector or console to force an event check
    /// </summary>
    [ContextMenu("Force Check Events Now")]
    public void DEBUG_ForceCheckEvents()
    {
        if (eventManager != null)
        {
            Debug.Log("🔧 DEBUG: Manual event check triggered");
            eventManager.CheckForEvents();
        }
        else
        {
            Debug.LogError("❌ DEBUG: Cannot check events - EventManager is null!");
        }
    }

    /// <summary>
    /// Advances game time by one quarter
    /// Checks for events, updates displays
    /// </summary>
    public void AdvanceQuarter()
    {
        currentQuarter++;

        // Every 4 quarters = 1 year
        if (currentQuarter % 4 == 0)
        {
            currentYear++;
        }

        // Check if game is over (40 quarters = 10 years)
        if (currentQuarter >= 40)
        {
            EndGame();
            return;
        }

        // ✅ FIXED: Check for events (this was commented out!)
        if (eventManager != null)
        {
            eventManager.CheckForEvents();
        }
        else
        {
            Debug.LogWarning("⚠️ EventManager is null - cannot check for events!");
        }

        // Why: Update UI to show new quarter/year
        RefreshUI();
    }

    /// <summary>
    /// Refreshes the UI - finds UIController_Game if needed
    /// </summary>
    private void RefreshUI()
    {
        // Find UIController_Game if reference is missing
        if (uiController == null)
        {
            uiController = FindObjectOfType<UIController_Game>();
        }

        if (uiController != null)
        {
            uiController.RefreshDisplay();
        }
        else
        {
            Debug.LogWarning("UIController_Game not found - cannot update display");
        }
    }

    /// <summary>
    /// Public method to force UI refresh from external scripts
    /// </summary>
    public void ForceRefreshUI()
    {
        RefreshUI();
    }

    // ============================================
    // ACTION IMPLEMENTATIONS
    // ============================================

    private void DoRecord()
    {
        // Why: Recording music costs money but builds fanbase
        int cost = 200;

        if (money < cost)
        {
            Debug.Log("❌ RECORD: Not enough money! Need $" + cost);
            return;
        }

        money -= cost;

        // Fans gained based on band skill
        int skillLevel = (technical + performance + charisma) / 3;
        int fansGained = skillLevel * 10;
        fans += fansGained;

        // Studio time is draining
        unity -= 5;
        unity = Mathf.Clamp(unity, 0, 100);

        Debug.Log($"🎵 RECORD: -${cost}, +{fansGained} fans, -5 unity");
    }

    private void DoTour()
    {
        // Why: Touring costs money upfront but gains fans and revenue
        int cost = 300;

        if (money < cost)
        {
            Debug.Log("❌ TOUR: Not enough money! Need $" + cost);
            return;
        }

        money -= cost;

        // Revenue from tour based on fanbase + performance skill
        int revenue = (fans / 10) + (performance * 20);
        money += revenue;

        // Fans gained from live exposure
        int fansGained = performance * 5;
        fans += fansGained;

        // Touring is exhausting
        unity -= 10;
        unity = Mathf.Clamp(unity, 0, 100);

        Debug.Log($"🎤 TOUR: -${cost}, +${revenue}, +{fansGained} fans, -10 unity");
    }

    private void DoPractice()
    {
        // Why: Practice improves skills but costs time
        technical += 2;
        performance += 2;
        charisma += 1;

        // Small unity gain from working together
        unity += 5;
        unity = Mathf.Clamp(unity, 0, 100);

        Debug.Log("🎸 PRACTICE: +2 technical, +2 performance, +1 charisma, +5 unity");
    }

    private void DoRest()
    {
        // Why: Resting recovers unity and prevents burnout
        unity += 20;
        unity = Mathf.Clamp(unity, 0, 100);

        // Small money cost (living expenses)
        money -= 50;

        Debug.Log("😴 REST: +20 unity, -$50 living expenses");
    }

    private void DoRelease()
    {
        // Why: Release an album - big fanbase boost if you have enough tracks
        // For prototype: simplified - just big fan gain based on skills

        int skillLevel = (technical + performance + charisma) / 3;
        int fansGained = skillLevel * 50;
        fans += fansGained;

        // Promotion costs
        int cost = 400;
        if (money < cost)
        {
            Debug.Log("❌ RELEASE: Not enough money! Need $" + cost);
            return;
        }

        money -= cost;

        Debug.Log($"💿 RELEASE: -${cost}, +{fansGained} fans");
    }

    // ============================================
    // GAME END
    // ============================================

    private void EndGame()
    {
        Debug.Log("🎉 GAME OVER - 10 years complete!");
        // TODO: Load end scene with results
    }
}

// Why: Enum for the 5 action types
public enum ActionType
{
    Record,
    Tour,
    Practice,
    Rest,
    Release
}