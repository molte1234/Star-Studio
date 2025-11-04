using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// UIButton - Handles button animations (hover, press, shadow)
/// NOW SUPPORTS TOGGLE BUTTONS: Stays in pressed state when Toggle is ON
/// </summary>
public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("Optional Shadow")]
    [Tooltip("Drag shadow GameObject here (should be sibling, not child). Leave empty if no shadow.")]
    public GameObject shadowObject;

    [Tooltip("How much shadow fades when button is pressed (0 = invisible, 1 = full opacity)")]
    [Range(0f, 1f)]
    public float shadowPressedAlpha = 0.3f;

    [Header("Hover Animation")]
    [Tooltip("Scale multiplier on hover (e.g., 1.05 = 5% bigger)")]
    public float hoverScale = 1.05f;

    [Header("Press Animation")]
    [Tooltip("Scale multiplier on press (e.g., 0.95 = 5% smaller)")]
    public float pressScale = 0.95f;

    [Tooltip("How many pixels to move left on press")]
    public float pressMoveLeft = 12f;

    [Tooltip("How many pixels to move down on press")]
    public float pressMoveDown = 12f;

    [Header("Animation Settings")]
    [Tooltip("Animation duration in seconds")]
    public float animationDuration = 0.2f;

    [Tooltip("Animation easing")]
    public Ease animationEase = Ease.OutBack;

    [Header("Optional: Override Sounds")]
    public AudioClip customHoverSound;
    public AudioClip customClickSound;

    // Store original values
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Vector3 shadowOriginalScale;
    private bool isHovering = false;
    private CanvasGroup shadowCanvasGroup;
    private float shadowOriginalAlpha = 1f;

    // Toggle support
    private Toggle toggleComponent;
    private Button buttonComponent;
    private ColorBlock originalColors;

    /// <summary>
    /// Provides access to the underlying Button's onClick event.
    /// Returns null if this UIButton wraps a Toggle instead of a Button.
    /// </summary>
    public Button.ButtonClickedEvent onClick
    {
        get
        {
            if (buttonComponent == null)
            {
                buttonComponent = GetComponent<Button>();
            }
            return buttonComponent?.onClick;
        }
    }

    void Awake()
    {
        // Why: Store original scale and position to return to later
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;

        // Why: Check if this is a Toggle button (check Toggle FIRST, then Button)
        toggleComponent = GetComponent<Toggle>();
        if (toggleComponent == null)
        {
            buttonComponent = GetComponent<Button>();
        }

        // Why: Store original colors from Toggle OR Button
        if (toggleComponent != null)
        {
            originalColors = toggleComponent.colors;
        }
        else if (buttonComponent != null)
        {
            originalColors = buttonComponent.colors;
        }

        // Why: Setup shadow if assigned
        if (shadowObject != null)
        {
            shadowOriginalScale = shadowObject.transform.localScale;

            // Get or add CanvasGroup for alpha fading
            shadowCanvasGroup = shadowObject.GetComponent<CanvasGroup>();
            if (shadowCanvasGroup == null)
            {
                shadowCanvasGroup = shadowObject.AddComponent<CanvasGroup>();
            }
            shadowOriginalAlpha = shadowCanvasGroup.alpha;
        }

        // Why: Subscribe to Toggle events if this is a Toggle button
        if (toggleComponent != null)
        {
            toggleComponent.onValueChanged.AddListener(OnToggleValueChanged);

            // Why: Apply initial state if Toggle starts ON
            if (toggleComponent.isOn)
            {
                ApplyTogglePressedState(immediate: true);
            }
        }
    }

    void OnDestroy()
    {
        // Why: Cleanup Toggle listener
        if (toggleComponent != null)
        {
            toggleComponent.onValueChanged.RemoveListener(OnToggleValueChanged);
        }
    }

    /// <summary>
    /// Called when Toggle value changes
    /// Keeps button in pressed state when Toggle is ON
    /// </summary>
    private void OnToggleValueChanged(bool isOn)
    {
        if (isOn)
        {
            // Why: Toggle is ON → stay in pressed state
            ApplyTogglePressedState(immediate: false);
        }
        else
        {
            // Why: Toggle is OFF → return to normal state
            ApplyToggleNormalState();
        }
    }

    /// <summary>
    /// Applies pressed state for Toggle ON
    /// If bool = true → normal color = pressed color
    /// </summary>
    private void ApplyTogglePressedState(bool immediate)
    {
        // Why: Scale down and move to pressed position
        Vector3 pressPosition = originalPosition + new Vector3(-pressMoveLeft, -pressMoveDown, 0);
        float duration = immediate ? 0f : animationDuration * 0.5f;

        transform.DOKill();
        transform.DOScale(originalScale * pressScale, duration).SetEase(Ease.OutQuad);
        transform.DOLocalMove(pressPosition, duration).SetEase(Ease.OutQuad);

        // Why: bool = true → Set normal color = pressed color
        if (toggleComponent != null)
        {
            ColorBlock colors = toggleComponent.colors;
            colors.normalColor = originalColors.pressedColor;
            toggleComponent.colors = colors;
        }
        else if (buttonComponent != null)
        {
            ColorBlock colors = buttonComponent.colors;
            colors.normalColor = originalColors.pressedColor;
            buttonComponent.colors = colors;
        }

        // Why: Scale down and fade shadow
        if (shadowObject != null)
        {
            shadowObject.transform.DOKill();
            shadowObject.transform.DOScale(shadowOriginalScale * pressScale, duration).SetEase(Ease.OutQuad);

            if (shadowCanvasGroup != null)
            {
                shadowCanvasGroup.DOKill();
                shadowCanvasGroup.DOFade(shadowPressedAlpha, duration).SetEase(Ease.OutQuad);
            }
        }
    }

    /// <summary>
    /// Returns to normal state for Toggle OFF
    /// If bool = false → normal color = original normal color
    /// </summary>
    private void ApplyToggleNormalState()
    {
        float duration = animationDuration * 0.5f;

        transform.DOKill();
        transform.DOScale(originalScale, duration).SetEase(Ease.OutQuad);
        transform.DOLocalMove(originalPosition, duration).SetEase(Ease.OutQuad);

        // Why: bool = false → Set normal color = original normal color
        if (toggleComponent != null)
        {
            ColorBlock colors = toggleComponent.colors;
            colors.normalColor = originalColors.normalColor;
            toggleComponent.colors = colors;
        }
        else if (buttonComponent != null)
        {
            ColorBlock colors = buttonComponent.colors;
            colors.normalColor = originalColors.normalColor;
            buttonComponent.colors = colors;
        }

        // Why: Return shadow to normal
        if (shadowObject != null)
        {
            shadowObject.transform.DOKill();
            shadowObject.transform.DOScale(shadowOriginalScale, duration).SetEase(Ease.OutQuad);

            if (shadowCanvasGroup != null)
            {
                shadowCanvasGroup.DOKill();
                shadowCanvasGroup.DOFade(shadowOriginalAlpha, duration).SetEase(Ease.OutQuad);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Why: Don't animate hover if Toggle is ON (already in pressed state)
        if (toggleComponent != null && toggleComponent.isOn) return;

        // Why: Scale up slightly on hover (no position change)
        isHovering = true;

        // Kill any existing tweens to prevent conflicts
        transform.DOKill();
        if (shadowObject != null) shadowObject.transform.DOKill();

        // Scale up button
        transform.DOScale(originalScale * hoverScale, animationDuration)
            .SetEase(animationEase);

        // Scale up shadow to match
        if (shadowObject != null)
        {
            shadowObject.transform.DOScale(shadowOriginalScale * hoverScale, animationDuration)
                .SetEase(animationEase);
        }

        // Play hover sound
        if (AudioManager.Instance != null)
        {
            if (customHoverSound != null)
            {
                AudioManager.Instance.PlaySFX(customHoverSound);
            }
            else
            {
                AudioManager.Instance.PlayButtonHover();
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Why: Don't animate exit if Toggle is ON (stay in pressed state)
        if (toggleComponent != null && toggleComponent.isOn) return;

        // Why: Return to original state when mouse leaves
        isHovering = false;

        // Kill any existing tweens
        transform.DOKill();
        if (shadowObject != null)
        {
            shadowObject.transform.DOKill();
            if (shadowCanvasGroup != null) shadowCanvasGroup.DOKill();
        }

        // Return button to original scale and position
        transform.DOScale(originalScale, animationDuration)
            .SetEase(Ease.OutQuad);

        transform.DOLocalMove(originalPosition, animationDuration)
            .SetEase(Ease.OutQuad);

        // Return shadow to original state
        if (shadowObject != null)
        {
            shadowObject.transform.DOScale(shadowOriginalScale, animationDuration)
                .SetEase(Ease.OutQuad);

            if (shadowCanvasGroup != null)
            {
                shadowCanvasGroup.DOFade(shadowOriginalAlpha, animationDuration)
                    .SetEase(Ease.OutQuad);
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Why: Scale down AND move left + down when pressed (button press effect)
        transform.DOKill();
        if (shadowObject != null)
        {
            shadowObject.transform.DOKill();
            if (shadowCanvasGroup != null) shadowCanvasGroup.DOKill();
        }

        // Scale down button
        transform.DOScale(originalScale * pressScale, animationDuration * 0.5f)
            .SetEase(Ease.OutQuad);

        // Move button left and down
        Vector3 pressPosition = originalPosition + new Vector3(-pressMoveLeft, -pressMoveDown, 0);
        transform.DOLocalMove(pressPosition, animationDuration * 0.5f)
            .SetEase(Ease.OutQuad);

        // Scale down and fade shadow
        if (shadowObject != null)
        {
            shadowObject.transform.DOScale(shadowOriginalScale * pressScale, animationDuration * 0.5f)
                .SetEase(Ease.OutQuad);

            if (shadowCanvasGroup != null)
            {
                shadowCanvasGroup.DOFade(shadowPressedAlpha, animationDuration * 0.5f)
                    .SetEase(Ease.OutQuad);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Why: If this is a Toggle button and it's ON, stay in pressed state
        if (toggleComponent != null && toggleComponent.isOn)
        {
            // Stay pressed
            return;
        }

        // Why: Return to hover state if still hovering, otherwise return to normal
        transform.DOKill();
        if (shadowObject != null)
        {
            shadowObject.transform.DOKill();
            if (shadowCanvasGroup != null) shadowCanvasGroup.DOKill();
        }

        if (isHovering)
        {
            // Return to hover state (scaled up, original position)
            transform.DOScale(originalScale * hoverScale, animationDuration * 0.5f)
                .SetEase(Ease.OutQuad);

            transform.DOLocalMove(originalPosition, animationDuration * 0.5f)
                .SetEase(Ease.OutQuad);

            // Return shadow to hover state
            if (shadowObject != null)
            {
                shadowObject.transform.DOScale(shadowOriginalScale * hoverScale, animationDuration * 0.5f)
                    .SetEase(Ease.OutQuad);

                if (shadowCanvasGroup != null)
                {
                    shadowCanvasGroup.DOFade(shadowOriginalAlpha, animationDuration * 0.5f)
                        .SetEase(Ease.OutQuad);
                }
            }
        }
        else
        {
            // Return to normal state
            transform.DOScale(originalScale, animationDuration * 0.5f)
                .SetEase(Ease.OutQuad);

            transform.DOLocalMove(originalPosition, animationDuration * 0.5f)
                .SetEase(Ease.OutQuad);

            // Return shadow to normal state
            if (shadowObject != null)
            {
                shadowObject.transform.DOScale(shadowOriginalScale, animationDuration * 0.5f)
                    .SetEase(Ease.OutQuad);

                if (shadowCanvasGroup != null)
                {
                    shadowCanvasGroup.DOFade(shadowOriginalAlpha, animationDuration * 0.5f)
                        .SetEase(Ease.OutQuad);
                }
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Why: Play click sound when button is pressed
        if (AudioManager.Instance != null)
        {
            if (customClickSound != null)
            {
                AudioManager.Instance.PlaySFX(customClickSound);
            }
            else
            {
                AudioManager.Instance.PlayButtonClick();
            }
        }
    }

    void OnDisable()
    {
        // Why: Clean up tweens when button is disabled
        transform.DOKill();
        transform.localScale = originalScale;
        transform.localPosition = originalPosition;

        if (shadowObject != null)
        {
            shadowObject.transform.DOKill();
            shadowObject.transform.localScale = shadowOriginalScale;

            if (shadowCanvasGroup != null)
            {
                shadowCanvasGroup.DOKill();
                shadowCanvasGroup.alpha = shadowOriginalAlpha;
            }
        }
    }
}