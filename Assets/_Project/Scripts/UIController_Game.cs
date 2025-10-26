using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls all UI elements on the main game screen
/// Updates displays, handles button clicks, shows/hides event popups
/// Follows the UIController naming pattern (like UIController_MainMenu)
/// </summary>
public class UIController_Game : MonoBehaviour
{
    [Header("Stats Display")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI fansText;
    public TextMeshProUGUI yearText;
    public TextMeshProUGUI quarterText;

    [Header("Stat Bars")]
    public StatBar technicalBar;      // 0-10 scale
    public StatBar performanceBar;    // 0-10 scale
    public StatBar charismaBar;       // 0-10 scale
    public StatBar unityBar;          // 0-100 scale

    [Header("Band Display")]
    public CharacterDisplay[] characterDisplays; // Array of character displays (min 6, expandable)

    [Header("Event Popup - For Later")]
    public GameObject eventPopupPanel;
    public TextMeshProUGUI eventTitleText;
    public TextMeshProUGUI eventDescriptionText;
    public Button[] eventChoiceButtons;

    void OnEnable()
    {
        // Why: Scene was just activated by SceneLoader, refresh the display
        RefreshDisplay();
    }

    void Start()
    {
        // Hide event popup at start
        if (eventPopupPanel != null)
        {
            eventPopupPanel.SetActive(false);
        }

        // Initial display update
        RefreshDisplay();
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

        // Update text displays
        moneyText.text = "$" + gm.money.ToString("N0"); // N0 adds comma separators
        fansText.text = gm.fans.ToString("N0");
        yearText.text = "YEAR " + gm.currentYear;

        // Calculate which quarter (1-4) from currentQuarter
        int displayQuarter = (gm.currentQuarter % 4) + 1;
        quarterText.text = "QUARTER " + displayQuarter;

        // Update stat bars
        technicalBar.SetValue(gm.technical, 10);      // current value, max value
        performanceBar.SetValue(gm.performance, 10);
        charismaBar.SetValue(gm.charisma, 10);
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

    public void OnPauseClicked()
    {
        // Why: Player wants to pause the game
        // TODO: Implement pause menu/functionality
        Debug.Log("PAUSE clicked - implement pause menu here");
    }

    // ============================================
    // EVENT POPUP METHODS (FOR LATER)
    // ============================================

    /// <summary>
    /// Shows the event popup with given event data
    /// </summary>
    public void ShowEvent(EventData eventData)
    {
        if (eventPopupPanel != null)
        {
            eventPopupPanel.SetActive(true);

            // TODO: Populate event UI with eventData
            // eventTitleText.text = eventData.eventTitle;
            // eventDescriptionText.text = eventData.description;
        }
    }

    /// <summary>
    /// Hides the event popup
    /// </summary>
    public void HideEvent()
    {
        if (eventPopupPanel != null)
        {
            eventPopupPanel.SetActive(false);
        }
    }
}