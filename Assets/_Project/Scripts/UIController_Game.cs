using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls all UI elements on the main game screen
/// Updates displays, handles button clicks
/// FIXED: Progress bars fill upwards, character indices set for cancel buttons
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
    /// UPDATED: Now sets character index on each display for cancel button functionality
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

            // ✅ NEW: Set character index so cancel button knows which character to cancel
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

    public void RefreshUI()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("❌ UIController_Game.RefreshUI(): GameManager.Instance is null!");
            return;
        }

        GameManager gm = GameManager.Instance;

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

        RefreshCharacters();
    }

    public void RefreshDisplay()
    {
        RefreshUI();
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

    /// <summary>
    /// Update character displays with current action states
    /// Called every frame from ActionManager.Update() via RefreshUI()
    /// </summary>
    private void RefreshCharacters()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("❌ UIController_Game.RefreshCharacters(): GameManager.Instance is null!");
            return;
        }

        GameManager gm = GameManager.Instance;

        // Update all character displays
        for (int i = 0; i < characterDisplays.Length; i++)
        {
            if (characterDisplays[i] == null)
            {
                Debug.LogWarning($"characterDisplays[{i}] is null - not assigned in Inspector!");
                continue;
            }

            // Check if character exists in this slot
            if (i < gm.characterStates.Length && gm.characterStates[i] != null && gm.characterStates[i].slotData != null)
            {
                // Update character data (in case it changed)
                characterDisplays[i].SetCharacter(gm.characterStates[i].slotData);

                // Update busy state for progress bars
                CharacterSlotState charState = gm.characterStates[i];
                if (charState.isBusy && charState.currentAction != null)
                {
                    // Calculate progress using actual duration (with stat bonuses)
                    float totalTime = charState.actionTotalDuration;
                    float timeElapsed = totalTime - charState.actionTimeRemaining;
                    float progress = (totalTime > 0) ? (timeElapsed / totalTime) : 0f;

                    // ✅ Progress now fills upwards (0→1) instead of counting down
                    characterDisplays[i].SetBusyState(true, charState.currentAction.actionName, progress);
                }
                else
                {
                    characterDisplays[i].SetBusyState(false);
                }
            }
        }
    }

    // Menu toggle handlers
    public void OnPracticeMenuToggled(bool isOn) { ShowHidePanel(practicePanel, isOn); }
    public void OnProduceMenuToggled(bool isOn) { ShowHidePanel(producePanel, isOn); }
    public void OnTourMenuToggled(bool isOn) { ShowHidePanel(tourPanel, isOn); }
    public void OnReleaseMenuToggled(bool isOn) { ShowHidePanel(releasePanel, isOn); }
    public void OnFinanceMenuToggled(bool isOn) { ShowHidePanel(financePanel, isOn); }
    public void OnMarketingMenuToggled(bool isOn) { ShowHidePanel(marketingPanel, isOn); }
    public void OnManagingMenuToggled(bool isOn) { ShowHidePanel(managingPanel, isOn); }

    private void ShowHidePanel(GameObject panel, bool shouldShow)
    {
        if (panel != null)
        {
            panel.SetActive(shouldShow);
        }
    }
}