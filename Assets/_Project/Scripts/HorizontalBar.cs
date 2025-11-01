using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

/// <summary>
/// Horizontal bar display with unified 0.0-1.0 interface
/// 
/// DISCRETE MODE:
/// - Activates/deactivates segment GameObjects
/// - fillBar GameObject is disabled
/// - Each segment image should be cropped to show ONLY that blip
/// - Optional "burn effect" for color transitions
/// 
/// SMOOTH MODE:
/// - Single filled image bar (fillBar active, segments disabled)
/// 
/// USAGE:
/// - Discrete: Assign 10 segment GameObjects (Bar_10, Bar_09, ..., Bar_01)
/// - Smooth: Assign ONE fillBar (shows full bar, fillAmount controls visibility)
/// - Add "Frame" or "Overlay" as separate child (always visible)
/// </summary>
public class HorizontalBar : MonoBehaviour
{
    [Header("Value")]
    [Range(0f, 1f)]
    [Tooltip("Current fill value (0.0 = empty, 1.0 = full)")]
    public float value = 0f;

    [Header("Bar Mode")]
    [Tooltip("Discrete = segments (enable/disable GameObjects) | Smooth = fill bar")]
    public BarMode mode = BarMode.Discrete;

    [Header("Discrete Mode (Stats)")]
    [Tooltip("Drag Image components here - each shows ONE blip (cropped in Photoshop)")]
    public Image[] segments;

    [Header("Smooth Mode (Timers/Progress)")]
    [Tooltip("Single Image with Fill type - shows full bar, fillAmount controls visibility")]
    public Image fillBar;

    [Header("Colors")]
    [Tooltip("Fill color for active segments (Discrete) and fill bar (Smooth)")]
    public Color fillColor = Color.green;

    [Tooltip("Inactive segment color (only used if burn effect disabled)")]
    public Color inactiveColor = new Color(0.3f, 0.3f, 0.3f, 1f);

    [Header("Animation Effects (Discrete Mode Only)")]
    [Tooltip("Enable sequential wave fill animation")]
    public bool useSequentialFill = true;

    [Tooltip("Delay between each segment filling")]
    [Range(0f, 0.2f)]
    public float sequentialDelay = 0.05f;

    [Tooltip("Enable scale punch/bounce when segment activates")]
    public bool useScalePunch = true;

    [Tooltip("Scale punch strength")]
    [Range(1.0f, 1.5f)]
    public float punchScale = 1.15f;

    [Header("🔥 Burn Effect (Discrete Mode)")]
    [Tooltip("Enable color fade effect (like embers glowing/cooling)")]
    public bool useBurnEffect = false;

    [Tooltip("Color segments START at when activating (e.g., orange/red)")]
    public Color burnInColor = new Color(1f, 0.5f, 0f, 1f); // Orange

    [Tooltip("Color segments FADE TO when deactivating (e.g., dark red)")]
    public Color burnOutColor = new Color(0.5f, 0f, 0f, 1f); // Dark red

    [Tooltip("Burn transition speed (higher = faster)")]
    [Range(1f, 20f)]
    public float burnSpeed = 8f;

    // Private state
    private int lastDiscreteValue = -1;
    private float lastValue = -1f;

    void Start()
    {
        // Setup proper GameObject states based on mode
        SetupModeObjects();

        // Initialize bar to starting value
        SetProgress(value, instant: true);
        lastValue = value;
    }

    void Update()
    {
        // Apply value changes during Play mode
        if (Mathf.Abs(value - lastValue) > 0.001f)
        {
            SetProgress(value);
            lastValue = value;
        }
    }

    // ============================================
    // MODE SETUP
    // ============================================

