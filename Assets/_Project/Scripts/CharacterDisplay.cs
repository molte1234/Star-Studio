using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages a single character portrait display
/// Shows character portrait, action status, and time remaining
/// Does NOT control its own GameObject active state - UIController handles that
/// UPDATED: Uses unified HorizontalBar.SetProgress(0-1) API
/// </summary>
public class CharacterDisplay : MonoBehaviour
{
    [Header("Portrait")]
    public Image portraitImage;

    [Header("Action Status (Shown when busy)")]
    [Tooltip("Text that shows action name like 'PRACTICING'")]
    public TextMeshProUGUI actionText;

    [Tooltip("HorizontalBar that shows time remaining")]
    public HorizontalBar timeBar;

    [Header("Visual Settings")]
    [Tooltip("Color to tint portrait when character is busy")]
    public Color busyTintColor = new Color(0.7f, 0.7f, 0.7f, 1f);

    // ============================================
    // PRIVATE STATE
    // ============================================
    private Color originalPortraitColor = Color.white;
    private int currentCharacterIndex = -1;

    void Awake()
    {
        if (portraitImage != null)
        {
            originalPortraitColor = portraitImage.color;
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
    // BUSY STATE
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

                // Progress = 0.0 (started) → 1.0 (complete)
                // Bar shows TIME REMAINING, so invert: 1.0 (full) → 0.0 (empty)
                float timeRemaining = 1f - progress;

                timeBar.SetProgress(timeRemaining);
            }

            // Darken portrait
            if (portraitImage != null)
            {
                portraitImage.color = busyTintColor;
            }
        }
        else
        {
            // IDLE STATE
            HideActionUI();

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
        if (actionText != null)
        {
            actionText.gameObject.SetActive(false);
        }

        if (timeBar != null)
        {
            timeBar.gameObject.SetActive(false);
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