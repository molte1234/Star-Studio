using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Manages a single character portrait display
/// Shows character portrait, action status, time remaining, and handles mouse interactions
/// NOTE: This script is DEPRECATED and will be replaced by CharacterObject.cs in the room-based system
/// </summary>
public class CharacterDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
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

    [Header("Mouse Interaction")]
    [Tooltip("Scale multiplier when hovering over portrait")]
    public float hoverScaleMultiplier = 1.05f;

    [Tooltip("Color multiplier when mouse hovers over portrait")]
    public Color hoverTintColor = new Color(1.2f, 1.2f, 1.2f, 1f);

    [Tooltip("Color tint when character is selected (clicked)")]
    public Color selectedTintColor = new Color(0.5f, 1f, 1f, 1f); // Cyan-ish

    // ============================================
    // PRIVATE STATE
    // ============================================
    private Color originalPortraitColor = Color.white;
    private Vector3 originalScale = Vector3.one;
    private int currentCharacterIndex = -1;
    private Button cancelButtonComponent;
    private CanvasGroup cancelButtonCanvasGroup;

    // Visual state tracking
    private bool isHovered = false;
    private bool isSelected = false;
    private bool isBusy = false;

    void Awake()
    {
        // Store original values
        if (portraitImage != null)
        {
            originalPortraitColor = portraitImage.color;
        }

        originalScale = transform.localScale;

        // Setup cancel button with CanvasGroup for visibility control
        if (cancelButtonRoot != null)
        {
            cancelButtonCanvasGroup = cancelButtonRoot.GetComponent<CanvasGroup>();
            if (cancelButtonCanvasGroup == null)
            {
                cancelButtonCanvasGroup = cancelButtonRoot.AddComponent<CanvasGroup>();
                Debug.Log("✅ Added CanvasGroup to CancelButtonRoot for proper visibility control");
            }

            cancelButtonComponent = cancelButtonRoot.GetComponentInChildren<Button>();

            if (cancelButtonComponent == null)
            {
                Debug.LogError("❌ CharacterDisplay: No Button component found in cancelButtonRoot children!");
            }
            else
            {
                cancelButtonComponent.onClick.AddListener(OnCancelButtonClicked);

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
            UpdatePortraitColor(); // Apply current visual state to new sprite
        }
        else
        {
            Debug.LogWarning("portraitImage is not assigned in CharacterDisplay Inspector!");
        }
    }

    // ============================================
    // ACTION STATE UPDATES (Called by UIController)
    // ============================================

    public void SetBusyState(bool busy, ActionData action, float timeRemaining, float totalDuration)
    {
        isBusy = busy;

        if (busy && action != null)
        {
            // Show action UI
            if (actionText != null)
            {
                actionText.text = action.actionName.ToUpper();
                actionText.gameObject.SetActive(true);
            }

            if (timeBar != null)
            {
                float progress = 1f - (timeRemaining / totalDuration);
                timeBar.SetProgress(progress);
                timeBar.gameObject.SetActive(true);
            }

            // Show cancel button
            if (cancelButtonCanvasGroup != null)
            {
                cancelButtonCanvasGroup.alpha = 1f;
                cancelButtonCanvasGroup.interactable = true;
                cancelButtonCanvasGroup.blocksRaycasts = true;
            }
        }
        else
        {
            // Hide action UI
            HideActionUI();
        }

        UpdatePortraitColor();
    }

    public void UpdateActionProgress(float timeRemaining, float totalDuration)
    {
        if (timeBar != null && isBusy)
        {
            float progress = 1f - (timeRemaining / totalDuration);
            timeBar.SetProgress(progress);
        }
    }

    // ============================================
    // MOUSE INTERACTION (IPointerHandler)
    // ============================================

    /// <summary>
    /// Mouse enters portrait → apply hover tint + scale up
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        UpdatePortraitColor();
        transform.localScale = originalScale * hoverScaleMultiplier;

        // COMMENTED OUT - UIController_Game doesn't have this method anymore
        // UIController_Game uiController = FindObjectOfType<UIController_Game>();
        // if (uiController != null)
        // {
        //     uiController.SetHoveredCharacter(currentCharacterIndex);
        // }
    }

    /// <summary>
    /// Mouse exits portrait → remove hover tint + scale back to normal
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdatePortraitColor();
        transform.localScale = originalScale;

        // COMMENTED OUT - UIController_Game doesn't have this method anymore
        // UIController_Game uiController = FindObjectOfType<UIController_Game>();
        // if (uiController != null)
        // {
        //     uiController.ClearHoveredCharacter();
        // }
    }

    /// <summary>
    /// Mouse clicks portrait → tell UIController to select this character
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // COMMENTED OUT - UIController_Game doesn't have this method anymore
        // UIController_Game uiController = FindObjectOfType<UIController_Game>();
        // if (uiController != null)
        // {
        //     uiController.SelectCharacter(currentCharacterIndex);
        // }
        // else
        // {
        //     Debug.LogError("❌ CharacterDisplay: Cannot find UIController_Game!");
        // }
    }

    /// <summary>
    /// Called by UIController when selection changes
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdatePortraitColor();
    }

    /// <summary>
    /// Updates portrait color based on current state (hover, selection, busy)
    /// Priority: Busy > Selected > Hover > Normal
    /// </summary>
    private void UpdatePortraitColor()
    {
        if (portraitImage == null) return;

        Color targetColor = originalPortraitColor;

        if (isBusy)
        {
            // Busy state = darken portrait
            targetColor = busyTintColor;
        }
        else if (isSelected)
        {
            // Selected = highlight with selected color
            targetColor = selectedTintColor;
        }
        else if (isHovered)
        {
            // Hover = slight brightness boost
            targetColor = originalPortraitColor * hoverTintColor;
        }

        portraitImage.color = targetColor;
    }

    // ============================================
    // CANCEL BUTTON HANDLER
    // ============================================

    private void OnCancelButtonClicked()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("❌ CharacterDisplay: Cannot find GameManager!");
            return;
        }

        if (currentCharacterIndex < 0)
        {
            Debug.LogWarning("⚠️ CharacterDisplay: Cannot cancel - character index not set!");
            return;
        }

        Debug.Log($"🚫 Cancel button clicked for character slot {currentCharacterIndex}");
        GameManager.Instance.CancelAction(currentCharacterIndex);
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