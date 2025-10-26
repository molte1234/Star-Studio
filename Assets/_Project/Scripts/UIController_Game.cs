using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls all UI elements on the main game screen
/// Updates displays, handles button clicks
/// Follows the UIController naming pattern (like UIController_MainMenu)
/// </summary>
public class UIController_Game : MonoBehaviour
{
    [Header("Band Info Display")]
    public TextMeshProUGUI bandNameText;

    [Header("Stats Display")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI fansText;
    public TextMeshProUGUI yearText;
    public TextMeshProUGUI quarterText;

    [Header("Stat Bars")]
    public StatBar technicalBar;      // 0-30 scale
    public StatBar performanceBar;    // 0-30 scale
    public StatBar charismaBar;       // 0-30 scale
    public StatBar unityBar;          // 0-100 scale

    [Header("Band Display")]
    public CharacterDisplay[] characterDisplays; // Array of character displays (min 6, expandable)

    [Header("Pause Toggle")]
    [Tooltip("Optional: Assign the Toggle component to auto-sync its visual state with pause state")]
    public Toggle pauseToggle;

    void OnEnable()
    {
        // Why: Scene was just activated by SceneLoader, refresh the display
        RefreshDisplay();
        SyncPauseToggle();
    }

    void Start()
    {
        // Initial display update
        RefreshDisplay();
        SyncPauseToggle();
    }

    /// <summary>
    /// Refreshes all displays - call this when scene activates or data changes
    /// </summary>
    public void RefreshDisplay()
    {
        UpdateBandDisplay();
        UpdateStatsDisplay();
    }

    /// <summary>
    /// Refreshes all stat displays with current GameManager values
    /// Call this after any action or quarter change
    /// </summary>
    public void UpdateStatsDisplay()
    {
        GameManager gm = GameManager.Instance;

        // Update band name
        if (bandNameText != null)
        {
            bandNameText.text = gm.bandName;
        }

        // Update text displays
        moneyText.text = "$" + gm.money.ToString("N0"); // N0 adds comma separators
        fansText.text = gm.fans.ToString("N0");
        yearText.text = "YEAR " + gm.currentYear;

        // Calculate which quarter (1-4) from currentQuarter
        int displayQuarter = (gm.currentQuarter % 4) + 1;
        quarterText.text = "QUARTER " + displayQuarter;

        // Update stat bars - FIXED: Use 30 as max for band stats (they cap at 30 in GameManager)
        technicalBar.SetValue(gm.technical, 30);      // current value, max value
        performanceBar.SetValue(gm.performance, 30);
        charismaBar.SetValue(gm.charisma, 30);
        unityBar.SetValue(gm.unity, 100);
    }

    /// <summary>
    /// Shows/hides band member portraits from GameManager.slots
    /// Automatically shows characters that exist, hides empty slots
    /// </summary>
    public void UpdateBandDisplay()
    {
        // Safety check
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager.Instance is null - can't update band display");
            return;
        }

        if (characterDisplays == null || characterDisplays.Length == 0)
        {
            Debug.LogWarning("characterDisplays array is empty! Assign CharacterDisplay components in Inspector");
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
    // ACTION BUTTON HANDLERS (Called from Inspector)
    // ============================================

    public void OnRecordClicked()
    {
        // Why: Player clicked RECORD, tell GameManager to process it
        GameManager.Instance.DoAction(ActionType.Record);
    }

    public void OnTourClicked()
    {
        GameManager.Instance.DoAction(ActionType.Tour);
    }

    public void OnPracticeClicked()
    {
        GameManager.Instance.DoAction(ActionType.Practice);
    }

    public void OnRestClicked()
    {
        GameManager.Instance.DoAction(ActionType.Rest);
    }

    public void OnReleaseClicked()
    {
        GameManager.Instance.DoAction(ActionType.Release);
    }

    public void OnNextQuarterClicked()
    {
        // Why: Advance to next quarter
        GameManager.Instance.AdvanceQuarter();
    }

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

    /// <summary>
    /// Syncs the pause toggle's visual state with TimeManager's isPaused state
    /// Call this when scene loads or when TimeManager.isPaused changes externally
    /// </summary>
    public void SyncPauseToggle()
    {
        if (pauseToggle == null || TimeManager.Instance == null) return;

        // Why: Update toggle's isOn state to match TimeManager without triggering onValueChanged
        pauseToggle.SetIsOnWithoutNotify(TimeManager.Instance.isPaused);
    }
}