    /// <summary>
    /// Enable/disable appropriate GameObjects based on current mode
    /// </summary>
    private void SetupModeObjects()
    {
        if (mode == BarMode.Discrete)
        {
            // DISCRETE: Enable segments, disable fillBar
            if (segments != null)
            {
                foreach (var seg in segments)
                {
                    if (seg != null)
                    {
                        seg.gameObject.SetActive(false); // Start disabled
                    }
                }
            }

            if (fillBar != null)
            {
                fillBar.gameObject.SetActive(false); // Disable smooth bar
            }
        }
        else // Smooth mode
        {
            // SMOOTH: Disable segments, enable fillBar
            if (segments != null)
            {
                foreach (var seg in segments)
                {
                    if (seg != null)
                    {
                        seg.gameObject.SetActive(false); // Disable all segments
                    }
                }
            }

            if (fillBar != null)
            {
                fillBar.gameObject.SetActive(true); // Enable smooth bar
                fillBar.color = fillColor; // Apply fill color
            }
        }
    }

    // ============================================
    // UNIFIED API - ONE METHOD TO RULE THEM ALL
    // ============================================

    /// <summary>
    /// Set bar progress from 0.0 (empty) to 1.0 (full)
    /// Works for both Discrete and Smooth modes
    /// </summary>
    public void SetProgress(float progressValue, bool instant = false)
    {
        value = Mathf.Clamp01(progressValue);

        if (mode == BarMode.Discrete)
        {
            SetProgressDiscrete(value, instant);
        }
        else
        {
            SetProgressSmooth(value);
        }
    }

    // ============================================
    // DISCRETE MODE IMPLEMENTATION
    // ============================================

    private void SetProgressDiscrete(float progressValue, bool instant)
    {
        if (segments == null || segments.Length == 0)
        {
            Debug.LogError($"HorizontalBar '{name}' has no segments assigned!");
            return;
        }

        // Calculate target based on actual segment count
        int maxSegments = segments.Length;
        int targetSegments = Mathf.RoundToInt(progressValue * maxSegments);
        targetSegments = Mathf.Clamp(targetSegments, 0, maxSegments);

        // If instant or no animations, update immediately
        if (instant || (!useSequentialFill && !useScalePunch && !useBurnEffect))
        {
            SetDiscreteInstant(targetSegments);
            return;
        }

        // Only animate if value actually changed
        if (targetSegments == lastDiscreteValue)
        {
            return;
        }

        // Stop any ongoing animations
        StopAllCoroutines();
        DOTween.Kill(transform);

        // Animate up or down
        if (targetSegments > lastDiscreteValue)
        {
            StartCoroutine(AnimateDiscreteUp(lastDiscreteValue, targetSegments));
        }
        else if (targetSegments < lastDiscreteValue)
        {
            StartCoroutine(AnimateDiscreteDown(lastDiscreteValue, targetSegments));
        }

        lastDiscreteValue = targetSegments;
    }

    private void SetDiscreteInstant(int targetSegments)
    {
        lastDiscreteValue = targetSegments;

        for (int i = 0; i < segments.Length; i++)
        {
            if (segments[i] != null)
            {
                bool shouldBeActive = (i < targetSegments);

                // Enable/disable entire GameObject
                segments[i].gameObject.SetActive(shouldBeActive);

                // Set color if active
                if (shouldBeActive)
                {
                    segments[i].color = fillColor; // Use unified fill color
                    segments[i].transform.localScale = Vector3.one;
                }
            }
        }
    }

    private IEnumerator AnimateDiscreteUp(int startValue, int targetValue)
    {
        int maxIndex = Mathf.Min(targetValue, segments.Length);

        for (int i = startValue; i < maxIndex; i++)
        {
            if (i >= 0 && i < segments.Length && segments[i] != null)
            {
                // Enable GameObject FIRST
                segments[i].gameObject.SetActive(true);

                // Burn effect: Start at burn-in color, fade to fill color
                if (useBurnEffect)
                {
                    segments[i].color = burnInColor; // Start orange/red
                    segments[i].DOColor(fillColor, 1f / burnSpeed); // Fade to fill color
                }
                else
                {
                    segments[i].color = fillColor; // Use unified fill color
                }

                // Scale punch
                if (useScalePunch)
                {
                    segments[i].transform.localScale = Vector3.one;
                    segments[i].transform.DOPunchScale(Vector3.one * (punchScale - 1f), 0.2f, 1, 0.5f);
                }

                // Sequential delay
                if (useSequentialFill && sequentialDelay > 0f)
                {
                    yield return new WaitForSeconds(sequentialDelay);
                }
            }
        }
    }

