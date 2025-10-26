using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Singleton GameManager - holds all game state and processes player actions
/// This is the brain of the game
/// NOTE: Quarter advancement is now handled by TimeManager (continuous time)
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton pattern - only one GameManager exists
    public static GameManager Instance;

    [Header("Game State")]
    [Tooltip("Check this to trigger welcome screen on next game scene load (for testing)")]
    public bool isNewGame = true;

    [Tooltip("Enable to manually control 'Is New Game' - prevents code from changing it")]
    public bool testingMode = false;

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

        // Copy band members
        for (int i = 0; i < selectedBand.Length; i++)
        {
            slots[i] = selectedBand[i];
        }

        // Calculate starting stats from band members
        RecalculateStats();

        Debug.Log($"🎸 New Game Setup Complete: {bandName}");
        Debug.Log($"   Starting Year 1 Quarter 1");
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
    /// NOTE: Actions NO LONGER advance quarters automatically!
    /// Time advances continuously via TimeManager
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

        // Why: Update UI after action
        // NOTE: We DON'T call AdvanceQuarter() anymore - TimeManager handles that!
        RefreshUI();
    }

    /// <summary>
    /// Called by EventManager when it first loads
    /// Shows welcome screen if this is a new game
    /// </summary>
    public void OnGameSceneLoaded()
    {
        // Why: If this is a fresh game start, show welcome screen
        if (isNewGame && eventManager != null)
        {
            Debug.Log("========================================");
            Debug.Log("🎮 NEW GAME START - SHOWING WELCOME SCREEN");
            Debug.Log($"   Current State: Year {currentYear}, Quarter {currentQuarter} (displays as Q{(currentQuarter % 4) + 1})");
            Debug.Log("========================================");

            eventManager.ShowWelcomeScreen();  // ✅ Show welcome screen (bypasses all conditions)

            // Only auto-clear isNewGame if not in testing mode
            if (!testingMode)
            {
                isNewGame = false; // Only show once
            }
        }
        else if (!isNewGame)
        {
            Debug.Log("⚠️ OnGameSceneLoaded called but isNewGame is false - skipping welcome screen");
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
    /// DEBUG: Manually show welcome screen - useful for testing
    /// </summary>
    [ContextMenu("Show Welcome Screen")]
    public void DEBUG_ShowWelcomeScreen()
    {
        if (eventManager != null)
        {
            Debug.Log("🔧 DEBUG: Manually showing welcome screen");
            eventManager.ShowWelcomeScreen();
        }
        else
        {
            Debug.LogError("❌ DEBUG: Cannot show welcome screen - EventManager is null!");
        }
    }

    /// <summary>
    /// Advances game time by one quarter
    /// NOW CALLED BY TIMEMANAGER automatically every X seconds!
    /// Checks for events, updates displays
    /// </summary>
    public void AdvanceQuarter()
    {
        currentQuarter++;

        // Every 4 quarters = 1 year
        if (currentQuarter % 4 == 0)
        {
            currentYear++;

            // Why: Play year change sound (bigger, more dramatic)
            if (audioManager != null)
            {
                audioManager.PlayYearAdvance();
            }

            Debug.Log($"📅 YEAR ADVANCED: Year {currentYear}");
        }
        else
        {
            // Why: Play quarter change sound (lighter click)
            if (audioManager != null)
            {
                audioManager.PlayQuarterAdvance();
            }

            Debug.Log($"📅 QUARTER ADVANCED: Year {currentYear}, Quarter {(currentQuarter % 4) + 1}");
        }

        // Check if game is over (40 quarters = 10 years)
        if (currentQuarter >= 40)
        {
            EndGame();
            return;
        }

        // Check for events
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
            Debug.Log("❌ RECORD: Not enough money!");
            return;
        }

        money -= cost;
        fans += 10 + (charisma / 3); // More charisma = better marketing

        Debug.Log($"🎵 RECORD: -${cost}, +{10 + (charisma / 3)} fans");
    }

    private void DoTour()
    {
        // Why: Touring earns money and fans but drains unity
        int cost = rules.tourCost;

        if (money < cost)
        {
            Debug.Log("❌ TOUR: Not enough money!");
            return;
        }

        money -= cost;
        int earnings = performance * rules.tourMoneyMultiplier;
        money += earnings;
        fans += rules.tourFanGain;
        unity -= rules.tourUnityCost;

        // Clamp unity to 0-100
        unity = Mathf.Clamp(unity, 0, 100);

        Debug.Log($"🎤 TOUR: -${cost}, +${earnings}, +{rules.tourFanGain} fans, -{rules.tourUnityCost} unity");
    }

    private void DoPractice()
    {
        // Why: Practice improves band stats
        technical += rules.practiceStatGain;
        performance += rules.practiceStatGain;
        charisma += rules.practiceStatGain;

        Debug.Log($"🎸 PRACTICE: +{rules.practiceStatGain} to all stats");
    }

    private void DoRest()
    {
        // Why: Rest recovers unity
        unity += rules.restUnityGain;
        unity = Mathf.Clamp(unity, 0, 100);

        Debug.Log($"😌 REST: +{rules.restUnityGain} unity");
    }

    private void DoRelease()
    {
        // Why: Release album - big fan boost based on stats
        int fanGain = (technical + performance + charisma) / 3;
        fans += fanGain;

        Debug.Log($"💿 RELEASE: +{fanGain} fans");
    }

    // ============================================
    // GAME OVER
    // ============================================

    private void EndGame()
    {
        Debug.Log("========================================");
        Debug.Log("🎊 GAME OVER - Reached 10 years!");
        Debug.Log($"   Final Stats: ${money}, {fans} fans");
        Debug.Log("========================================");

        // Why: Load the GameOver scene
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadGameOver();
        }
        else
        {
            Debug.LogError("❌ SceneLoader.Instance is null - cannot load GameOver scene!");
        }
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