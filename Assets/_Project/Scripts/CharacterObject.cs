using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Visual representation of a character in a room
/// Replaces CharacterDisplay for the room-based system
/// Handles full-body sprites at socket positions
/// 
/// SETUP:
/// 1. Create GameObject with Image component
/// 2. Add this script
/// 3. Wire up references
/// 4. Save as prefab
/// </summary>
public class CharacterObject : MonoBehaviour
{
    // ============================================
    // VISUAL COMPONENTS
    // ============================================

    [Header("Core Display")]
    [Tooltip("Main character sprite (full-body)")]
    public Image characterImage;

    [Tooltip("Canvas group for fade effects")]
    public CanvasGroup canvasGroup;

    [Header("Optional UI Elements")]
    [Tooltip("Character name display")]
    public TextMeshProUGUI nameText;

    [Tooltip("Shows when character is busy")]
    public GameObject busyIndicator;

    [Tooltip("Shows action name when busy")]
    public TextMeshProUGUI actionText;

    [Tooltip("Progress bar for actions")]
    public Image progressBar;

    [Header("Speech Bubble")]
    [Tooltip("Position where speech bubbles spawn from")]
    public Transform mouthPosition;

    // ============================================
    // ANIMATION SETTINGS
    // ============================================

    [Header("Animation")]
    [Tooltip("Fade in duration when spawning")]
    public float fadeInDuration = 0.5f;

    [Tooltip("Fade out duration when despawning")]
    public float fadeOutDuration = 0.3f;

    [Tooltip("Enable idle breathing animation")]
    public bool enableBreathing = true;

    [Tooltip("Breathing scale amount")]
    public float breathingScale = 1.02f;

    [Tooltip("Breathing cycle duration")]
    public float breathingDuration = 2f;

    // ============================================
    // INTERACTION SETTINGS
    // ============================================

    [Header("Interaction")]
    [Tooltip("Scale on hover")]
    public float hoverScale = 1.05f;

    [Tooltip("Hover animation duration")]
    public float hoverDuration = 0.2f;

    [Tooltip("Click animation scale")]
    public float clickScale = 0.95f;

    // ============================================
    // RUNTIME STATE
    // ============================================

    private CharacterSlotState characterState;
    private RoomController roomController; // Reference to the room this character is in
    private bool isHovered = false;
    private Tween breathingTween;
    private Tween currentTween;
    private Vector3 baseScale = Vector3.one; // Store the base scale set by socket sizing

    // ============================================
    // INITIALIZATION
    // ============================================

