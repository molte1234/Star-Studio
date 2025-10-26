using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls all UI in the main Game scene
/// Handles displaying stats, character portraits, and menu panels
/// </summary>
public class UIController_Game : MonoBehaviour
{
    [Header("Stats Display")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI fansText;
    public TextMeshProUGUI technicalText;
    public TextMeshProUGUI performanceText;
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

    void Start()
    {
        // Why: Make sure all panels start hidden
        HideAllPanels();

        RefreshUI();
    }

    private void HideAllPanels()
    {
        // Why: Start with all panels hidden
        if (practicePanel != null) practicePanel.SetActive(false);
        if (producePanel != null) producePanel.SetActive(false);
        if (tourPanel != null) tourPanel.SetActive(false);
        if (releasePanel != null) releasePanel.SetActive(false);
        if (financePanel != null) financePanel.SetActive(false);
        if (marketingPanel != null) marketingPanel.SetActive(false);
        if (managingPanel != null) managingPanel.SetActive(false);
    }

    // ============================================
    // UI REFRESH (Updates all displays)
    // ============================================

    public void RefreshUI()
    {
        // Why: Update all stat displays from GameManager
        RefreshStats();
        RefreshCharacters();
    }

    public void RefreshDisplay()
    {
        // Why: Called by GameManager after actions, same as RefreshUI
        RefreshUI();
    }

    private void RefreshStats()
    {
        // Why: Show current game state numbers
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("⚠️ GameManager.Instance is null - cannot refresh stats!");
            return;
        }

        GameManager gm = GameManager.Instance;

        if (moneyText != null)
        {
            moneyText.text = $"${gm.money}";
        }

        if (fansText != null)
        {
            fansText.text = $"{gm.fans}";
        }

        if (technicalText != null)
        {
            technicalText.text = $"{gm.technical}";
        }

        if (performanceText != null)
        {
            performanceText.text = $"{gm.performance}";
        }

        if (unityText != null)
        {
            unityText.text = $"{gm.unity}";
        }

        if (quarterYearText != null)
        {
            quarterYearText.text = $"YEAR {gm.currentYear}  QUARTER {gm.currentQuarter + 1}";
        }

        if (bandNameText != null)
        {
            bandNameText.text = gm.bandName;
        }
    }

    private void RefreshCharacters()
    {
        // Why: Update character portraits in slots
        if (characterDisplays == null || characterDisplays.Length == 0)
        {
            Debug.LogWarning("⚠️ No CharacterDisplay components assigned! Assign CharacterDisplay components in Inspector");
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

            // Check if this slot index exists in GameManager
            if (i < gm.slots.Length && gm.slots[i] != null)
            {
                // Why: Character exists in this slot - show it
                characterDisplays[i].SetCharacter(gm.slots[i]);
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

    private void ShowHidePanel(GameObject panel, bool show)
    {
        // Why: If panel is null, can't show/hide it (not assigned yet)
        if (panel == null)
        {
            return; // Silently ignore - panel not implemented yet
        }

        // Why: Show or hide the panel based on toggle state
        // ToggleGroup ensures only one toggle is ON at a time, so only one panel shows
        panel.SetActive(show);
    }

    // ============================================
    // ACTION BUTTON HANDLERS - TODO: Design action system later
    // ============================================
    // NOTE: Action buttons will be added inside panels later
    // For now, menus just open/close - no actions are executed

    public void OnMainMenuClicked()
    {
        // Why: Player wants to return to main menu
        SceneLoader.Instance.LoadMainMenu();
    }

    // ============================================
    // PAUSE TOGGLE HANDLER (Called by Toggle.onValueChanged)
    // ============================================

    /// <summary>
    /// Called by Toggle component's onValueChanged event
    /// Pauses or resumes time based on toggle state
    /// </summary>
    public void OnPauseToggled(bool isPaused)
    {
        // Why: Player toggled pause, update TimeManager
        if (TimeManager.Instance != null)
        {
            if (isPaused)
            {
                TimeManager.Instance.PauseTime();
            }
            else
            {
                TimeManager.Instance.ResumeTime();
            }
        }
        else
        {
            Debug.LogWarning("⚠️ TimeManager.Instance is null - cannot pause!");
        }
    }
}