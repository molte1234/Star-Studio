using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls all UI in the main Game scene
/// Handles displaying stats, character portraits, and menu panels
/// UPDATED: Added CloseAllMenus() to return to main game view after actions
/// </summary>
public class UIController_Game : MonoBehaviour
{
    [Header("Stats Display - Resources")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI fansText;
    public TextMeshProUGUI unityText;
    public TextMeshProUGUI quarterYearText;
    public TextMeshProUGUI bandNameText;

    [Header("Character Displays")]
    public CharacterDisplay[] characterDisplays;

    [Header("Menu System - Area A (Action Panels)")]
    [Tooltip("Panel in Area A that contains Practice action buttons")]
    public GameObject practicePanel;
    [Tooltip("Panel in Area A that contains Produce action buttons")]
    public GameObject producePanel;
    [Tooltip("Panel in Area A that contains Tour action buttons")]
    public GameObject tourPanel;
    [Tooltip("Panel in Area A that contains Release action buttons")]
    public GameObject releasePanel;
    [Tooltip("Panel in Area A that contains Finance action buttons")]
    public GameObject financePanel;
    [Tooltip("Panel in Area A that contains Marketing action buttons")]
    public GameObject marketingPanel;
    [Tooltip("Panel in Area A that contains Managing action buttons")]
    public GameObject managingPanel;

    [Header("Menu System - Area B (Toggle Buttons)")]
    [Tooltip("Toggle Group that contains all menu buttons - needed to untoggle them")]
    public ToggleGroup menuToggleGroup;

    void Start()
    {
        // Why: Make sure all panels start hidden
        HideAllPanels();

        // Why: Register this UIController with GameManager (they're in different scenes)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterUIController(this);
            Debug.Log("✅ UIController_Game registered with GameManager");
        }
        else
        {
            Debug.LogError("❌ UIController_Game: GameManager.Instance is null!");
        }
    }

    // ============================================
    // PUBLIC METHODS - Called by other systems
    // ============================================

    /// <summary>
    /// Closes all menu panels and untoggle all menu buttons
    /// Called after starting an action to return to main game view
    /// </summary>
    public void CloseAllMenus()
    {
        // Why: Hide all panels
        HideAllPanels();

        // Why: Untoggle all menu buttons
        if (menuToggleGroup != null)
        {
            // CRITICAL: ToggleGroup needs allowSwitchOff = true to untoggle all buttons
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

    /// <summary>
    /// Updates all UI displays from GameManager data
    /// Called by GameManager whenever game state changes
    /// </summary>
    public void RefreshUI()
    {
        // Why: Update from GameManager
        if (GameManager.Instance == null)
        {
            Debug.LogError("❌ UIController_Game.RefreshUI(): GameManager.Instance is null!");
            return;
        }

        GameManager gm = GameManager.Instance;

        // Why: Update resource displays
        if (moneyText != null) moneyText.text = "$" + gm.money.ToString();
        if (fansText != null) fansText.text = gm.fans.ToString();
        if (unityText != null) unityText.text = gm.unity.ToString() + "%";

        // Why: Update quarter/year display
        if (quarterYearText != null)
        {
            int year = (gm.currentQuarter / 4) + 1; // Years 1-20
            int quarter = (gm.currentQuarter % 4) + 1; // Quarters 1-4
            quarterYearText.text = $"Q{quarter} YEAR {year}";
        }

        // Why: Update band name
        if (bandNameText != null) bandNameText.text = gm.bandName;

        // Why: Update character displays
        RefreshCharacters();
    }

    /// <summary>
    /// Wrapper method called by GameManager (for compatibility)
    /// </summary>
    public void RefreshDisplay()
    {
        RefreshUI();
    }

    // ============================================
    // PRIVATE HELPER METHODS
    // ============================================

    private void HideAllPanels()
    {
        // Why: Hide all action menu panels
        if (practicePanel != null) practicePanel.SetActive(false);
        if (producePanel != null) producePanel.SetActive(false);
        if (tourPanel != null) tourPanel.SetActive(false);
        if (releasePanel != null) releasePanel.SetActive(false);
        if (financePanel != null) financePanel.SetActive(false);
        if (marketingPanel != null) marketingPanel.SetActive(false);
        if (managingPanel != null) managingPanel.SetActive(false);
    }

    private void RefreshCharacters()
    {
        // Why: Update character portraits and stats from GameManager
        if (GameManager.Instance == null)
        {
            Debug.LogError("❌ UIController_Game.RefreshCharacters(): GameManager.Instance is null!");
            return;
        }

        GameManager gm = GameManager.Instance;

        // Loop through all character displays
        for (int i = 0; i < characterDisplays.Length; i++)
        {
            // Safety check
            if (characterDisplays[i] == null)
            {
                Debug.LogWarning($"characterDisplays[{i}] is null - not assigned in Inspector!");
                continue;
            }

            // Check if character exists in this slot
            if (i < gm.characterStates.Length && gm.characterStates[i] != null && gm.characterStates[i].slotData != null)
            {
                // Why: Character exists in this slot - show it
                characterDisplays[i].SetCharacter(gm.characterStates[i].slotData);
            }
            else
            {
                // Why: No character in this slot - hide it
                characterDisplays[i].Clear();
            }
        }
    }

    // ============================================
    // MENU TOGGLE HANDLERS (Called directly by Toggle.onValueChanged in Inspector)
    // ============================================
    // WHY: Each toggle in Area B calls these methods directly via Inspector
    // No need to store toggle references - Unity handles the connection

    public void OnPracticeMenuToggled(bool isOn)
    {
        // Why: Show/hide Practice panel based on toggle state
        ShowHidePanel(practicePanel, isOn);
    }

    public void OnProduceMenuToggled(bool isOn)
    {
        // Why: Show/hide Produce panel based on toggle state
        ShowHidePanel(producePanel, isOn);
    }

    public void OnTourMenuToggled(bool isOn)
    {
        // Why: Show/hide Tour panel based on toggle state
        ShowHidePanel(tourPanel, isOn);
    }

    public void OnReleaseMenuToggled(bool isOn)
    {
        // Why: Show/hide Release panel based on toggle state
        ShowHidePanel(releasePanel, isOn);
    }

    public void OnFinanceMenuToggled(bool isOn)
    {
        // Why: Show/hide Finance panel based on toggle state
        ShowHidePanel(financePanel, isOn);
    }

    public void OnMarketingMenuToggled(bool isOn)
    {
        // Why: Show/hide Marketing panel based on toggle state
        ShowHidePanel(marketingPanel, isOn);
    }

    public void OnManagingMenuToggled(bool isOn)
    {
        // Why: Show/hide Managing panel based on toggle state
        ShowHidePanel(managingPanel, isOn);
    }

    private void ShowHidePanel(GameObject panel, bool shouldShow)
    {
        // Why: Toggle panel visibility
        if (panel != null)
        {
            panel.SetActive(shouldShow);
        }
    }
}