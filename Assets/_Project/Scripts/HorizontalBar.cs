using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

/// <summary>
/// Simple horizontal bar display - works standalone
/// Supports discrete segments (0-10 stats) or smooth fill (timers/progress)
/// 
/// FEATURES:
/// - Discrete Mode: 10 segments that fill individually (for stats)
/// - Smooth Mode: Single fill bar (for timers/progress)
/// - Toggleable Effects: Sequential fill, scale punch, color lerp
/// - Bi-directional Animation: Animates both UP (filling) and DOWN (emptying)
/// - Inspector Testing: Test slider (instant) + Animation Preview slider (animated)
/// - Context Menu: Preview Animation, Fill to Max, Clear Bar
/// 
/// USAGE:
/// 1. Add component to GameObject with UI Images
/// 2. Choose mode (Discrete or Smooth)
/// 3. Assign segments[] or fillBar
/// 4. Toggle effects on/off as needed
/// 5. Test using sliders in Inspector (enter Play mode for animation preview)
/// 
/// KISS: Clean foundation, easy to add effects later (blink, pulse, shake)
/// </summary>
public class HorizontalBar : MonoBehaviour
{
    [Header("Bar Mode")]
    [Tooltip("Discrete = 10 segments for stats | Smooth = fill bar for timers")]
    public BarMode mode = BarMode.Discrete;

    [Header("Discrete Mode (Stats)")]
    [Tooltip("Drag 10 Image components here - each represents one stat point")]
    public Image[] segments; // 10 images for 0-10 values

    [Header("Smooth Mode (Timers/Progress)")]
    [Tooltip("Single Image with Fill type - for smooth progress bars")]
    public Image fillBar; // One image for smooth 0.0-1.0 fill

    [Header("Visual Settings")]
    [Tooltip("Color when bar is active/filled")]
    public Color activeColor = Color.green;

    [Tooltip("Color when bar segment is inactive/empty")]
    public Color inactiveColor = Color.gray;

    [Header("Animation Effects - Toggle On/Off")]
    [Tooltip("Enable sequential wave fill animation (segments fill one by one)")]
    public bool useSequentialFill = true;

    [Tooltip("Delay between each segment filling (in seconds)")]
    [Range(0f, 0.2f)]
    public float sequentialDelay = 0.05f;

    [Tooltip("Enable scale punch/bounce when segment activates")]
    public bool useScalePunch = true;

    [Tooltip("Scale punch strength (1.0 = no punch, 1.2 = 20% bigger)")]
    [Range(1.0f, 1.5f)]
    public float punchScale = 1.15f;

    [Tooltip("Enable smooth color transition (lerp from inactive to active)")]
    public bool useColorLerp = true;

    [Tooltip("Color transition speed (higher = faster)")]
    [Range(1f, 20f)]
    public float colorLerpSpeed = 10f;

    [Header("Preview Animation (Inspector Only)")]
    [Tooltip("Non-interactive - shows animation progress for testing")]
    [Range(0f, 1f)]
    public float animationPreview = 0f;

    [Header("Testing (Inspector Only)")]
    [Range(0f, 1f)]
    [Tooltip("Drag slider to test bar in editor - applies immediately")]
    public float testValue = 0f;

    // Why: Current value being displayed (for reference/debugging)
    private int currentDiscreteValue = 0;
    private float currentSmoothValue = 0f;
    private float lastTestValue = -1f; // Track last test value to detect changes
    private float lastAnimationPreview = -1f; // Track animation preview slider

    private void Start()
    {
        // Why: Initialize bar to starting state
        if (mode == BarMode.Discrete)
        {
            int discreteValue = Mathf.RoundToInt(testValue * 10f);
            SetValue(discreteValue, 10, instant: true);
        }
        else
        {
            SetFillPercent(testValue);
        }

        lastTestValue = testValue;
        lastAnimationPreview = animationPreview;
    }

    private void Update()
    {
        // Why: Apply test slider changes during Play mode (instant)
        if (Mathf.Abs(testValue - lastTestValue) > 0.001f)
        {
            if (mode == BarMode.Discrete)
            {
                int discreteValue = Mathf.RoundToInt(testValue * 10f);
                SetValue(discreteValue, 10, instant: true); // Instant for test slider
            }
            else
            {
                SetFillPercent(testValue);
            }

            lastTestValue = testValue;
        }

        // Why: Apply animation preview slider (animated)
        if (Mathf.Abs(animationPreview - lastAnimationPreview) > 0.001f)
        {
            if (mode == BarMode.Discrete)
            {
                int previewValue = Mathf.RoundToInt(animationPreview * 10f);
                SetValue(previewValue, 10); // Animated!
            }

            lastAnimationPreview = animationPreview;
        }
    }

