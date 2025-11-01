using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls all UI elements on the main game screen
/// 
/// ARCHITECTURE:
/// - RefreshCharacters(): Loop through ALL, read state from GameManager, set UI for everyone
/// - Update(): SEPARATELY update progress bars for busy characters only
/// - SelectCharacter(): Manage "only one selected" rule
/// - NO complex state tracking - just display current state
/// UPDATED: Added hover tracking and MiniStats panel integration
/// </summary>
public class UIController_Game : MonoBehaviour
{
    [Header("Band Info Display")]
    public TextMeshProUGUI bandNameText;

    [Header("Stats Display")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI fansText;
    public TextMeshProUGUI unityText;
    public TextMeshProUGUI quarterYearText;

    [Header("Band Display")]
    public CharacterDisplay[] characterDisplays; // Array of character displays (min 6, expandable)

    [Header("Menu Panels (Area A)")]
    public GameObject practicePanel;
    public GameObject producePanel;
    public GameObject tourPanel;
    public GameObject releasePanel;
    public GameObject financePanel;
    public GameObject marketingPanel;
    public GameObject managingPanel;

    [Header("Menu Toggle Group (Area B)")]
    public ToggleGroup menuToggleGroup; // The ToggleGroup that contains all 7 menu toggles

    [Header("Test Band (Optional)")]
    [Tooltip("Drag TestBandHelper here if you want to use test band on scene load")]
    public TestBandHelper testBandHelper;

    [Header("Stats Mini Panel")]
    [Tooltip("MiniStats component - shows character stats on hover/select")]
    public MiniStats miniStats;

    // ============================================
    // SELECTION & HOVER TRACKING
    // ============================================
    private int selectedCharacterIndex = -1; // -1 = none selected
    private int hoveredCharacterIndex = -1;  // -1 = no hover

    void Start()
    {
        Debug.Log("========================================");
        Debug.Log("🎮 UIController_Game.Start() BEGIN");
        Debug.Log("========================================");

        // Setup menus
        if (menuToggleGroup != null)
        {
            menuToggleGroup.allowSwitchOff = true;
        }
        HideAllPanels();

        // STEP 1: TestBand (if assigned and enabled)
        if (testBandHelper != null)
        {
            Debug.Log($"🔧 TestBandHelper assigned. useTestBand = {testBandHelper.useTestBand}");

            if (testBandHelper.useTestBand)
            {
                Debug.Log("🎸 Populating test band...");
                testBandHelper.PopulateTestBandIfEnabled();
            }
        }
        else
        {
            Debug.Log("⚠️ No TestBandHelper assigned in Inspector");
        }

        // STEP 2: Initialize character displays
        Debug.Log("🎨 Initializing character displays...");
        InitializeCharacterDisplays();

        // STEP 3: Register this controller with GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterUIController(this);
        }

        Debug.Log("========================================");
        Debug.Log("🎮 UIController_Game.Start() END");
        Debug.Log("========================================");
    }

