using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages a single character portrait display
/// Shows character portrait, action status, and time remaining
/// Does NOT control its own GameObject active state - UIController handles that
/// FIXED: Uses CanvasGroup on parent so cancel button + UIButton stay active for proper initialization
/// ARCHITECTURE: SetBusyState called ONCE on state change, UpdateProgress called every frame
/// </summary>
public class CharacterDisplay : MonoBehaviour
{
    [Header("Portrait")]
    public Image portraitImage;

    [Header("Action Status (Shown when busy)")]
    [Tooltip("Text that shows action name like 'PRACTICING'")]
    public TextMeshProUGUI actionText;

    [Tooltip("HorizontalBar that shows action progress (fills upwards 0→1)")]
    public HorizontalBar timeBar;

    [Tooltip("Parent GameObject containing cancel button + shadow (CanvasGroup will be auto-added)")]
    public GameObject cancelButtonRoot;

    [Header("Visual Settings")]
    [Tooltip("Color to tint portrait when character is busy")]
    public Color busyTintColor = new Color(0.7f, 0.7f, 0.7f, 1f);

    // ============================================
    // PRIVATE STATE
    // ============================================
    private Color originalPortraitColor = Color.white;
    private int currentCharacterIndex = -1;
    private Button cancelButtonComponent; // Cached button component
    private CanvasGroup cancelButtonCanvasGroup; // For hiding without SetActive

    void Awake()
    {
        if (portraitImage != null)
        {
            originalPortraitColor = portraitImage.color;
        }

        // ✅ Setup cancel button with CanvasGroup for visibility control
        if (cancelButtonRoot != null)
        {
            // Get or add CanvasGroup to parent (keeps everything active for UIButton to work)
            cancelButtonCanvasGroup = cancelButtonRoot.GetComponent<CanvasGroup>();
            if (cancelButtonCanvasGroup == null)
            {
                cancelButtonCanvasGroup = cancelButtonRoot.AddComponent<CanvasGroup>();
                Debug.Log("✅ Added CanvasGroup to CancelButtonRoot for proper visibility control");
            }

            // Find Button component in children
            cancelButtonComponent = cancelButtonRoot.GetComponentInChildren<Button>();

            if (cancelButtonComponent == null)
            {
                Debug.LogError("❌ CharacterDisplay: No Button component found in cancelButtonRoot children!");
            }
            else
            {
                // Wire up onClick
                cancelButtonComponent.onClick.AddListener(OnCancelButtonClicked);

                // Check if UIButton is attached
                UIButton uiButton = cancelButtonComponent.GetComponent<UIButton>();
                if (uiButton == null)
                {
                    Debug.LogWarning($"⚠️ Cancel button is missing UIButton component! Add it for hover/press animations.");
                }
            }
        }

        HideActionUI();
    }

    // ============================================
    // CHARACTER SETUP
    // ============================================

    public void SetCharacter(SlotData slotData)
    {
        if (slotData == null)
        {
            Debug.LogWarning("SetCharacter called with null slotData!");
            return;
        }

        if (portraitImage != null)
        {
            portraitImage.sprite = slotData.sprite;
            portraitImage.color = originalPortraitColor;
        }
        else
        {
            Debug.LogWarning("portraitImage is not assigned in CharacterDisplay Inspector!");
        }

        HideActionUI();
    }

    // ============================================
    // BUSY STATE - CALLED ONCE ON STATE CHANGE
    // ============================================

    /// <summary>
    /// Show/hide busy UI elements when action starts/ends
    /// Called ONCE from ActionManager when state changes
    /// </summary>
    /// <param name="isBusy">Is character now busy?</param>
    /// <param name="actionName">Name of action (e.g., "PRACTICING")</param>
    public void SetBusyState(bool isBusy, string actionName = "")
    {
        if (isBusy)
        {
            // ============================================
            // BUSY STATE: Show action UI ONCE
            // ============================================

            // Show action name
            if (actionText != null)
            {
                actionText.text = actionName.ToUpper();
                actionText.gameObject.SetActive(true);
            }

            // Show progress bar (don't set value yet - UpdateProgress will do that)
            if (timeBar != null)
            {
                timeBar.gameObject.SetActive(true);
            }

            // Show cancel button using CanvasGroup
            if (cancelButtonCanvasGroup != null)
            {
                cancelButtonCanvasGroup.alpha = 1f;
                cancelButtonCanvasGroup.interactable = true;
                cancelButtonCanvasGroup.blocksRaycasts = true;
            }

            // Darken portrait
            if (portraitImage != null)
            {
                portraitImage.color = busyTintColor;
            }

            Debug.Log($"✅ SetBusyState(true, '{actionName}') - UI shown for character {currentCharacterIndex}");
        }
        else
        {
            // ============================================
            // IDLE STATE: Hide action UI ONCE
            // ============================================
            HideActionUI();

            if (portraitImage != null)
            {
                portraitImage.color = originalPortraitColor;
            }

            Debug.Log($"✅ SetBusyState(false) - UI hidden for character {currentCharacterIndex}");
        }
    }

    // ============================================
    // UPDATE PROGRESS - CALLED EVERY FRAME
    // ============================================

    /// <summary>
    /// Update the progress bar fill amount
    /// Called every frame from UIController.RefreshCharacters()
    /// </summary>
    /// <param name="progress">Progress from 0.0 (just started) to 1.0 (complete)</param>
    public void UpdateProgress(float progress)
    {
        if (timeBar != null && timeBar.gameObject.activeSelf)
        {
            timeBar.SetProgress(progress);
        }
    }

    // ============================================
    // CANCEL BUTTON HANDLER
    // ============================================

    private void OnCancelButtonClicked()
    {
        if (ActionManager.Instance == null)
        {
            Debug.LogError("❌ CharacterDisplay: ActionManager.Instance is null!");
            return;
        }

        if (currentCharacterIndex < 0)
        {
            Debug.LogWarning("⚠️ CharacterDisplay: Cannot cancel - character index not set!");
            return;
        }

        Debug.Log($"🚫 Cancel button clicked for character slot {currentCharacterIndex}");

        // Call ActionManager to cancel this character's action
        ActionManager.Instance.CancelAction(currentCharacterIndex);

        // Note: AudioManager call removed - UIButton already handles click sound
    }

    // ============================================
    // HELPER METHODS
    // ============================================

    private void HideActionUI()
    {
        if (actionText != null)
        {
            actionText.gameObject.SetActive(false);
        }

        if (timeBar != null)
        {
            timeBar.gameObject.SetActive(false);
        }

        // ✅ Hide cancel button using CanvasGroup (keeps button+shadow active, UIButton keeps working!)
        if (cancelButtonCanvasGroup != null)
        {
            cancelButtonCanvasGroup.alpha = 0f;
            cancelButtonCanvasGroup.interactable = false;
            cancelButtonCanvasGroup.blocksRaycasts = false;
        }
    }

    public void SetCharacterIndex(int index)
    {
        currentCharacterIndex = index;
    }

    public int GetCharacterIndex()
    {
        return currentCharacterIndex;
    }
}