    void Awake()
    {
        // Ensure we have a canvas group for fading
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        // Start invisible
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    void OnDestroy()
    {
        // Clean up tweens
        breathingTween?.Kill();
        currentTween?.Kill();
    }

    // ============================================
    // CHARACTER SETUP
    // ============================================

    /// <summary>
    /// Set the character this object represents
    /// Called by RoomController when spawning
    /// </summary>
    public void SetCharacter(CharacterSlotState state)
    {
        characterState = state;

        if (state == null || state.slotData == null)
        {
            Debug.LogError("CharacterObject: Null character state!");
            return;
        }

        // Set sprite
        if (characterImage != null && state.slotData.sprite != null)
        {
            characterImage.sprite = state.slotData.sprite;
            characterImage.preserveAspect = true;
        }

        // Set name
        if (nameText != null)
        {
            nameText.text = state.slotData.displayName;
        }

        // Update busy state
        UpdateBusyState();

        // Start breathing animation
        if (enableBreathing)
        {
            StartBreathing();
        }

        Debug.Log($"🎭 CharacterObject: Set to {state.slotData.displayName}");
    }

    /// <summary>
    /// Set the room controller this character belongs to
    /// Called by RoomController when spawning
    /// </summary>
    public void SetRoomController(RoomController controller)
    {
        roomController = controller;
    }

    /// <summary>
    /// Get the character state this object represents
    /// </summary>
    public CharacterSlotState GetCharacter()
    {
        return characterState;
    }

    /// <summary>
    /// Get the base scale (set by socket sizing)
    /// </summary>
    public Vector3 GetBaseScale()
    {
        return baseScale;
    }

    // ============================================
    // VISUAL STATES
    // ============================================

    /// <summary>
    /// Update visual based on character's busy state
    /// </summary>
    public void UpdateBusyState()
    {
        if (characterState == null) return;

        bool isBusy = characterState.isBusy;

        // Show/hide busy indicator
        if (busyIndicator != null)
        {
            busyIndicator.SetActive(isBusy);
        }

        // Update action text
        if (actionText != null)
        {
            if (isBusy && characterState.currentAction != null)
            {
                actionText.text = characterState.currentAction.actionName;
                actionText.gameObject.SetActive(true);
            }
            else
            {
                actionText.gameObject.SetActive(false);
            }
        }

        // Update progress bar
        if (progressBar != null)
        {
            if (isBusy && characterState.actionTotalDuration > 0)
            {
                float progress = 1f - (characterState.actionTimeRemaining / characterState.actionTotalDuration);
                progressBar.fillAmount = progress;
                progressBar.gameObject.SetActive(true);
            }
            else
            {
                progressBar.gameObject.SetActive(false);
            }
        }

        // Darken sprite if busy
        if (characterImage != null)
        {
            Color targetColor = isBusy
                ? new Color(0.7f, 0.7f, 0.7f, 1f)  // Darker when busy
                : Color.white;                      // Normal when idle

            characterImage.DOColor(targetColor, 0.3f);
        }
    }

    /// <summary>
    /// Apply room-specific lighting/tint
    /// </summary>
    public void SetRoomLighting(Color tint)
    {
        if (characterImage != null)
        {
            // Multiply current color by tint
            Color current = characterImage.color;
            characterImage.color = new Color(
                current.r * tint.r,
                current.g * tint.g,
                current.b * tint.b,
                current.a * tint.a
            );
        }
    }

    // ============================================
    // ANIMATIONS
    // ============================================

    /// <summary>
    /// Fade in when spawning
    /// </summary>
    public void FadeIn()
    {
        if (canvasGroup == null) return;

        // Store the base scale (set by RoomController based on socket size)
        baseScale = transform.localScale;

        canvasGroup.alpha = 0f;
        transform.localScale = baseScale * 0.8f;

        // Fade and scale in
        DOTween.Sequence()
            .Join(canvasGroup.DOFade(1f, fadeInDuration))
            .Join(transform.DOScale(baseScale, fadeInDuration).SetEase(Ease.OutBack))
            .OnComplete(() => {
                Debug.Log($"✨ {characterState?.slotData?.displayName} appeared!");
            });
    }

    /// <summary>
    /// Fade out when despawning
    /// </summary>
    public void FadeOut()
    {
        if (canvasGroup == null) return;

        DOTween.Sequence()
            .Join(canvasGroup.DOFade(0f, fadeOutDuration))
            .Join(transform.DOScale(baseScale * 0.8f, fadeOutDuration).SetEase(Ease.InBack))
            .OnComplete(() => {
                Destroy(gameObject);
            });
    }

    /// <summary>
    /// Start idle breathing animation
    /// </summary>
    private void StartBreathing()
    {
        if (!enableBreathing || characterImage == null) return;

        // Kill existing breathing
        breathingTween?.Kill();

        // Create breathing loop
        breathingTween = transform
            .DOScale(baseScale * breathingScale, breathingDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    // ============================================
    // INTERACTION
    // ============================================

    void OnMouseEnter()
    {
        if (isHovered) return;
        isHovered = true;

        // Scale up
        currentTween?.Kill();
        currentTween = transform.DOScale(baseScale * hoverScale, hoverDuration).SetEase(Ease.OutBack);

        // Play hover sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonHover();
        }
    }

    void OnMouseExit()
    {
        if (!isHovered) return;
        isHovered = false;

        // Scale back
        currentTween?.Kill();
        currentTween = transform.DOScale(baseScale, hoverDuration).SetEase(Ease.OutBack);
    }

    void OnMouseDown()
    {
        // Click animation
        currentTween?.Kill();
        currentTween = transform.DOScale(baseScale * clickScale, 0.1f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                transform.DOScale(isHovered ? baseScale * hoverScale : baseScale, 0.1f);
            });

        // Play click sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        // Notify UI that this character was clicked
        CharacterClicked();
    }

    /// <summary>
    /// Handle character click - opens the character menu with action buttons
    /// </summary>
    private void CharacterClicked()
    {
        Debug.Log($"👆 Clicked on {characterState?.slotData?.displayName}");

        // Check if character is busy - don't open menu if busy
        if (characterState != null && characterState.isBusy)
        {
            Debug.Log($"   Character is busy with: {characterState.currentAction?.actionName}");
            // Could show a "busy" feedback here
            return;
        }

        // Open character menu via CharacterMenuController
        if (CharacterMenuController.Instance != null && roomController != null)
        {
            CharacterMenuController.Instance.OpenCharacterMenu(this, roomController);
        }
        else
        {
            if (CharacterMenuController.Instance == null)
            {
                Debug.LogWarning("CharacterMenuController instance not found!");
            }
            if (roomController == null)
            {
                Debug.LogWarning("RoomController reference not set for this character!");
            }
        }
    }

    // ============================================
    // UPDATE
    // ============================================

    void Update()
    {
        // Update progress bar every frame if busy
        if (characterState != null && characterState.isBusy && progressBar != null)
        {
            if (characterState.actionTotalDuration > 0)
            {
                float progress = 1f - (characterState.actionTimeRemaining / characterState.actionTotalDuration);
                progressBar.fillAmount = progress;
            }
        }
    }
}