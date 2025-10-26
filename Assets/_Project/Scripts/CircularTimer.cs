using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// CircularTimer - Visual display component for quarter countdown
/// Displays a radial filled circle that fills up as the quarter progresses
/// Attach to GameObject with Image component (set to Filled, Radial 360, Origin: Top)
/// </summary>
public class CircularTimer : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Image component with Fill Type: Filled, Fill Method: Radial 360, Fill Origin: Top")]
    public Image circularFillImage;

    [Header("Timer Display Mode")]
    [Tooltip("Use stepped time (updates each second) instead of smooth linear progression")]
    public bool useSteppedTime = false;

    [Header("Pause Indicator")]
    [Tooltip("GameObject to show when game is paused (typically TMP text saying 'PAUSED')")]
    public GameObject pauseIndicator;

    [Tooltip("Use smooth fade in/out (Option 2) instead of instant enable/disable (Option 1)")]
    public bool useFadeTransition = false;

    [Tooltip("Fade duration in seconds (only used if useFadeTransition is true)")]
    public float fadeDuration = 0.3f;

    // Private state
    private CanvasGroup pauseIndicatorCanvasGroup;
    private bool wasPaused = false;
    private int lastSecond = -1;
    private float quarterDuration = 30f; // Default fallback, will be loaded from GameRules

    void Start()
    {
        // Why: Load quarter duration from GameManager's rules
        if (GameManager.Instance?.rules != null)
        {
            quarterDuration = GameManager.Instance.rules.quarterDuration;
            Debug.Log($"⏱️ CircularTimer: Quarter duration loaded from GameRules = {quarterDuration}s");
        }
        else
        {
            Debug.LogWarning("⚠️ CircularTimer: GameRules not found! Using default 30 seconds.");
        }

        if (pauseIndicator == null) return;

        if (useFadeTransition)
        {
            // Why: Option 2 - Setup CanvasGroup for fade transitions
            pauseIndicatorCanvasGroup = pauseIndicator.GetComponent<CanvasGroup>();
            if (pauseIndicatorCanvasGroup == null)
            {
                pauseIndicatorCanvasGroup = pauseIndicator.AddComponent<CanvasGroup>();
            }

            // Why: Start with indicator invisible but GameObject active
            pauseIndicatorCanvasGroup.alpha = 0f;
            pauseIndicator.SetActive(true);
        }
        else
        {
            // Why: Option 1 - Simple mode, start disabled
            pauseIndicator.SetActive(false);
        }
    }

    void Update()
    {
        // Why: Update the fill amount every frame based on TimeManager
        if (TimeManager.Instance != null && circularFillImage != null)
        {
            float currentProgress = TimeManager.Instance.GetFillAmount();

            if (useSteppedTime)
            {
                // Why: Stepped mode - convert smooth progress to discrete steps
                // Calculate which second we're on based on progress
                int currentSecond = Mathf.FloorToInt(currentProgress * quarterDuration);

                // Why: Only update visual when second changes
                if (currentSecond != lastSecond)
                {
                    lastSecond = currentSecond;
                    // Why: Set fill to discrete step (e.g., 0/30, 1/30, 2/30, ...)
                    circularFillImage.fillAmount = currentSecond / quarterDuration;
                }
            }
            else
            {
                // Why: Linear mode - smooth continuous update
                circularFillImage.fillAmount = currentProgress;
                lastSecond = -1; // Reset for when switching back to stepped
            }
        }

        // Why: Handle pause indicator visibility
        if (TimeManager.Instance != null && pauseIndicator != null)
        {
            bool isPaused = TimeManager.Instance.isPaused;

            // Why: Only update when pause state changes
            if (isPaused != wasPaused)
            {
                if (useFadeTransition && pauseIndicatorCanvasGroup != null)
                {
                    // Why: Option 2 - Smooth fade transition
                    if (isPaused)
                    {
                        FadeInPauseIndicator();
                    }
                    else
                    {
                        FadeOutPauseIndicator();
                    }
                }
                else
                {
                    // Why: Option 1 - Instant enable/disable
                    pauseIndicator.SetActive(isPaused);
                }

                wasPaused = isPaused;
            }
        }
    }

    /// <summary>
    /// Option 2: Fades in the pause indicator smoothly
    /// </summary>
    private void FadeInPauseIndicator()
    {
        if (pauseIndicatorCanvasGroup == null) return;

        pauseIndicatorCanvasGroup.DOKill();
        pauseIndicatorCanvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.OutQuad);
    }

    /// <summary>
    /// Option 2: Fades out the pause indicator smoothly
    /// </summary>
    private void FadeOutPauseIndicator()
    {
        if (pauseIndicatorCanvasGroup == null) return;

        pauseIndicatorCanvasGroup.DOKill();
        pauseIndicatorCanvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.InQuad);
    }

    void OnValidate()
    {
        // Why: Auto-find Image component if not assigned
        if (circularFillImage == null)
        {
            circularFillImage = GetComponent<Image>();
        }
    }

    void OnDisable()
    {
        // Why: Clean up tweens when disabled
        if (pauseIndicatorCanvasGroup != null)
        {
            pauseIndicatorCanvasGroup.DOKill();
        }
    }
}