    /// <summary>
    /// Initialize character display GameObjects (active/inactive) based on GameManager data
    /// Called once at Start()
    /// </summary>
    private void InitializeCharacterDisplays()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("⚠️ InitializeCharacterDisplays: GameManager.Instance is null!");
            return;
        }

        GameManager gm = GameManager.Instance;

        for (int i = 0; i < characterDisplays.Length; i++)
        {
            if (characterDisplays[i] == null)
            {
                Debug.LogWarning($"characterDisplays[{i}] is null - not assigned in Inspector!");
                continue;
            }

            // Set character index so cancel button knows which character to cancel
            characterDisplays[i].SetCharacterIndex(i);

            // Check if character exists in this slot
            bool hasCharacter = (i < gm.characterStates.Length &&
                                gm.characterStates[i] != null &&
                                gm.characterStates[i].slotData != null);

            // Set GameObject active/inactive
            characterDisplays[i].gameObject.SetActive(hasCharacter);

            // If character exists, set the data
            if (hasCharacter)
            {
                characterDisplays[i].SetCharacter(gm.characterStates[i].slotData);
                Debug.Log($"✅ Initialized CharacterDisplay {i}: {gm.characterStates[i].slotData.displayName}");
            }
            else
            {
                Debug.Log($"⚫ CharacterDisplay {i}: Empty (disabled)");
            }
        }
    }

    // ============================================
    // SEPARATE UPDATE LOOP FOR PROGRESS BARS
    // ============================================

    void Update()
    {
        UpdateProgressBars();
    }

    /// <summary>
    /// SEPARATE loop that updates progress bars ONLY for busy characters
    /// Runs every frame in Update()
    /// </summary>
    private void UpdateProgressBars()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        for (int i = 0; i < characterDisplays.Length; i++)
        {
            if (characterDisplays[i] == null) continue;

            // Check if character exists and is busy
            if (i < gm.characterStates.Length &&
                gm.characterStates[i] != null &&
                gm.characterStates[i].isBusy)
            {
                CharacterSlotState charState = gm.characterStates[i];

                // Calculate progress
                float totalTime = charState.actionTotalDuration;
                float timeElapsed = totalTime - charState.actionTimeRemaining;
                float progress = (totalTime > 0) ? (timeElapsed / totalTime) : 0f;

                // Update progress bar
                characterDisplays[i].UpdateProgress(progress);
            }
        }
    }

    // ============================================
    // HOVER TRACKING (for stats panel)
    // ============================================

    /// <summary>
    /// Called by CharacterDisplay when mouse enters portrait
    /// </summary>
    public void SetHoveredCharacter(int characterIndex)
    {
        hoveredCharacterIndex = characterIndex;
        UpdateStatsPanel();
    }

    /// <summary>
    /// Called by CharacterDisplay when mouse exits portrait
    /// </summary>
    public void ClearHoveredCharacter()
    {
        hoveredCharacterIndex = -1;
        UpdateStatsPanel();
    }

    // ============================================
    // CHARACTER SELECTION
    // ============================================

    /// <summary>
    /// Select a character (called by CharacterDisplay when clicked)
    /// Enforces "only one selected at a time" rule
    /// </summary>
    public void SelectCharacter(int characterIndex)
    {
        Debug.Log($"🎯 UIController: SelectCharacter({characterIndex}) called");

        // Why: Clicking same character again = deselect
        if (selectedCharacterIndex == characterIndex)
        {
            Debug.Log($"   Same character clicked - deselecting");
            DeselectAll();
            return;
        }

        // Why: Deselect old character first
        if (selectedCharacterIndex >= 0 && selectedCharacterIndex < characterDisplays.Length)
        {
            if (characterDisplays[selectedCharacterIndex] != null)
            {
                characterDisplays[selectedCharacterIndex].SetSelected(false);
                Debug.Log($"   Deselected character {selectedCharacterIndex}");
            }
        }

        // Why: Select new character
        selectedCharacterIndex = characterIndex;

        if (characterIndex >= 0 && characterIndex < characterDisplays.Length)
        {
            if (characterDisplays[characterIndex] != null)
            {
                characterDisplays[characterIndex].SetSelected(true);
                Debug.Log($"   ✅ Selected character {characterIndex}");
            }
        }

        // Update stats panel
        UpdateStatsPanel();
    }

    /// <summary>
    /// Deselect all characters
    /// </summary>
    public void DeselectAll()
    {
        if (selectedCharacterIndex >= 0 && selectedCharacterIndex < characterDisplays.Length)
        {
            if (characterDisplays[selectedCharacterIndex] != null)
            {
                characterDisplays[selectedCharacterIndex].SetSelected(false);
                Debug.Log($"⚪ Deselected character {selectedCharacterIndex}");
            }
        }

        selectedCharacterIndex = -1;

        // Update stats panel
        UpdateStatsPanel();
    }

    /// <summary>
    /// Get currently selected character index (-1 if none)
    /// </summary>
    public int GetSelectedCharacterIndex()
    {
        return selectedCharacterIndex;
    }

    // ============================================
    // STATS PANEL UPDATE
    // ============================================

    /// <summary>
    /// Update stats panel based on priority: Hover > Selected > Hidden
    /// </summary>
    private void UpdateStatsPanel()
    {
        // Skip if no MiniStats assigned
        if (miniStats == null) return;

        GameManager gm = GameManager.Instance;
        if (gm == null)
        {
            miniStats.Hide();
            return;
        }

        // Priority 1: Show hovered character
        if (hoveredCharacterIndex >= 0)
        {
            SlotData character = GetCharacterData(hoveredCharacterIndex);
            if (character != null)
            {
                miniStats.ShowCharacter(character);
                return;
            }
        }

        // Priority 2: Show selected character
        if (selectedCharacterIndex >= 0)
        {
            SlotData character = GetCharacterData(selectedCharacterIndex);
            if (character != null)
            {
                miniStats.ShowCharacter(character);
                return;
            }
        }

        // Priority 3: Hide panel
        miniStats.Hide();
    }

    /// <summary>
    /// Helper: Get character data by index
    /// </summary>
    private SlotData GetCharacterData(int characterIndex)
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return null;

        if (characterIndex < 0 || characterIndex >= gm.characterStates.Length)
            return null;

        if (gm.characterStates[characterIndex] == null)
            return null;

        return gm.characterStates[characterIndex].slotData;
    }

    // ============================================
    // REFRESH UI - Called by GameManager
    // ============================================

    public void RefreshUI()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("❌ UIController_Game.RefreshUI(): GameManager.Instance is null!");
            return;
        }

        GameManager gm = GameManager.Instance;

        // Update stats
        if (moneyText != null) moneyText.text = "$" + gm.money.ToString();
        if (fansText != null) fansText.text = gm.fans.ToString();
        if (unityText != null) unityText.text = gm.unity.ToString() + "%";

        if (quarterYearText != null)
        {
            int year = (gm.currentQuarter / 4) + 1;
            int quarter = (gm.currentQuarter % 4) + 1;
            quarterYearText.text = $"Q{quarter} YEAR {year}";
        }

        if (bandNameText != null) bandNameText.text = gm.bandName;

        // Update characters
        RefreshCharacters();
    }

    public void RefreshDisplay()
    {
        RefreshUI();
    }

    /// <summary>
    /// Loop through ALL characters and set their UI based on current GameManager state
    /// NO state tracking, NO change detection - just read and display
    /// </summary>
    private void RefreshCharacters()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("❌ UIController_Game.RefreshCharacters(): GameManager.Instance is null!");
            return;
        }

        GameManager gm = GameManager.Instance;

        // Loop through all characters
        for (int i = 0; i < characterDisplays.Length; i++)
        {
            if (characterDisplays[i] == null)
            {
                Debug.LogWarning($"characterDisplays[{i}] is null - not assigned in Inspector!");
                continue;
            }

            // Check if character exists in this slot
            if (i < gm.characterStates.Length &&
                gm.characterStates[i] != null &&
                gm.characterStates[i].slotData != null)
            {
                CharacterSlotState charState = gm.characterStates[i];

                // Update character data
                characterDisplays[i].SetCharacter(charState.slotData);

                // Set busy state (action UI)
                if (charState.isBusy && charState.currentAction != null)
                {
                    characterDisplays[i].SetBusyState(true, charState.currentAction.actionName);
                }
                else
                {
                    characterDisplays[i].SetBusyState(false);
                }
            }
        }
    }

    // ============================================
    // MENU CONTROLS
    // ============================================

    public void CloseAllMenus()
    {
        HideAllPanels();

        if (menuToggleGroup != null)
        {
            if (!menuToggleGroup.allowSwitchOff)
            {
                menuToggleGroup.allowSwitchOff = true;
                Debug.Log("✅ Enabled allowSwitchOff on menuToggleGroup");
            }

            menuToggleGroup.SetAllTogglesOff();
            Debug.Log("✅ All menu toggles turned off");
        }
        else
        {
            Debug.LogWarning("UIController_Game: menuToggleGroup is null - cannot untoggle menu buttons!");
        }
    }

    private void HideAllPanels()
    {
        if (practicePanel != null) practicePanel.SetActive(false);
        if (producePanel != null) producePanel.SetActive(false);
        if (tourPanel != null) tourPanel.SetActive(false);
        if (releasePanel != null) releasePanel.SetActive(false);
        if (financePanel != null) financePanel.SetActive(false);
        if (marketingPanel != null) marketingPanel.SetActive(false);
        if (managingPanel != null) managingPanel.SetActive(false);
    }

    // ============================================
    // MENU TOGGLE HANDLERS (Called by Toggle buttons)
    // ============================================

    public void OnPracticeToggle(bool isOn)
    {
        if (practicePanel != null) practicePanel.SetActive(isOn);
    }

    public void OnProduceToggle(bool isOn)
    {
        if (producePanel != null) producePanel.SetActive(isOn);
    }

    public void OnTourToggle(bool isOn)
    {
        if (tourPanel != null) tourPanel.SetActive(isOn);
    }

    public void OnReleaseToggle(bool isOn)
    {
        if (releasePanel != null) releasePanel.SetActive(isOn);
    }

    public void OnFinanceToggle(bool isOn)
    {
        if (financePanel != null) financePanel.SetActive(isOn);
    }

    public void OnMarketingToggle(bool isOn)
    {
        if (marketingPanel != null) marketingPanel.SetActive(isOn);
    }

    public void OnManagingToggle(bool isOn)
    {
        if (managingPanel != null) managingPanel.SetActive(isOn);
    }
}