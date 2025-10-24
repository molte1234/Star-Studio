using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // Singleton pattern - only one GameManager exists
    public static GameManager Instance;

    [Header("Band Info")]
    public string bandName;
    public SlotData[] slots = new SlotData[6]; // 3 main + 3 support/equipment

    [Header("Time")]
    public int currentQuarter = 0; // 0-79 (80 quarters = 20 years)
    public int currentYear = 1;     // 1-20

    [Header("Resources")]
    public int money = 500;
    public int fans = 50;

    [Header("Band Stats")]
    public int technical = 0;    // Calculated from band members
    public int performance = 0;
    public int charisma = 0;
    public int unity = 100;      // Band cohesion (0-100)

    [Header("Story Flags")]
    public List<string> flags = new List<string>(); // Track story progression

    [Header("References")]
    public GameRules rules; // ScriptableObject with all the numbers
    public EventManager eventManager;
    public UIManager uiManager;

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

    public void SetupNewGame(SlotData[] selectedBand, string bandName)
    {
        // Why: Called from BandSetupScene, initializes new game
        this.bandName = bandName;

        for (int i = 0; i < 3; i++)
        {
            slots[i] = selectedBand[i];
        }

        currentQuarter = 0;
        currentYear = 1;
        money = rules.startingMoney;
        fans = rules.startingFans;
        unity = 100;

        CalculateBandStats();
    }

    public void CalculateBandStats()
    {
        // Why: Sum up stats from all active band members
        technical = 0;
        performance = 0;
        charisma = 0;

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

    public void DoAction(ActionType actionType)
    {
        // Why: Player clicked an action button, process the results
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

        AdvanceQuarter();
    }

    private void DoRecord()
    {
        // Why: Spend money to create music, gain creativity
        money -= rules.recordCost;
        // Add recording logic here
    }

    private void DoTour()
    {
        // Why: Earn money and fans, lose unity
        int earnings = performance * rules.tourMoneyMultiplier;
        money += earnings;
        fans += rules.tourFanGain;
        unity -= rules.tourUnityCost;
    }

    private void DoPractice()
    {
        // Why: Improve stats slightly
        technical += rules.practiceStatGain;
        performance += rules.practiceStatGain;
    }

    private void DoRest()
    {
        // Why: Recover unity
        unity += rules.restUnityGain;
        unity = Mathf.Min(unity, 100); // Cap at 100
    }

    private void DoRelease()
    {
        // Why: Release album, gain fans based on quality
        // Add release logic here
    }

    public void AdvanceQuarter()
    {
        // Why: Move time forward, check for events and win/lose
        currentQuarter++;

        if (currentQuarter % 4 == 0)
        {
            currentYear++;
        }

        uiManager.UpdateUI();
        eventManager.CheckForEvents();
        CheckGameOver();
    }

    private void CheckGameOver()
    {
        // Why: Check win/lose conditions
        if (money <= 0)
        {
            GameOver("Bankruptcy!");
        }
        if (unity <= 0)
        {
            GameOver("Band broke up!");
        }
        if (currentQuarter >= 80)
        {
            GameWin();
        }
    }

    private void GameOver(string reason)
    {
        Debug.Log("Game Over: " + reason);
        // Load EndScene
    }

    private void GameWin()
    {
        Debug.Log("You survived 20 years!");
        // Load EndScene with victory
    }
}

// Why: Enum makes action types clear and safe
public enum ActionType
{
    Record,
    Tour,
    Practice,
    Rest,
    Release
}