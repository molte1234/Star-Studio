using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages a single character portrait display
/// Shows character portrait, action status, and time remaining
/// UPDATED: Now handles busy states with action text and progress bar
/// </summary>
public class CharacterDisplay : MonoBehaviour
{
    [Header("Portrait")]
    public Image portraitImage; // The UI Image that shows the character sprite

    [Header("Action Status (Shown when busy)")]
    [Tooltip("Text that shows action name like 'PRACTICING'")]
    public TextMeshProUGUI actionText;

    [Tooltip("HorizontalBar that shows time remaining (smooth mode)")]
    public HorizontalBar timeBar;

    [Header("Visual Settings")]
    [Tooltip("Color to tint portrait when character is busy")]
    public Color busyTintColor = new Color(0.7f, 0.7f, 0.7f, 1f);

    // ============================================
    // PRIVATE STATE
    // ============================================
    private Color originalPortraitColor = Color.white;
    private int currentCharacterIndex = -1; // Track which character this is displaying

    void Awake()
    {
        // Why: Store original color
        if (portraitImage != null)
        {
            originalPortraitColor = portraitImage.color;
        }

        // Why: Start with action UI hidden
        HideActionUI();
    }

    // ============================================
    // CHARACTER SETUP
    // ============================================

    /// <summary>
    /// Updates this display with a character from SlotData
    /// Automatically shows if character exists, hides if null
    /// </summary>
    public void SetCharacter(SlotData slotData)
    {
        if (slotData == null)
        {
            // Why: No character in this slot, hide it
            Clear();
            return;
        }

        // Why: We have a character, show it
        if (portraitImage != null)
        {
            portraitImage.sprite = slotData.sprite;
            portraitImage.enabled = true;
            portraitImage.color = originalPortraitColor; // Reset to normal color
        }
        else
        {
            Debug.LogWarning("portraitImage is not assigned in CharacterDisplay Inspector!");
        }

        // Why: Hide action UI by default (will be shown when action starts)
        HideActionUI();
    }

    /// <summary>
    /// Clears this display (hides it)
    /// </summary>
    public void Clear()
    {
        if (portraitImage != null)
        {
            portraitImage.enabled = false;
        }

        HideActionUI();
        currentCharacterIndex = -1;
    }

    // ============================================
    // BUSY STATE (Called by ActionManager/UIController)
    // ============================================

    /// <summary>
    /// Shows/hides busy state with action name and progress bar
    /// </summary>
    /// <param name="isBusy">Is character currently doing an action?</param>
    /// <param name="actionName">Name of action (e.g., "PRACTICING")</param>
    /// <param name="progress">Progress from 0.0 (just started) to 1.0 (complete)</param>
    public void SetBusyState(bool isBusy, string actionName = "", float progress = 0f)
    {
        if (isBusy)
        {
            // ============================================
            // BUSY STATE: Show action UI
            // ============================================

            // Show action name
            if (actionText != null)
            {
                actionText.text = actionName.ToUpper();
                actionText.gameObject.SetActive(true);
            }

            // Show and update progress bar
            if (timeBar != null)
            {
                timeBar.gameObject.SetActive(true);

                // Why: Use smooth mode for time countdown (1.0 = full, 0.0 = empty)
                // Progress goes from 0 → 1 as action completes
                // Bar should go from 1 → 0 as time runs out
                float barFillAmount = 1f - progress; // Invert so bar empties as time passes
                timeBar.SetFillPercent(barFillAmount);
            }

            // Why: Optionally darken portrait to show they're busy
            if (portraitImage != null)
            {
                portraitImage.color = busyTintColor;
            }
        }
        else
        {
            // ============================================
            // IDLE STATE: Hide action UI
            // ============================================
            HideActionUI();

            // Why: Return portrait to normal color
            if (portraitImage != null)
            {
                portraitImage.color = originalPortraitColor;
            }
        }
    }

    // ============================================
    // HELPER METHODS
    // ============================================

    private void HideActionUI()
    {
        // Why: Hide action text
        if (actionText != null)
        {
            actionText.gameObject.SetActive(false);
        }

        // Why: Hide progress bar
        if (timeBar != null)
        {
            timeBar.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Assigns which character index this display represents
    /// Used for tracking which slot in GameManager.characterStates[] this represents
    /// </summary>
    public void SetCharacterIndex(int index)
    {
        currentCharacterIndex = index;
    }

    public int GetCharacterIndex()
    {
        return currentCharacterIndex;
    }
}