using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

/// <summary>
/// Simple horizontal bar display with unified 0.0-1.0 interface
/// Supports discrete segments (stats) or smooth fill (timers/progress)
/// 
/// UNIFIED API: SetProgress(float value) where value is 0.0 to 1.0
/// - Discrete Mode: Converts to 10 segments internally
/// - Smooth Mode: Uses value directly as fillAmount
/// 
/// KISS: One method to rule them all
/// </summary>
public class HorizontalBar : MonoBehaviour
{
    [Header("═══ TEST VALUE - Drag to Test ═══")]
    [Tooltip("Drag this slider to test bar in Inspector (Play mode)")]
    [Range(0f, 1f)]
    public float currentValue = 0f;

    [Header("Bar Mode")]
    [Tooltip("Discrete = 10 segments for stats | Smooth = fill bar for timers")]
    public BarMode mode = BarMode.Discrete;

    [Header("Discrete Mode (Stats)")]
    [Tooltip("Drag 10 Image components here - each represents one stat point")]
    public Image[] segments;

    [Header("Smooth Mode (Timers/Progress)")]
    [Tooltip("Single Image with Fill type - for smooth progress bars")]
    public Image fillBar;

    [Header("Visual Settings")]
    [Tooltip("Color when bar is active/filled")]
    public Color activeColor = Color.green;

    [Tooltip("Color when bar segment is inactive/empty")]
    public Color inactiveColor = Color.gray;

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

    [Tooltip("Enable smooth color transition")]
    public bool useColorLerp = true;

    [Tooltip("Color transition speed")]
    [Range(1f, 20f)]
    public float colorLerpSpeed = 10f;

    // Private state
    private int lastDiscreteValue = -1;
    private float lastTestValue = -1f;

    void Start()
    {
        // Initialize bar to starting value
        SetProgress(currentValue, instant: true);
        lastTestValue = currentValue;
    }

    void Update()
    {
        // Apply test slider changes during Play mode
        if (Mathf.Abs(currentValue - lastTestValue) > 0.001f)
        {
            SetProgress(currentValue);
            lastTestValue = currentValue;
        }
    }

    // ============================================
    // UNIFIED API - ONE METHOD TO RULE THEM ALL
    // ============================================

    /// <summary>
    /// Set bar progress from 0.0 (empty) to 1.0 (full)
    /// Works for both Discrete and Smooth modes
    /// </summary>
    /// <param name="value">Progress value 0.0-1.0</param>
    /// <param name="instant">Skip animations (instant update)</param>
    public void SetProgress(float value, bool instant = false)
    {
        // Clamp to valid range
        currentValue = Mathf.Clamp01(value);

        if (mode == BarMode.Discrete)
        {
            SetProgressDiscrete(currentValue, instant);
        }
        else
        {
            SetProgressSmooth(currentValue);
        }
    }

    // ============================================
    // DISCRETE MODE IMPLEMENTATION
    // ============================================

    private void SetProgressDiscrete(float value, bool instant)
    {
        // Safety check
        if (segments == null || segments.Length == 0)
        {
            Debug.LogError($"HorizontalBar '{name}' has no segments assigned!");
            return;
        }

        // Convert 0.0-1.0 to 0-10 segments
        int targetSegments = Mathf.RoundToInt(value * 10f);
        targetSegments = Mathf.Clamp(targetSegments, 0, 10);

        // If instant or no animations, update immediately
        if (instant || (!useSequentialFill && !useScalePunch && !useColorLerp))
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
                segments[i].color = shouldBeActive ? activeColor : inactiveColor;
                segments[i].transform.localScale = Vector3.one;
            }
        }
    }

    private IEnumerator AnimateDiscreteUp(int startValue, int targetValue)
    {
        for (int i = startValue; i < targetValue; i++)
        {
            if (segments[i] != null)
            {
                // Color lerp
                if (useColorLerp)
                {
                    segments[i].DOColor(activeColor, 1f / colorLerpSpeed);
                }
                else
                {
                    segments[i].color = activeColor;
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
        for (int i = startValue - 1; i >= targetValue; i--)
        {
            if (segments[i] != null)
            {
                // Color lerp to inactive
                if (useColorLerp)
                {
                    segments[i].DOColor(inactiveColor, 1f / colorLerpSpeed);
                }
                else
                {
                    segments[i].color = inactiveColor;
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

    private void SetProgressSmooth(float value)
    {
        // Safety check
        if (fillBar == null)
        {
            Debug.LogError($"HorizontalBar '{name}' has no fillBar assigned!");
            return;
        }

        // Set fill amount directly
        fillBar.fillAmount = value;

        // TODO: Color gradient based on value (red when low, green when high)
        // TODO: Pulse effect when timer running low
    }

    // ============================================
    // CONTEXT MENU HELPERS
    // ============================================

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
}

public enum BarMode
{
    Discrete,  // 10 segments for stats (0-10)
    Smooth     // Smooth fill for timers/progress (0.0-1.0)
}