using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Controls the event popup panel
/// Populates title, image, description and creates choice buttons dynamically
/// Attach this to the Event Popup Panel GameObject
/// </summary>
public class EventPanel : MonoBehaviour
{
    [Header("Main Panel")]
    [Tooltip("The TOP-LEVEL event panel GameObject that darkens the whole screen (NOT the card panel)")]
    public GameObject panelRoot;

    [Header("Content References")]
    [Tooltip("Title text at top of event")]
    public TextMeshProUGUI titleText;

    [Tooltip("Event illustration/image")]
    public Image eventImage;

    [Tooltip("Main event description text")]
    public TextMeshProUGUI descriptionText;

    [Header("Choice Buttons")]
    [Tooltip("Container where choice buttons will be spawned (horizontal layout group recommended)")]
    public Transform buttonContainer;

    [Tooltip("Prefab for choice buttons - should have a Button component and TextMeshProUGUI child")]
    public GameObject choiceButtonPrefab;

    [Header("Animation Settings")]
    public float popupDuration = 0.5f;
    public Ease popupEase = Ease.OutBack;

    // Why: Store created buttons so we can clean them up
    private GameObject[] spawnedButtons;
    private EventData currentEventData;

    void Awake()
    {
        // Why: Start hidden
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    /// <summary>
    /// Displays the event with all its data
    /// Called by EventManager when an event triggers
    /// </summary>
    public void ShowEvent(EventData eventData)
    {
        if (eventData == null)
        {
            Debug.LogError("EventPanel.ShowEvent: eventData is null!");
            return;
        }

        currentEventData = eventData;

        // Populate content
        PopulateContent(eventData);

        // Create choice buttons
        CreateChoiceButtons(eventData.choices);

        // Show panel with animation
        ShowPanelWithAnimation();
    }

    private void PopulateContent(EventData eventData)
    {
        // Why: Fill in title, image, description from EventData

        if (titleText != null)
        {
            titleText.text = eventData.eventTitle;
        }

        if (eventImage != null && eventData.eventSprite != null)
        {
            eventImage.sprite = eventData.eventSprite;
            eventImage.enabled = true;
        }
        else if (eventImage != null)
        {
            eventImage.enabled = false; // Hide if no sprite
        }

        if (descriptionText != null)
        {
            descriptionText.text = eventData.eventDescription;
        }
    }

    private void CreateChoiceButtons(ChoiceData[] choices)
    {
        // Why: Clean up any old buttons first
        ClearButtons();

        // Why: If no choices provided, create a simple "OK" button to close the event
        if (choices == null || choices.Length == 0)
        {
            Debug.Log("EventPanel: No choices for this event - creating default OK button");
            CreateOKButton();
            return;
        }

        // Create array to store spawned buttons
        spawnedButtons = new GameObject[choices.Length];

        // Spawn a button for each choice
        for (int i = 0; i < choices.Length; i++)
        {
            GameObject buttonObj = Instantiate(choiceButtonPrefab, buttonContainer);
            spawnedButtons[i] = buttonObj;

            // Set button text - searches children for TextMeshProUGUI
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = choices[i].choiceText;
            }
            else
            {
                Debug.LogWarning($"Choice button {i} has no TextMeshProUGUI in children!");
            }

            // Hook up button click - searches children for Button component
            Button button = buttonObj.GetComponentInChildren<Button>();
            if (button != null)
            {
                int choiceIndex = i; // Capture index for closure
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnChoiceClicked(choiceIndex));
            }
            else
            {
                Debug.LogError($"Choice button {i} has no Button component in children! Check prefab structure.");
            }
        }
    }

    private void CreateOKButton()
    {
        // Why: Create a single "OK" button that just closes the event
        spawnedButtons = new GameObject[1];

        GameObject buttonObj = Instantiate(choiceButtonPrefab, buttonContainer);
        spawnedButtons[0] = buttonObj;

        // Set button text to "OK"
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = "OK";
        }

        // Hook up button to just close the panel
        Button button = buttonObj.GetComponentInChildren<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnOKClicked());
        }
    }

    private void OnOKClicked()
    {
        // Why: Player clicked OK on an event with no choices - just close the panel
        Debug.Log("EventPanel: OK button clicked - closing event");

        // Play button click sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        // ✅ FIX: Resume regular music (fade out event music)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ResumeMusic();
        }

        // Hide the panel
        HideEvent();

        // ✅ FIX: Refresh UI after closing
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ForceRefreshUI();
        }
    }

    private void ClearButtons()
    {
        // Why: Remove all previously spawned buttons
        if (spawnedButtons != null)
        {
            foreach (GameObject btn in spawnedButtons)
            {
                if (btn != null)
                {
                    Destroy(btn);
                }
            }
            spawnedButtons = null;
        }
    }

    private void ShowPanelWithAnimation()
    {
        // Why: Animate panel appearing
        if (panelRoot == null) return;

        panelRoot.SetActive(true);

        // Scale from 0 to 1 with bounce
        panelRoot.transform.localScale = Vector3.zero;
        panelRoot.transform.DOScale(Vector3.one, popupDuration).SetEase(popupEase);
    }

    /// <summary>
    /// Hides the event panel
    /// Called after player makes a choice
    /// </summary>
    public void HideEvent()
    {
        if (panelRoot == null) return;

        // Animate out
        panelRoot.transform.DOScale(Vector3.zero, popupDuration * 0.5f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                panelRoot.SetActive(false);
                ClearButtons();
            });
    }

    private void OnChoiceClicked(int choiceIndex)
    {
        // Why: Player clicked a choice button
        // Tell EventManager to process this choice
        EventManager eventMgr = FindObjectOfType<EventManager>();
        if (eventMgr != null)
        {
            eventMgr.PlayerChoseOption(choiceIndex);
        }
        else
        {
            Debug.LogError("EventPanel: Could not find EventManager!");
        }

        // Play button click sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }

    /// <summary>
    /// Optional: Call this to show event outcome text after choice
    /// For now, we just close the panel immediately
    /// </summary>
    public void ShowOutcome(string outcomeText)
    {
        // TODO: Show a brief outcome message before closing
        // For prototype, we skip this
        Debug.Log("Event Outcome: " + outcomeText);
    }
}