    private IEnumerator AnimateDiscreteDown(int startValue, int targetValue)
    {
        int maxIndex = Mathf.Min(startValue, segments.Length);

        for (int i = maxIndex - 1; i >= targetValue; i--)
        {
            if (i >= 0 && i < segments.Length && segments[i] != null)
            {
                // Burn effect: Fade from fill color to burn-out color, THEN disable
                if (useBurnEffect)
                {
                    float fadeDuration = 1f / burnSpeed;

                    // Fade to dark/cooling color
                    segments[i].DOColor(burnOutColor, fadeDuration).OnComplete(() =>
                    {
                        // Disable GameObject AFTER fade completes
                        if (segments[i] != null)
                        {
                            segments[i].gameObject.SetActive(false);
                        }
                    });
                }
                else
                {
                    // No burn effect - just disable immediately
                    segments[i].gameObject.SetActive(false);
                }

                // Smaller scale punch when going down
                if (useScalePunch)
                {
                    segments[i].transform.localScale = Vector3.one;
                    segments[i].transform.DOPunchScale(Vector3.one * (punchScale - 1f) * 0.5f, 0.2f, 1, 0.5f);
                }

                // Sequential delay
                if (useSequentialFill && sequentialDelay > 0f)
                {
                    yield return new WaitForSeconds(sequentialDelay);
                }
            }
        }
    }

    // ============================================
    // SMOOTH MODE IMPLEMENTATION
    // ============================================

    private void SetProgressSmooth(float progressValue)
    {
        if (fillBar == null)
        {
            Debug.LogError($"HorizontalBar '{name}' has no fillBar assigned!");
            return;
        }

        // Ensure fillBar is active and colored
        if (!fillBar.gameObject.activeSelf)
        {
            fillBar.gameObject.SetActive(true);
        }

        fillBar.color = fillColor; // Use unified fill color
        fillBar.fillAmount = progressValue;
    }

    // ============================================
    // CONTEXT MENU HELPERS
    // ============================================

    [ContextMenu("Setup Mode Objects")]
    private void SetupModeObjectsMenu()
    {
        SetupModeObjects();
        Debug.Log($"✅ HorizontalBar '{name}' configured for {mode} mode");
    }

    [ContextMenu("Test: Fill to Max")]
    private void TestFillMax()
    {
        if (Application.isPlaying)
        {
            SetProgress(1f);
        }
    }

    [ContextMenu("Test: Empty Bar")]
    private void TestEmpty()
    {
        if (Application.isPlaying)
        {
            SetProgress(0f);
        }
    }

    [ContextMenu("Test: Half Full")]
    private void TestHalf()
    {
        if (Application.isPlaying)
        {
            SetProgress(0.5f);
        }
    }

    [ContextMenu("Test: Burn Effect Demo")]
    private void TestBurnEffect()
    {
        if (Application.isPlaying && mode == BarMode.Discrete)
        {
            StartCoroutine(BurnEffectDemo());
        }
    }

    private IEnumerator BurnEffectDemo()
    {
        // Fill up
        SetProgress(1f);
        yield return new WaitForSeconds(2f);

        // Empty
        SetProgress(0f);
        yield return new WaitForSeconds(2f);

        // Fill halfway
        SetProgress(0.5f);
    }

    // ============================================
    // VALIDATION
    // ============================================

    private void OnValidate()
    {
        // Warn if segments array doesn't match expected count
        if (mode == BarMode.Discrete && segments != null && segments.Length != 10)
        {
            Debug.LogWarning($"HorizontalBar '{name}': Discrete mode works best with 10 segments (found {segments.Length})", this);
        }

        // Apply fill color if in smooth mode (editor preview)
        if (mode == BarMode.Smooth && fillBar != null && !Application.isPlaying)
        {
            fillBar.color = fillColor;
        }
    }
}

public enum BarMode
{
    Discrete,  // Segments that enable/disable (typically 10 for 0-10 stats)
    Smooth     // Smooth fill bar for timers/progress (0.0-1.0)
}