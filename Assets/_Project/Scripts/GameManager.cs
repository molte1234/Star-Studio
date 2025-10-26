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
        this.bandName = bandName;

        // Copy all slots from setup (up to 6)
        for (int i = 0; i < selectedBand.Length && i < slots.Length; i++)
        {
            slots[i] = selectedBand[i];
        }

        // Reset game state
        currentQuarter = 0;
        currentYear = 1;
        money = rules.startingMoney;
        fans = rules.startingFans;
        unity = 100;

        // Calculate initial band stats
        CalculateBandStats();
    }

    /// <summary>
    /// Sums up stats from all active band members
    /// Call this whenever band composition changes
    /// </summary>
    public void CalculateBandStats()
    {
        technical = 0;
        performance = 0;
        charisma = 0;

        // Sum stats from all filled slots
        for (int i = 0; i < slots.Length; i++)
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
    /// Player clicked an action button - process the results
    /// </summary>
    public void DoAction(ActionType actionType)
    {
        switch (actionType)
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

        // Why: Every action advances time by 1 quarter
        AdvanceQuarter();
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

        // Check for random events
        if (eventManager != null)
        {
            // eventManager.CheckForEvents();
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

        // Tour success based on performance and charisma
        int tourPower = (performance + charisma) / 2;
        int fansGained = tourPower * 20;
        int revenue = tourPower * 50;

        fans += fansGained;
        money += revenue;

        // Touring is exhausting
        unity -= 10;
        unity = Mathf.Clamp(unity, 0, 100);

        Debug.Log($"🎤 TOUR: -${cost}, +${revenue}, +{fansGained} fans, -10 unity");
    }

    private void DoPractice()
    {
        // Why: Practice is free and improves band skills
        // Small stat increase
        technical += 1;
        performance += 1;
        charisma += 1;

        // Cap stats at reasonable level
        technical = Mathf.Min(technical, 30);
        performance = Mathf.Min(performance, 30);
        charisma = Mathf.Min(charisma, 30);

        // Practice together builds unity
        unity += 5;
        unity = Mathf.Clamp(unity, 0, 100);

        Debug.Log($"🎸 PRACTICE: +1 to all stats, +5 unity");
    }

    private void DoRest()
    {
        // Why: Rest is free and recovers unity
        unity += 20;
        unity = Mathf.Clamp(unity, 0, 100);

        Debug.Log($"😴 REST: +20 unity");
    }

    private void DoRelease()
    {
        // Why: Releasing an album is expensive but can be very profitable
        int cost = 500;

        if (money < cost)
        {
            Debug.Log("❌ RELEASE: Not enough money! Need $" + cost);
            return;
        }

        money -= cost;

        // Release success based on all stats
        int bandPower = technical + performance + charisma;
        int fansGained = bandPower * 15;
        int revenue = bandPower * 100;

        fans += fansGained;
        money += revenue;

        // Exciting to release! Small unity boost
        unity += 10;
        unity = Mathf.Clamp(unity, 0, 100);

        Debug.Log($"💿 RELEASE: -${cost}, +${revenue}, +{fansGained} fans, +10 unity");
    }

    /// <summary>
    /// Calculates action success based on band stats
    /// Returns a value 1-10 based on combined band power
    /// </summary>
    private int CalculateSuccess()
    {
        // Simple formula: average of all stats
        int totalStats = technical + performance + charisma;
        int averageSkill = totalStats / 3;

        // Add some randomness (±2)
        int randomBonus = Random.Range(-2, 3);
        int result = averageSkill + randomBonus;

        // Clamp to 1-10
        return Mathf.Clamp(result, 1, 10);
    }

    private void EndGame()
    {
        Debug.Log("GAME OVER! 10 years complete!");
        // TODO: Load end scene with final score
    }
}

/// <summary>
/// Enum for the 5 action types
/// </summary>
public enum ActionType
{
    Record,
    Tour,
    Practice,
    Rest,
    Release
}