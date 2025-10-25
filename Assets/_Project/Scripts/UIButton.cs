using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

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

    void Awake()
    {
        // Why: Store original scale and position to return to later
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;

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
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
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