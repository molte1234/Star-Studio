using UnityEngine;
using DG.Tweening;

/// <summary>
/// Central UI authority for the entire game
/// Singleton that persists across scenes
/// Holds reference to UIJuiceSettings and provides global access
/// Handles UI updates, displays, and feedback coordination
/// </summary>
public class UIManager : MonoBehaviour
{
    // Singleton pattern
    public static UIManager Instance { get; private set; }

    [Header("=== JUICE SETTINGS ===")]
    [Tooltip("Reference to the UIJuiceData ScriptableObject")]
    public UIJuiceData juiceSettings;

    [Header("=== SCENE REFERENCES ===")]
    [Tooltip("Main Canvas - will be populated automatically per scene")]
    public Canvas mainCanvas;

    [Header("=== DEBUG ===")]
    [Tooltip("Show debug logs for UI operations")]
    public bool debugLogs = false;

    // ===========================================
    // LIFECYCLE
    // ===========================================

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (debugLogs) Debug.Log("✨ UIManager initialized");
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Validate settings
        if (juiceSettings == null)
        {
            Debug.LogWarning("⚠️ UIJuiceData not assigned to UIManager!");
        }
    }

    private void Start()
    {
        // TODO: Initialize any UI systems that need setup
        InitializeUI();
    }

    // ===========================================
    // INITIALIZATION
    // ===========================================

    private void InitializeUI()
    {
        // TODO: Setup global UI systems
        // - Find main canvas if not assigned
        // - Setup event system
        // - Initialize tooltip system
        // - Setup screen overlay effects

        if (mainCanvas == null)
        {
            mainCanvas = FindObjectOfType<Canvas>();
            if (mainCanvas != null && debugLogs)
            {
                Debug.Log($"📺 Auto-found Canvas: {mainCanvas.name}");
            }
        }
    }

    // ===========================================
    // JUICE HELPERS (Convenience methods)
    // ===========================================

    /// <summary>
    /// Play button hover animation on any transform
    /// </summary>
    public void PlayButtonHover(Transform target)
    {
        if (juiceSettings == null || target == null) return;

        target.DOScale(juiceSettings.buttonHoverScale, juiceSettings.buttonHoverDuration)
            .SetEase(juiceSettings.buttonEase)
            .SetUpdate(true); // Ignore timescale

        if (juiceSettings.debugMode) Debug.Log($"🎯 Button hover: {target.name}");
    }

    /// <summary>
    /// Play button press animation on any transform
    /// </summary>
    public void PlayButtonPress(Transform target)
    {
        if (juiceSettings == null || target == null) return;

        target.DOScale(juiceSettings.buttonPressScale, juiceSettings.buttonPressDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true)
            .OnComplete(() => {
                // Return to hover scale
                target.DOScale(juiceSettings.buttonHoverScale, juiceSettings.buttonPressDuration)
                    .SetEase(juiceSettings.buttonEase);
            });

        if (juiceSettings.debugMode) Debug.Log($"👆 Button press: {target.name}");
    }

    /// <summary>
    /// Reset button to normal scale
    /// </summary>
    public void ResetButtonScale(Transform target)
    {
        if (juiceSettings == null || target == null) return;

        target.DOScale(1f, juiceSettings.buttonHoverDuration)
            .SetEase(juiceSettings.buttonEase)
            .SetUpdate(true);
    }

    // ===========================================
    // PANEL MANAGEMENT (TODO)
    // ===========================================

    /// <summary>
    /// Show a panel with juice animation
    /// </summary>
    public void ShowPanel(RectTransform panel)
    {
        if (juiceSettings == null || panel == null) return;

        // TODO: Implement panel show animation
        // - Start from panelStartScale
        // - Animate to scale 1
        // - Use panelOpenEase
        // - Duration: panelOpenDuration

        if (juiceSettings.debugMode) Debug.Log($"📂 Show panel: {panel.name}");
    }

    /// <summary>
    /// Hide a panel with juice animation
    /// </summary>
    public void HidePanel(RectTransform panel)
    {
        if (juiceSettings == null || panel == null) return;

        // TODO: Implement panel hide animation
        // - Animate to panelStartScale
        // - Use panelCloseEase
        // - Duration: panelCloseDuration
        // - Deactivate on complete

        if (juiceSettings.debugMode) Debug.Log($"📁 Hide panel: {panel.name}");
    }

    // ===========================================
    // SCREEN EFFECTS (TODO)
    // ===========================================

    /// <summary>
    /// Shake the camera/canvas
    /// </summary>
    public void ScreenShake(float intensityMultiplier = 1f)
    {
        if (juiceSettings == null || !juiceSettings.screenShakeIntensity.Equals(0)) return;

        // TODO: Implement screen shake
        // - Use mainCanvas transform or Camera
        // - Intensity: screenShakeIntensity * intensityMultiplier
        // - Duration: screenShakeDuration

        if (juiceSettings.debugMode) Debug.Log($"📳 Screen shake: {intensityMultiplier}x");
    }

    /// <summary>
    /// Flash the screen
    /// </summary>
    public void ScreenFlash(Color? flashColor = null)
    {
        if (juiceSettings == null || !juiceSettings.screenFlashEnabled) return;

        // TODO: Implement screen flash
        // - Create fullscreen overlay
        // - Fade in/out
        // - Duration: screenFlashDuration

        if (juiceSettings.debugMode) Debug.Log($"⚡ Screen flash");
    }

    // ===========================================
    // NUMBER ANIMATIONS (TODO)
    // ===========================================

    /// <summary>
    /// Animate a number counting up/down
    /// </summary>
    public void AnimateNumber(TMPro.TextMeshProUGUI textField, int from, int to)
    {
        if (juiceSettings == null || textField == null) return;

        // TODO: Use DOTween to count from → to
        // - Duration: numberCountDuration
        // - Ease: numberCountEase
        // - Update text every frame

        if (juiceSettings.debugMode) Debug.Log($"🔢 Animate number: {from} → {to}");
    }

    // ===========================================
    // REFRESH UI (TODO)
    // ===========================================

    /// <summary>
    /// Refresh all UI displays
    /// Called when game state changes
    /// </summary>
    public void RefreshAllUI()
    {
        // TODO: Update all UI elements based on GameManager state
        // - Money display
        // - Fans display
        // - Character portraits
        // - Stat bars
        // - Current quarter/year

        if (debugLogs) Debug.Log("🔄 Refreshing all UI");
    }

    // ===========================================
    // EVENT DISPLAY (TODO)
    // ===========================================

    /// <summary>
    /// Show an event popup
    /// </summary>
    public void ShowEvent(EventData eventData)
    {
        // TODO: Display event popup
        // - Show event image
        // - Show event text
        // - Show choice buttons
        // - Animate in with juice

        if (debugLogs) Debug.Log($"📰 Show event: {eventData?.name}");
    }

    // ===========================================
    // UTILITY
    // ===========================================

    /// <summary>
    /// Quick access to juice settings from anywhere
    /// Usage: UIManager.Instance.Juice.buttonHoverDuration
    /// </summary>
    public UIJuiceData Juice => juiceSettings;
}