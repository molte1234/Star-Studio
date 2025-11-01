using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Manages a single character portrait display
/// Shows character portrait, action status, time remaining, and handles mouse interactions
/// UPDATED: Added mouse-over (hover) and selection (click) functionality + stats panel integration
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

    [Header("Mouse Interaction Colors")]
    [Tooltip("Color multiplier when mouse hovers over portrait")]
    public Color hoverTintColor = new Color(1.2f, 1.2f, 1.2f, 1f);

    [Tooltip("Color tint when character is selected (clicked)")]
    public Color selectedTintColor = new Color(0.5f, 1f, 1f, 1f); // Cyan-ish

    [Header("Mouse Interaction Scale")]
    [Tooltip("Scale multiplier on hover (e.g., 1.05 = 5% bigger)")]
    [Range(1.0f, 1.2f)]
    public float hoverScaleMultiplier = 1.05f;

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

        HideActionUI();
    }

    // ============================================
    // BUSY STATE - CALLED ONCE ON STATE CHANGE
    // ============================================

    public void SetBusyState(bool busy, string actionName = "")
    {
        isBusy = busy;

        if (busy)
        {
            // Show action name
            if (actionText != null)
            {
                actionText.text = actionName.ToUpper();
                actionText.gameObject.SetActive(true);
            }

            // Show progress bar
            if (timeBar != null)
            {
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
            // Hide all busy UI
            HideActionUI();
        }

        // Update portrait color based on new busy state
        UpdatePortraitColor();
    }

    // ============================================
    // PROGRESS BAR UPDATE - CALLED EVERY FRAME
    // ============================================

    public void UpdateProgress(float progress)
    {
        if (timeBar != null)
        {
            timeBar.SetProgress(progress);
        }
    }

    // ============================================
    // MOUSE INTERACTION
    // ============================================

    /// <summary>
    /// Called when mouse enters portrait area
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        UpdatePortraitColor();

        // Scale up slightly (instant, no animation)
        transform.localScale = originalScale * hoverScaleMultiplier;

        // Notify UIController to show this character's stats
        UIController_Game uiController = FindObjectOfType<UIController_Game>();
        if (uiController != null)
        {
            uiController.SetHoveredCharacter(currentCharacterIndex);
        }
    }

    /// <summary>
    /// Called when mouse leaves portrait area
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdatePortraitColor();

        // Scale back to original (instant, no animation)
        transform.localScale = originalScale;

        // Notify UIController hover ended
        UIController_Game uiController = FindObjectOfType<UIController_Game>();
        if (uiController != null)
        {
            uiController.ClearHoveredCharacter();
        }
    }

    /// <summary>
    /// Called when portrait is clicked
    /// Notifies UIController to select this character
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // Tell UIController to select this character
        UIController_Game uiController = FindObjectOfType<UIController_Game>();
        if (uiController != null)
        {
            uiController.SelectCharacter(currentCharacterIndex);
        }
        else
        {
            Debug.LogError("❌ CharacterDisplay: Cannot find UIController_Game!");
        }
    }

    // ============================================
    // SELECTION STATE (Called by UIController)
    // ============================================

    /// <summary>
    /// Set whether this character is selected (called by UIController)
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdatePortraitColor();
    }

    // ============================================
    // COLOR MANAGEMENT
    // ============================================

    /// <summary>
    /// Update portrait color based on current state (busy, hover, selected)
    /// Priority: Busy > Selected > Hover > Normal
    /// </summary>
    private void UpdatePortraitColor()
    {
        if (portraitImage == null) return;

        Color targetColor = originalPortraitColor;

        // Priority 1: Busy state (overrides everything)
        if (isBusy)
        {
            targetColor = busyTintColor;
        }
        // Priority 2: Selected
        else if (isSelected)
        {
            targetColor = selectedTintColor;
        }
        // Priority 3: Hover
        else if (isHovered)
        {
            targetColor = originalPortraitColor * hoverTintColor; // Multiply for brightness boost
        }
        // Priority 4: Normal
        else
        {
            targetColor = originalPortraitColor;
        }

        portraitImage.color = targetColor;
    }

    // ============================================
    // CANCEL BUTTON
    // ============================================

    private void OnCancelButtonClicked()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("❌ CharacterDisplay: GameManager.Instance is null!");
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