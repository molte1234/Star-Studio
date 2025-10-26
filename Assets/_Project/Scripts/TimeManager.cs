using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Singleton TimeManager - handles automatic quarter advancement
/// Counts down in real-time, advances quarter when timer expires
/// Can be paused by events or player
/// </summary>
public class TimeManager : MonoBehaviour
{
    // Singleton pattern
    public static TimeManager Instance;

    [Header("State")]
    public bool isPaused = false;

    // Private state
    private float currentTime = 0f;
    private float quarterDuration; // Loaded from GameRules

    void Awake()
    {
        // Why: Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Why: Load quarter duration from GameRules
        if (GameManager.Instance?.rules != null)
        {
            quarterDuration = GameManager.Instance.rules.quarterDuration;
            Debug.Log($"⏰ TimeManager initialized - Quarter duration: {quarterDuration}s");
        }
        else
        {
            Debug.LogWarning("⚠️ GameRules not found! Using default 30 seconds.");
            quarterDuration = 30f;
        }

        // Why: Start the timer at 0
        currentTime = 0f;
    }

    void Update()
    {
        // Why: If paused, don't count time
        if (isPaused) return;

        // Why: Count up time
        currentTime += Time.deltaTime;

        // Why: When timer reaches duration, advance quarter
        if (currentTime >= quarterDuration)
        {
            AdvanceQuarter();
        }
    }

    /// <summary>
    /// Called when quarter timer expires
    /// Advances game state and resets timer
    /// </summary>
    private void AdvanceQuarter()
    {
        // Why: Reset timer for next quarter
        currentTime = 0f;

        // Why: Tell GameManager to advance quarter
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AdvanceQuarter();
        }
        else
        {
            Debug.LogError("❌ GameManager.Instance is null - cannot advance quarter!");
        }
    }

    /// <summary>
    /// Pauses time progression
    /// Called by EventManager when showing events or player clicks pause button
    /// </summary>
    public void PauseTime()
    {
        isPaused = true;

        // Why: Play pause sound (slow down effect)
        if (GameManager.Instance?.audioManager != null)
        {
            GameManager.Instance.audioManager.PlayPause();
        }

        Debug.Log("⏸️ Time PAUSED");
    }

    /// <summary>
    /// Resumes time progression
    /// Called by EventManager when hiding events or player unpauses
    /// </summary>
    public void ResumeTime()
    {
        isPaused = false;

        // Why: Play unpause sound (speed back up effect)
        if (GameManager.Instance?.audioManager != null)
        {
            GameManager.Instance.audioManager.PlayUnpause();
        }

        Debug.Log("▶️ Time RESUMED");
    }

    /// <summary>
    /// Toggles between pause and resume
    /// Used for simple pause button (not toggle)
    /// </summary>
    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeTime();
        }
        else
        {
            PauseTime();
        }
    }

    /// <summary>
    /// Returns current timer progress (0-1)
    /// Useful for debugging or additional UI
    /// </summary>
    public float GetProgress()
    {
        return currentTime / quarterDuration;
    }

    /// <summary>
    /// Returns time remaining in current quarter
    /// Useful for displaying countdown timer
    /// </summary>
    public float GetTimeRemaining()
    {
        return quarterDuration - currentTime;
    }

    /// <summary>
    /// Returns fill amount for circular UI (0.0 to 1.0)
    /// Used by CircularTimer component
    /// </summary>
    public float GetFillAmount()
    {
        return Mathf.Clamp01(currentTime / quarterDuration);
    }
}