    /// <summary>
    /// Set discrete value (0-10) - for stats
    /// Example: SetValue(7, 10) fills 7 out of 10 segments
    /// Animated version - respects effect toggles
    /// Animates UP or DOWN depending on previous value
    /// </summary>
    public void SetValue(int current, int max, bool instant = false)
    {
        // Why: Only works in Discrete mode
        if (mode != BarMode.Discrete)
        {
            Debug.LogWarning($"HorizontalBar '{name}' is not in Discrete mode!");
            return;
        }

        // Why: Safety check
        if (segments == null || segments.Length == 0)
        {
            Debug.LogError($"HorizontalBar '{name}' has no segments assigned!");
            return;
        }

        int targetValue = Mathf.Clamp(current, 0, max);

        // Why: If instant mode or no animations enabled, update immediately
        if (instant || (!useSequentialFill && !useScalePunch && !useColorLerp))
        {
            SetValueInstant(targetValue, max);
            return;
        }

        // Why: Stop any ongoing animations
        StopAllCoroutines();
        DOTween.Kill(transform); // Kill any tweens on this object

        // Why: Determine direction and start appropriate animation
        if (targetValue > currentDiscreteValue)
        {
            // Animating UP (filling)
            StartCoroutine(AnimatedFillUp(currentDiscreteValue, targetValue));
        }
        else if (targetValue < currentDiscreteValue)
        {
            // Animating DOWN (emptying)
            StartCoroutine(AnimatedFillDown(currentDiscreteValue, targetValue));
        }

        currentDiscreteValue = targetValue;
    }

    /// <summary>
    /// Instant version - no animations
    /// </summary>
    private void SetValueInstant(int current, int max)
    {
        currentDiscreteValue = Mathf.Clamp(current, 0, max);

        // Why: Loop through segments and activate based on current value
        int activatedCount = 0;
        for (int i = 0; i < segments.Length; i++)
        {
            if (segments[i] != null)
            {
                // Why: Fill segments up to current value
                bool shouldBeActive = (i < currentDiscreteValue);
                segments[i].color = shouldBeActive ? activeColor : inactiveColor;
                segments[i].transform.localScale = Vector3.one;

                if (shouldBeActive) activatedCount++;
            }
        }

        Debug.Log($"SetValue: {current}/{max} → Activated {activatedCount}/{segments.Length} segments (instant)");
    }

    /// <summary>
    /// Animated fill UP coroutine - handles filling segments from start to target
    /// </summary>
    private IEnumerator AnimatedFillUp(int startValue, int targetValue)
    {
        // Why: Fill segments sequentially from startValue to targetValue
        for (int i = startValue; i < targetValue; i++)
        {
            if (segments[i] != null)
            {
                // Effect 1: Color lerp
                if (useColorLerp)
                {
                    segments[i].DOColor(activeColor, 1f / colorLerpSpeed);
                }
                else
                {
                    segments[i].color = activeColor;
                }

                // Effect 2: Scale punch
                if (useScalePunch)
                {
                    segments[i].transform.localScale = Vector3.one;
                    segments[i].transform.DOPunchScale(Vector3.one * (punchScale - 1f), 0.2f, 1, 0.5f);
                }

                // Effect 3: Sequential delay (wave effect)
                if (useSequentialFill && sequentialDelay > 0f)
                {
                    yield return new WaitForSeconds(sequentialDelay);
                }
            }
        }

        Debug.Log($"▲ AnimatedFillUp: {startValue} → {targetValue}/10 complete");
    }

    /// <summary>
    /// Animated fill DOWN coroutine - handles emptying segments from start down to target
    /// </summary>
    private IEnumerator AnimatedFillDown(int startValue, int targetValue)
    {
        // Why: Empty segments sequentially from startValue-1 down to targetValue (reverse order)
        for (int i = startValue - 1; i >= targetValue; i--)
        {
            if (segments[i] != null)
            {
                // Effect 1: Color lerp to inactive
                if (useColorLerp)
                {
                    segments[i].DOColor(inactiveColor, 1f / colorLerpSpeed);
                }
                else
                {
                    segments[i].color = inactiveColor;
                }

                // Effect 2: Scale punch (smaller punch when going down - feels like deflating)
                if (useScalePunch)
                {
                    segments[i].transform.localScale = Vector3.one;
                    segments[i].transform.DOPunchScale(Vector3.one * (punchScale - 1f) * 0.5f, 0.2f, 1, 0.5f);
                }

                // Effect 3: Sequential delay (wave effect in reverse)
                if (useSequentialFill && sequentialDelay > 0f)
                {
                    yield return new WaitForSeconds(sequentialDelay);
                }
            }
        }

        Debug.Log($"▼ AnimatedFillDown: {startValue} → {targetValue}/10 complete");
    }

