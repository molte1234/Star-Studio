using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
    public Image[] bandMemberPortraits; // Array of 3 portrait images

    [Header("Action Buttons")]
    public Button recordButton;
    public Button tourButton;
    public Button practiceButton;
    public Button restButton;
    public Button releaseButton;
    public Button nextQuarterButton;
    public Button mainMenuButton;
    public Button pauseButton;

    [Header("Event Popup - For Later")]
    public GameObject eventPopupPanel;
    public TextMeshProUGUI eventTitleText;
    public TextMeshProUGUI eventDescriptionText;
    public Button[] eventChoiceButtons;

    void Start()
    {
        // Why: Wire up all button clicks to their respective handlers
        recordButton.onClick.AddListener(OnRecordClicked);
        tourButton.onClick.AddListener(OnTourClicked);
        practiceButton.onClick.AddListener(OnPracticeClicked);
        restButton.onClick.AddListener(OnRestClicked);
        releaseButton.onClick.AddListener(OnReleaseClicked);
        nextQuarterButton.onClick.AddListener(OnNextQuarterClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        pauseButton.onClick.AddListener(OnPauseClicked);

        // Hide event popup at start
        if (eventPopupPanel != null)
        {
            eventPopupPanel.SetActive(false);
        }

        // Initial display update
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
    /// Shows the 3 selected band member portraits from GameManager.slots
    /// </summary>
    public void UpdateBandDisplay()
    {
        GameManager gm = GameManager.Instance;

        for (int i = 0; i < 3; i++)
        {
            if (i < bandMemberPortraits.Length)
            {
                if (gm.slots[i] != null)
                {
                    // Show portrait from SlotData
                    bandMemberPortraits[i].sprite = gm.slots[i].sprite;
                    bandMemberPortraits[i].gameObject.SetActive(true);
                }
                else
                {
                    // Hide if slot is empty
                    bandMemberPortraits[i].gameObject.SetActive(false);
                }
            }
        }
    }

    // ============================================
    // ACTION BUTTON HANDLERS
    // ============================================

    private void OnRecordClicked()
    {
        // Why: Player clicked RECORD, tell GameManager to process it
        GameManager.Instance.DoAction(ActionType.Record);
    }

    private void OnTourClicked()
    {
        GameManager.Instance.DoAction(ActionType.Tour);
    }

    private void OnPracticeClicked()
    {
        GameManager.Instance.DoAction(ActionType.Practice);
    }

    private void OnRestClicked()
    {
        GameManager.Instance.DoAction(ActionType.Rest);
    }

    private void OnReleaseClicked()
    {
        GameManager.Instance.DoAction(ActionType.Release);
    }

    private void OnNextQuarterClicked()
    {
        // Why: Advance to next quarter
        GameManager.Instance.AdvanceQuarter();
    }

    private void OnMainMenuClicked()
    {
        // Why: Player wants to return to main menu
        SceneManager.LoadScene("MainMenu");
    }

    private void OnPauseClicked()
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