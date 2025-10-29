using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    [Tooltip("Check this to trigger welcome screen on next game scene load (for testing)")]
    public bool isNewGame = true;

    [Tooltip("Enable to manually control 'Is New Game' - prevents code from changing it")]
    public bool testingMode = false;

    [Header("Band Info")]
    public string bandName;

    /// <summary>
    /// Runtime state wrappers for each slot (wraps SlotData + runtime info)
    /// NOTE: This won't show in Inspector because it's a custom class
    /// Use the debug buttons below to see band members
    /// </summary>
    public CharacterSlotState[] characterStates = new CharacterSlotState[6];

    [Header("🔍 DEBUG: Current Band Members (Read-Only Display)")]
    [Tooltip("These are automatically updated - just for viewing in Inspector")]
    [SerializeField] private string slot0_Name = "EMPTY";
    [SerializeField] private string slot1_Name = "EMPTY";
    [SerializeField] private string slot2_Name = "EMPTY";
    [SerializeField] private string slot3_Name = "EMPTY";
    [SerializeField] private string slot4_Name = "EMPTY";
    [SerializeField] private string slot5_Name = "EMPTY";

    [Header("Time")]
    public int currentQuarter = 0;
    public int currentYear = 1;

    [Header("Resources")]
    public int money = 500;
    public int fans = 50;

    [Header("Band Stats - Total from All Members")]
    [Tooltip("Sum of all band members' charisma")]
    public int charisma = 0;

    [Tooltip("Sum of all band members' stage performance")]
    public int stagePerformance = 0;

    [Tooltip("Sum of all band members' vocal")]
    public int vocal = 0;

    [Tooltip("Sum of all band members' instrument")]
    public int instrument = 0;

    [Tooltip("Sum of all band members' songwriting")]
    public int songwriting = 0;

    [Tooltip("Sum of all band members' production")]
    public int production = 0;

    [Tooltip("Sum of all band members' management")]
    public int management = 0;

    [Tooltip("Sum of all band members' practical")]
    public int practical = 0;

    [Tooltip("Band cohesion (0-100)")]
    public int unity = 100;

    [Header("Story Flags")]
    public List<string> flags = new List<string>();

    [Header("References")]
    public GameRules rules;
    public EventManager eventManager;
    public UIController_Game uiController;
    public AudioManager audioManager;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Why: Update the debug display every frame so you can see band members in Inspector
        UpdateDebugDisplay();
    }

    /// <summary>
    /// Updates the read-only debug fields in Inspector
    /// </summary>
    private void UpdateDebugDisplay()
    {
        slot0_Name = GetSlotDebugName(0);
        slot1_Name = GetSlotDebugName(1);
        slot2_Name = GetSlotDebugName(2);
        slot3_Name = GetSlotDebugName(3);
        slot4_Name = GetSlotDebugName(4);
        slot5_Name = GetSlotDebugName(5);
    }

    private string GetSlotDebugName(int index)
    {
        if (characterStates[index] != null && characterStates[index].slotData != null)
        {
            string busyStatus = characterStates[index].isBusy ? " [BUSY]" : "";
            return $"{characterStates[index].slotData.displayName}{busyStatus}";
        }
        return "EMPTY";
    }

    /// <summary>
    /// DEBUG: Print all band members to console
    /// </summary>
    [ContextMenu("🔍 DEBUG: Show Band Members")]
    public void DEBUG_ShowBandMembers()
    {
        Debug.Log("========================================");
        Debug.Log($"🎸 BAND: {bandName}");
        Debug.Log("========================================");

        for (int i = 0; i < characterStates.Length; i++)
        {
            if (characterStates[i] != null && characterStates[i].slotData != null)
            {
                SlotData data = characterStates[i].slotData;
                Debug.Log($"   Slot {i}: {data.displayName}");
                Debug.Log($"      Stats - CHA:{data.charisma} STAGE:{data.stagePerformance} VOC:{data.vocal} INST:{data.instrument}");
                Debug.Log($"              SONG:{data.songwriting} PROD:{data.production} MGT:{data.management} PRAC:{data.practical}");
            }
            else
            {
                Debug.Log($"   Slot {i}: EMPTY");
            }
        }

        Debug.Log("========================================");
        Debug.Log($"📊 TOTAL BAND STATS:");
        Debug.Log($"   CHA:{charisma} STAGE:{stagePerformance} VOC:{vocal} INST:{instrument}");
        Debug.Log($"   SONG:{songwriting} PROD:{production} MGT:{management} PRAC:{practical}");
        Debug.Log("========================================");
    }

    /// <summary>
    /// DEBUG: Manually force create test band (useful when Start() order is wrong)
    /// </summary>
    [ContextMenu("🔧 DEBUG: Force Create Test Band")]
    public void DEBUG_ForceCreateTestBand()
    {
        Debug.Log("🔧 Manually triggering TestBandHelper...");

        TestBandHelper testHelper = GetComponent<TestBandHelper>();
        if (testHelper != null)
        {
            testHelper.CheckAndCreateTestBand();
            RefreshUI();
        }
        else
        {
            Debug.LogError("❌ TestBandHelper component not found on GameManager!");
        }
    }

    /// <summary>
    /// Called by EventManager when it loads in Game scene
    /// Since EventManager lives in a different scene, it can't be hardlinked
    /// </summary>
    public void RegisterEventManager(EventManager manager)
    {
        eventManager = manager;
        Debug.Log("✅ EventManager registered with GameManager");

        // Why: After EventManager registers, check if we should show welcome screen
        CheckForWelcomeScreen();
    }

    /// <summary>
    /// Called by UIController_Game when it loads in Game scene
    /// Since UIController_Game lives in a different scene, it can't be hardlinked
    /// </summary>
    public void RegisterUIController(UIController_Game controller)
    {
        uiController = controller;
        Debug.Log("✅ UIController_Game registered with GameManager");
    }

    /// <summary>
    /// Check if we should show welcome screen (called after EventManager registers)
    /// </summary>
    private void CheckForWelcomeScreen()
    {
        if (isNewGame && eventManager != null)
        {
            Debug.Log("========================================");
            Debug.Log("🎮 NEW GAME START - SHOWING WELCOME SCREEN");
            Debug.Log($"   Current State: Year {currentYear}, Quarter {(currentQuarter % 4) + 1}");
            Debug.Log("========================================");

            eventManager.ShowWelcomeScreen();

            if (!testingMode)
            {
                isNewGame = false;
            }
        }
    }

    public void SetupNewGame(SlotData[] selectedBand, string bandName)
    {
        this.bandName = bandName;

        for (int i = 0; i < selectedBand.Length; i++)
        {
            if (selectedBand[i] != null)
            {
                characterStates[i] = new CharacterSlotState(selectedBand[i]);
            }
            else
            {
                characterStates[i] = null;
            }
        }

        RecalculateStats();

        Debug.Log("========================================");
        Debug.Log($"🎸 New Game Setup Complete: {bandName}");
        Debug.Log($"   Starting Year {currentYear}, Quarter {currentQuarter + 1}");
        Debug.Log($"   Band Members:");
        for (int i = 0; i < characterStates.Length; i++)
        {
            if (characterStates[i] != null && characterStates[i].slotData != null)
            {
                Debug.Log($"      Slot {i}: {characterStates[i].slotData.displayName}");
            }
        }
        Debug.Log("========================================");
    }

    public void RecalculateStats()
    {
        charisma = 0;
        stagePerformance = 0;
        vocal = 0;
        instrument = 0;
        songwriting = 0;
        production = 0;
        management = 0;
        practical = 0;

        for (int i = 0; i < characterStates.Length; i++)
        {
            if (characterStates[i] != null && characterStates[i].slotData != null)
            {
                SlotData data = characterStates[i].slotData;
                charisma += data.charisma;
                stagePerformance += data.stagePerformance;
                vocal += data.vocal;
                instrument += data.instrument;
                songwriting += data.songwriting;
                production += data.production;
                management += data.management;
                practical += data.practical;
            }
        }

        Debug.Log($"📊 Band Stats: CHA:{charisma} STAGE:{stagePerformance} VOC:{vocal} INST:{instrument} SONG:{songwriting} PROD:{production} MGT:{management} PRAC:{practical}");
    }

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

    public void DoAction(ActionType action)
    {
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

        RefreshUI();
    }

    public void AdvanceQuarter()
    {
        currentQuarter++;

        if (currentQuarter % 4 == 0)
        {
            currentYear++;

            if (audioManager != null)
            {
                audioManager.PlayYearAdvance();
            }

            Debug.Log($"📅 YEAR ADVANCED: Year {currentYear}");
        }
        else
        {
            if (audioManager != null)
            {
                audioManager.PlayQuarterAdvance();
            }

            Debug.Log($"📅 QUARTER ADVANCED: Year {currentYear}, Quarter {(currentQuarter % 4) + 1}");
        }

        if (currentQuarter >= 40)
        {
            EndGame();
            return;
        }

        if (eventManager != null)
        {
            eventManager.CheckForEvents();
        }
        else
        {
            Debug.LogWarning("⚠️ EventManager is null - cannot check for events!");
        }

        RefreshUI();
    }

    private void RefreshUI()
    {
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

    public void ForceRefreshUI()
    {
        RefreshUI();
    }

    private void DoRecord()
    {
        int cost = 200;

        if (money < cost)
        {
            Debug.Log("❌ RECORD: Not enough money!");
            return;
        }

        money -= cost;
        fans += 10 + (charisma / 3);

        Debug.Log($"🎵 RECORD: -${cost}, +{10 + (charisma / 3)} fans");
    }

    private void DoTour()
    {
        int cost = rules.tourCost;

        if (money < cost)
        {
            Debug.Log("❌ TOUR: Not enough money!");
            return;
        }

        money -= cost;
        int earnings = stagePerformance * rules.tourMoneyMultiplier;
        money += earnings;
        fans += rules.tourFanGain;
        unity -= rules.tourUnityCost;

        unity = Mathf.Clamp(unity, 0, 100);

        Debug.Log($"🎤 TOUR: -${cost}, +${earnings}, +{rules.tourFanGain} fans, -{rules.tourUnityCost} unity");
    }

    private void DoPractice()
    {
        stagePerformance += rules.practiceStatGain;
        vocal += rules.practiceStatGain;
        instrument += rules.practiceStatGain;

        Debug.Log($"🎸 PRACTICE: +{rules.practiceStatGain} to performance stats");
    }

    private void DoRest()
    {
        unity += rules.restUnityGain;
        unity = Mathf.Clamp(unity, 0, 100);

        Debug.Log($"😌 REST: +{rules.restUnityGain} unity");
    }

    private void DoRelease()
    {
        int fanGain = (songwriting + production + charisma) / 3;
        fans += fanGain;

        Debug.Log($"💿 RELEASE: +{fanGain} fans");
    }

    private void EndGame()
    {
        Debug.Log("========================================");
        Debug.Log("🎊 GAME OVER - Reached 10 years!");
        Debug.Log($"   Final Stats: ${money}, {fans} fans");
        Debug.Log("========================================");

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

public enum ActionType
{
    Record,
    Tour,
    Practice,
    Rest,
    Release
}