    /// <summary>
    /// Set smooth fill (0.0 - 1.0) - for timers and progress
    /// Example: SetFillPercent(0.5f) fills bar to 50%
    /// </summary>
    public void SetFillPercent(float percent)
    {
        // Why: Only works in Smooth mode
        if (mode != BarMode.Smooth)
        {
            Debug.LogWarning($"HorizontalBar '{name}' is not in Smooth mode!");
            return;
        }

        // Why: Safety check
        if (fillBar == null)
        {
            Debug.LogError($"HorizontalBar '{name}' has no fillBar assigned!");
            return;
        }

        currentSmoothValue = Mathf.Clamp01(percent);
        fillBar.fillAmount = currentSmoothValue;

        // TODO: Add color gradient based on percent (red when low, green when high)
        // TODO: Add pulse effect when timer running low
    }

    /// <summary>
    /// Get current displayed value (for debugging or external checks)
    /// </summary>
    public float GetCurrentValue()
    {
        return (mode == BarMode.Discrete) ? currentDiscreteValue : currentSmoothValue;
    }

    /// <summary>
    /// Called in editor when inspector values change - for testing
    /// </summary>
    private void OnValidate()
    {
        // Why: Don't try to update if we're not in play mode and components aren't ready
        if (!Application.isPlaying)
        {
            // Only update if segments/fillBar are actually assigned
            if (mode == BarMode.Discrete && segments != null && segments.Length > 0)
            {
                // Convert 0-1 range to 0-10 discrete value
                int discreteValue = Mathf.RoundToInt(testValue * 10f);
                Debug.Log($"TestValue: {testValue} → Discrete: {discreteValue}/10");
                SetValue(discreteValue, 10, instant: true); // Instant in editor
            }
            else if (mode == BarMode.Smooth && fillBar != null)
            {
                Debug.Log($"TestValue: {testValue} → Fill: {testValue * 100f}%");
                SetFillPercent(testValue);
            }
        }
    }

    /// <summary>
    /// Preview animation in editor - right-click component → Preview Animation
    /// </summary>
    [ContextMenu("Preview Animation")]
    private void PreviewAnimation()
    {
        if (Application.isPlaying && mode == BarMode.Discrete)
        {
            int previewValue = Mathf.RoundToInt(animationPreview * 10f);
            Debug.Log($"🎬 Previewing animation: {previewValue}/10");
            SetValue(previewValue, 10);
        }
        else if (!Application.isPlaying)
        {
            Debug.LogWarning("⚠️ Preview Animation only works in Play mode! Enter Play mode and try again.");
        }
    }

    /// <summary>
    /// Quick test - fill to maximum with animation
    /// </summary>
    [ContextMenu("Test: Fill to Max (Animated)")]
    private void TestFillToMax()
    {
        if (Application.isPlaying && mode == BarMode.Discrete)
        {
            SetValue(10, 10);
        }
    }

    /// <summary>
    /// Quick test - clear bar
    /// </summary>
    [ContextMenu("Test: Clear Bar")]
    private void TestClearBar()
    {
        if (Application.isPlaying && mode == BarMode.Discrete)
        {
            SetValue(0, 10, instant: true);
        }
    }

    // ========================================
    // FUTURE EFFECTS - Add these methods later when needed:
    // ========================================

    // public void Blink(float duration = 0.2f)
    // {
    //     // Quick flash for damage/change feedback
    //     // transform.DOPunchScale(Vector3.one * 0.1f, duration);
    // }

    // public void Pulse(float duration = 1f)
    // {
    //     // Gentle ongoing pulse - good for warnings
    //     // transform.DOScale(1.05f, duration).SetLoops(-1, LoopType.Yoyo);
    // }

    // public void FlashColor(Color color, float duration = 0.3f)
    // {
    //     // Quick color flash for feedback (damage = red, heal = green)
    //     // foreach (var segment in segments) segment.DOColor(color, duration).From();
    // }

    // public void Shake()
    // {
    //     // Quick shake effect using Feel or DOTween
    //     // transform.DOShakePosition(0.3f, 5f, 20);
    // }

    // public void PlaySoundPerSegment()
    // {
    //     // Optional: Play quiet tick sound when each segment fills
    //     // if (AudioManager.Instance) AudioManager.Instance.PlaySFX("segment_tick");
    // }
}

public enum BarMode
{
    Discrete,  // 10 segments for stats (0-10)
    Smooth     // Smooth fill for timers/progress (0.0-1.0)
}