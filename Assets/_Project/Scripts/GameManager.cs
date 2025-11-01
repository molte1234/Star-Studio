using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// CENTRAL GAME STATE MANAGER - Singleton pattern
/// Holds all game state and orchestrates game systems
/// Persists across scene changes (Bootstrap handles this)
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public string bandName = "";
    public int currentQuarter = 0; // 0-39 (40 quarters = 10 years)
    public int currentYear = 1; // For display only
    public bool isNewGame = true; // Used to show welcome screen once
    public bool testingMode = false; // Keep welcome screen enabled for testing

    [Header("Resources")]
    public int money = 500;
    public int fans = 50;

    [Header("✅ NEW 8-Stat System")]
    public int charisma = 0;
    public int stagePerformance = 0;
    public int vocal = 0;
    public int instrument = 0;
    public int songwriting = 0;
    public int production = 0;
    public int management = 0;
    public int practical = 0;
    public int unity = 100;

    [Header("Band Members")]
    public CharacterSlotState[] characterStates = new CharacterSlotState[6];

    [Header("Debug Display (Read-Only)")]
    [SerializeField] private string slot0_Name = "EMPTY";
    [SerializeField] private string slot1_Name = "EMPTY";
    [SerializeField] private string slot2_Name = "EMPTY";
    [SerializeField] private string slot3_Name = "EMPTY";
    [SerializeField] private string slot4_Name = "EMPTY";
    [SerializeField] private string slot5_Name = "EMPTY";

    [Header("Flags")]
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
            return characterStates[index].slotData.displayName + busyStatus;
        }
        return "EMPTY";
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

        Debug.Log($"📊 Stats Recalculated: CHA {charisma}, STG {stagePerformance}, VOC {vocal}, INS {instrument}, SNG {songwriting}, PRD {production}, MGT {management}, PRA {practical}");
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

    // ============================================
    // ACTION SYSTEM ORCHESTRATION
    // ============================================

    /// <summary>
    /// ORCHESTRATOR: Start an action
    /// Called from MemberSelectionPopup or ActionButton
    /// Changes state, then tells ActionManager to start timer and UIController to update
    /// </summary>
    public void StartAction(ActionData action, List<int> characterIndices)
    {
        if (action == null)
        {
            Debug.LogError("❌ GameManager.StartAction: action is null!");
            return;
        }

        Debug.Log($"🎬 GameManager: Starting action '{action.actionName}' with {characterIndices.Count} characters");

        // ============================================
        // 1. VALIDATE & CHANGE STATE
        // ============================================

        // Validate member requirements
        if (action.requiresMembers && characterIndices.Count < action.minMembers)
        {
            Debug.LogWarning($"⚠️ Not enough members! Need {action.minMembers}");
            return;
        }

        // Validate money
        if (money < action.baseCost)
        {
            Debug.LogWarning($"⚠️ Not enough money! Need ${action.baseCost}");
            return;
        }

        // Pay cost
        money -= action.baseCost;
        Debug.Log($"   💰 Paid ${action.baseCost} → Balance: ${money}");

        // Mark characters as busy
        foreach (int index in characterIndices)
        {
            characterStates[index].isBusy = true;
            characterStates[index].currentAction = action;
            Debug.Log($"   🔒 Character {index} now BUSY");
        }

        // ============================================
        // 2. TELL ACTIONMANAGER: "Start timer"
        // ============================================

        if (ActionManager.Instance != null)
        {
            ActionManager.Instance.StartTimer(action, characterIndices);
        }

        // ============================================
        // 3. TELL UICONTROLLER: "Update display"
        // ============================================

        if (uiController != null)
        {
            uiController.RefreshUI();
        }
    }

    /// <summary>
    /// ORCHESTRATOR: Complete an action
    /// Called from ActionManager when timer finishes
    /// Changes state, applies rewards, then tells UIController to update
    /// </summary>
    public void CompleteAction(int characterIndex)
    {
        CharacterSlotState charState = characterStates[characterIndex];
        if (charState == null || !charState.isBusy)
        {
            Debug.LogWarning($"⚠️ Cannot complete - character {characterIndex} not busy");
            return;
        }

        ActionData action = charState.currentAction;
        List<int> groupIndices = new List<int>(charState.groupedWithSlots);

        Debug.Log($"✅ GameManager: Completing action '{action.actionName}'");

        // ============================================
        // 1. APPLY REWARDS
        // ============================================

        money += action.rewardMoney;
        fans += action.rewardFans;
        Debug.Log($"   💰 Gained ${action.rewardMoney} | 👥 Gained {action.rewardFans} fans");

        // ============================================
        // 2. CHANGE STATE - UNLOCK CHARACTERS
        // ============================================

        foreach (int index in groupIndices)
        {
            characterStates[index].isBusy = false;
            characterStates[index].currentAction = null;
            characterStates[index].actionTimeRemaining = 0f;
            characterStates[index].actionTotalDuration = 0f;
            characterStates[index].groupedWithSlots.Clear();
            Debug.Log($"   🔓 Character {index} now IDLE");
        }

        // ============================================
        // 3. TELL UICONTROLLER: "Update display"
        // ============================================

        if (uiController != null)
        {
            uiController.RefreshUI();
        }
    }

    /// <summary>
    /// ORCHESTRATOR: Cancel an action for a SPECIFIC character
    /// Called from CharacterDisplay cancel button
    /// Only removes THIS character from the action, other group members continue
    /// </summary>
    public void CancelAction(int characterIndex)
    {
        CharacterSlotState charState = characterStates[characterIndex];
        if (charState == null || !charState.isBusy)
        {
            Debug.LogWarning($"⚠️ Cannot cancel - character {characterIndex} not busy");
            return;
        }

        ActionData action = charState.currentAction;
        List<int> originalGroup = new List<int>(charState.groupedWithSlots);

        Debug.Log($"❌ GameManager: Canceling character {characterIndex} from action '{action.actionName}'");
        Debug.Log($"   Original group: [{string.Join(", ", originalGroup)}]");

        // ============================================
        // 1. FREE THIS CHARACTER ONLY
        // ============================================

        characterStates[characterIndex].isBusy = false;
        characterStates[characterIndex].currentAction = null;
        characterStates[characterIndex].actionTimeRemaining = 0f;
        characterStates[characterIndex].actionTotalDuration = 0f;
        characterStates[characterIndex].groupedWithSlots.Clear();
        Debug.Log($"   🔓 Character {characterIndex} now IDLE");

        // ============================================
        // 2. UPDATE OTHER GROUP MEMBERS
        // Remove this character from their groupedWithSlots lists
        // ============================================

        foreach (int otherIndex in originalGroup)
        {
            if (otherIndex == characterIndex) continue; // Skip self

            if (characterStates[otherIndex] != null && characterStates[otherIndex].isBusy)
            {
                characterStates[otherIndex].groupedWithSlots.Remove(characterIndex);
                Debug.Log($"   🔗 Removed {characterIndex} from character {otherIndex}'s group");
                Debug.Log($"      New group for {otherIndex}: [{string.Join(", ", characterStates[otherIndex].groupedWithSlots)}]");
            }
        }

        // ============================================
        // 3. TELL ACTIONMANAGER: Stop timer for THIS character only
        // ============================================

        if (ActionManager.Instance != null)
        {
            ActionManager.Instance.StopTimer(new List<int> { characterIndex });
        }

        // ============================================
        // 4. TELL UICONTROLLER: Update display
        // ============================================

        if (uiController != null)
        {
            uiController.RefreshUI();
        }

        Debug.Log($"✅ Cancel complete - {originalGroup.Count - 1} characters still working on '{action.actionName}'");

        // TODO: Refund costs?
    }

    // ============================================
    // TIME SYSTEM
    // ============================================

    /// <summary>
    /// Advance to next quarter
    /// Called by TimeManager when quarter timer expires
    /// </summary>
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

    private void EndGame()
    {
        Debug.Log("🎊 GAME OVER: 10 years complete!");

        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadGameOver();
        }
    }
}