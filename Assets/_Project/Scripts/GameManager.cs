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
        // Why: Singleton setup - this GameManager persists between scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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

        // Assign first 3 slots to main band members
        for (int i = 0; i < 3; i++)
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

        // Only sum first 3 slots (main band members)
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

        // Why: Update UI after every action so player sees changes
        if (uiController != null)
        {
            uiController.UpdateStatsDisplay();
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

        // Check for random events
        if (eventManager != null)
        {
            // eventManager.CheckForEvents();
        }

        // Why: Update UI to show new quarter/year
        if (uiController != null)
        {
            uiController.UpdateStatsDisplay();
        }
    }

    // ============================================
    // ACTION IMPLEMENTATIONS
    // ============================================

    private void DoRecord()
    {
        // Why: Recording creates music and gains fans/money based on band skill
        int successRoll = CalculateSuccess();

        money += successRoll * 10;
        fans += successRoll * 5;

        Debug.Log("RECORD: Success roll = " + successRoll);
    }

    private void DoTour()
    {
        // Why: Touring costs money but gains lots of fans
        int cost = 100;

        if (money >= cost)
        {
            money -= cost;
            int successRoll = CalculateSuccess();
            fans += successRoll * 15;

            Debug.Log("TOUR: Success roll = " + successRoll);
        }
        else
        {
            Debug.Log("TOUR: Not enough money!");
        }
    }

    private void DoPractice()
    {
        // Why: Practice improves band stats
        // TODO: Implement stat improvement logic
        Debug.Log("PRACTICE: Band is practicing...");
    }

    private void DoRest()
    {
        // Why: Rest recovers unity/morale
        unity += 10;
        unity = Mathf.Clamp(unity, 0, 100);

        Debug.Log("REST: Unity restored");
    }

    private void DoRelease()
    {
        // Why: Release an album for big money/fan gain
        int successRoll = CalculateSuccess();

        money += successRoll * 50;
        fans += successRoll * 25;

        Debug.Log("RELEASE: Success roll = " + successRoll);
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