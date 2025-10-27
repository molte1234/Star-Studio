using UnityEngine;
using TMPro;
using DG.Tweening;

/// <summary>
/// UI Controller for the Band Setup scene
/// Handles button clicks and JUICY money display with DOTween + Unity Particles
/// </summary>
public class UIController_Setup : MonoBehaviour
{
    [Header("Manager Reference")]
    public BandSetupManager bandSetupManager;

    [Header("Money Display")]
    [Tooltip("Shows current money - animates smoothly to new values")]
    public TextMeshProUGUI moneyText;

    [Header("Money Juice (DOTween + Particles)")]
    [Tooltip("Particle system that plays when money changes (optional)")]
    public ParticleSystem moneyChangeParticles;

    [Tooltip("How fast money counts up/down (higher = faster)")]
    [Range(0.1f, 2f)]
    public float moneyLerpSpeed = 0.5f;

    [Tooltip("Should the money text scale punch when changing?")]
    public bool usePunchScale = true;

    [Tooltip("How much to punch scale (0.1 = subtle, 0.3 = dramatic)")]
    [Range(0.05f, 0.5f)]
    public float punchAmount = 0.15f;

    // Why: Track displayed money for smooth lerping
    private int displayedMoney = 0;
    private int targetMoney = 0;
    private Tween moneyTween;

    void Start()
    {
        // Why: Auto-find BandSetupManager if not assigned
        if (bandSetupManager == null)
        {
            bandSetupManager = FindObjectOfType<BandSetupManager>();
        }

        // Why: Initialize money display
        if (GameManager.Instance != null)
        {
            displayedMoney = GameManager.Instance.money;
            targetMoney = GameManager.Instance.money;
            UpdateMoneyText(displayedMoney);
        }
    }

    void Update()
    {
        // Why: Check if money changed in GameManager
        if (GameManager.Instance != null && GameManager.Instance.money != targetMoney)
        {
            // Money changed! Trigger juice effects
            OnMoneyChanged(GameManager.Instance.money);
        }
    }

    /// <summary>
    /// Called when money value changes - triggers all the juice!
    /// </summary>
    private void OnMoneyChanged(int newAmount)
    {
        targetMoney = newAmount;

        // Why: Cancel any existing tween to prevent conflicts
        if (moneyTween != null && moneyTween.IsActive())
        {
            moneyTween.Kill();
        }

        // Why: Play sparkle particles (if assigned)
        if (moneyChangeParticles != null)
        {
            moneyChangeParticles.Play();
        }

        // Why: Punch scale for extra juice (DOTween)
        if (usePunchScale && moneyText != null)
        {
            moneyText.transform.DOPunchScale(Vector3.one * punchAmount, 0.3f, 5, 0.5f);
        }

        // Why: Smoothly lerp/count to new value (DOTween)
        moneyTween = DOVirtual.Int(
            displayedMoney,           // From current displayed value
            targetMoney,              // To new target value
            moneyLerpSpeed,           // Duration
            (value) =>                // On update callback
            {
                displayedMoney = value;
                UpdateMoneyText(value);
            }
        ).SetEase(Ease.OutQuad);      // Smooth deceleration

        Debug.Log($"💰 Money changed: {displayedMoney} → {targetMoney}");
    }

    /// <summary>
    /// Updates the money text display
    /// </summary>
    private void UpdateMoneyText(int amount)
    {
        if (moneyText != null)
        {
            moneyText.text = $"${amount}";
        }
    }

    /// <summary>
    /// Public method to force refresh (optional, for manual calls)
    /// </summary>
    public void RefreshMoneyDisplay()
    {
        if (GameManager.Instance != null)
        {
            OnMoneyChanged(GameManager.Instance.money);
        }
    }

    // ============================================
    // BUTTON HANDLERS
    // ============================================

    /// <summary>
    /// NEXT button - browse to next character
    /// </summary>
    public void OnNextCharacterClicked()
    {
        bandSetupManager.ShowNextCharacter();
    }

    /// <summary>
    /// PREV button - browse to previous character
    /// </summary>
    public void OnPreviousCharacterClicked()
    {
        bandSetupManager.ShowPreviousCharacter();
    }

    /// <summary>
    /// ADD button - add current character to band
    /// </summary>
    public void OnAddCharacterClicked()
    {
        bandSetupManager.AddCurrentCharacterToBand();
    }

    /// <summary>
    /// READY/START button - begin the game
    /// </summary>
    public void OnStartPlayingClicked()
    {
        bandSetupManager.StartGame();
    }

    /// <summary>
    /// Back button - return to main menu
    /// </summary>
    public void OnBackToMenuClicked()
    {
        SceneLoader.Instance.LoadMainMenu();
    }

    void OnDestroy()
    {
        // Why: Clean up tweens to prevent memory leaks
        if (moneyTween != null && moneyTween.IsActive())
        {
            moneyTween.Kill();
        }
    }
}