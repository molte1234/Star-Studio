using UnityEngine;
using DG.Tweening;

/// <summary>
/// Central configuration for ALL UI juice/feedback in the game
/// Create asset: Right-click → Create → Band Manager → UI Juice Data
/// Reference from UIManager to provide global access
/// </summary>
[CreateAssetMenu(fileName = "UIJuiceData", menuName = "Band Manager/UI Juice Data")]
public class UIJuiceData : ScriptableObject
{
    [Header("=== BUTTON FEEDBACK ===")]
    [Tooltip("How long button hover scale animation takes")]
    public float buttonHoverDuration = 0.2f;

    [Tooltip("Scale multiplier on hover (1.1 = 110%)")]
    public float buttonHoverScale = 1.1f;

    [Tooltip("How long button press squash takes")]
    public float buttonPressDuration = 0.1f;

    [Tooltip("Scale multiplier on press (0.95 = 95%)")]
    public float buttonPressScale = 0.95f;

    [Tooltip("Easing curve for button animations")]
    public Ease buttonEase = Ease.OutBack;

    [Header("=== PANEL TRANSITIONS ===")]
    [Tooltip("How long panels take to open")]
    public float panelOpenDuration = 0.3f;

    [Tooltip("How long panels take to close")]
    public float panelCloseDuration = 0.2f;

    [Tooltip("Start scale for panels (0 = collapsed)")]
    public float panelStartScale = 0.8f;

    [Tooltip("Easing curve for panel open")]
    public Ease panelOpenEase = Ease.OutBack;

    [Tooltip("Easing curve for panel close")]
    public Ease panelCloseEase = Ease.InBack;

    [Header("=== CHARACTER PORTRAITS ===")]
    [Tooltip("Should portraits have idle breathing animation?")]
    public bool portraitBreathingEnabled = true;

    [Tooltip("How much portraits scale during breathing (1.02 = 102%)")]
    public float portraitBreathingScale = 1.02f;

    [Tooltip("How long one breath cycle takes")]
    public float portraitBreathingDuration = 2f;

    [Tooltip("Should portraits react on hover?")]
    public bool portraitHoverEnabled = true;

    [Tooltip("Portrait hover scale multiplier")]
    public float portraitHoverScale = 1.05f;

    [Tooltip("Duration of portrait hover animation")]
    public float portraitHoverDuration = 0.15f;

    [Header("=== STAT CHANGES ===")]
    [Tooltip("Duration of stat bar fill animation")]
    public float statBarFillDuration = 0.5f;

    [Tooltip("Should stat changes spawn number popups?")]
    public bool statNumberPopupsEnabled = true;

    [Tooltip("Should stat changes spawn particles?")]
    public bool statParticlesEnabled = true;

    [Tooltip("Stat bar animation easing")]
    public Ease statBarEase = Ease.OutQuad;

    [Header("=== PARTICLES ===")]
    [Tooltip("Global particle spawn rate multiplier (1 = normal, 2 = double)")]
    public float particleSpawnRateMultiplier = 1f;

    [Tooltip("Should particles spawn on button clicks?")]
    public bool buttonClickParticles = true;

    [Tooltip("Should particles spawn on money gain?")]
    public bool moneyGainParticles = true;

    [Tooltip("Should particles spawn on action complete?")]
    public bool actionCompleteParticles = true;

    [Header("=== SCREEN EFFECTS ===")]
    [Tooltip("Base intensity for screen shake (0 = off)")]
    public float screenShakeIntensity = 0.3f;

    [Tooltip("Duration of screen shake")]
    public float screenShakeDuration = 0.3f;

    [Tooltip("Should screen flash on important events?")]
    public bool screenFlashEnabled = true;

    [Tooltip("Duration of screen flash")]
    public float screenFlashDuration = 0.2f;

    [Header("=== AUDIO ===")]
    [Tooltip("Should UI sounds play on interactions?")]
    public bool uiSoundsEnabled = true;

    [Tooltip("Master volume for UI sounds (0-1)")]
    [Range(0f, 1f)]
    public float uiSoundVolume = 0.7f;

    [Tooltip("Should buttons play hover sounds?")]
    public bool buttonHoverSounds = true;

    [Tooltip("Should buttons play click sounds?")]
    public bool buttonClickSounds = true;

    [Header("=== NUMBER ANIMATIONS ===")]
    [Tooltip("Duration for counting number animations (money, fans, stats)")]
    public float numberCountDuration = 0.8f;

    [Tooltip("Easing for number counting")]
    public Ease numberCountEase = Ease.OutQuad;

    [Header("=== ADVANCED ===")]
    [Tooltip("Global juice intensity multiplier - adjust ALL timings at once")]
    [Range(0f, 2f)]
    public float globalJuiceMultiplier = 1f;

    [Tooltip("Debug mode - logs all juice events to console")]
    public bool debugMode = false;

    // Helper method to apply global multiplier to any duration
    public float ApplyGlobalMultiplier(float baseDuration)
    {
        return baseDuration * globalJuiceMultiplier;
    }
}