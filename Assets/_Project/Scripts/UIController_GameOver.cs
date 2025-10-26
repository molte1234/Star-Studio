using UnityEngine;
using TMPro;

/// <summary>
/// Controls the Game Over screen
/// Shows simple end message and allows player to return to main menu
/// </summary>
public class UIController_GameOver : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;

    private void Start()
    {
        // Why: Display end game message
        if (titleText != null)
        {
            titleText.text = "GAME OVER";
        }

        if (messageText != null)
        {
            messageText.text = "You managed your band for 10 years!";
        }

        Debug.Log("🎊 GameOver screen loaded");
    }

    /// <summary>
    /// Called by "Back to Menu" button
    /// Resets game state and returns to main menu
    /// </summary>
    public void BackToMenu()
    {
        Debug.Log("🔄 Resetting game and returning to main menu...");

        // Why: Reset all game state in GameManager
        ResetGameState();

        // Why: Load main menu scene
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadMainMenu();
        }
        else
        {
            Debug.LogError("❌ SceneLoader.Instance is null!");
        }
    }

    private void ResetGameState()
    {
        // Why: Reset GameManager to fresh state
        if (GameManager.Instance == null)
        {
            Debug.LogError("❌ GameManager.Instance is null!");
            return;
        }

        GameManager gm = GameManager.Instance;

        // Reset time
        gm.currentQuarter = 0;
        gm.currentYear = 1;

        // Reset resources
        gm.money = 500;
        gm.fans = 50;

        // Reset stats
        gm.technical = 0;
        gm.performance = 0;
        gm.charisma = 0;
        gm.unity = 100;

        // Clear band slots
        for (int i = 0; i < gm.slots.Length; i++)
        {
            gm.slots[i] = null;
        }

        // Clear flags
        gm.flags.Clear();

        // Reset band name
        gm.bandName = "";

        // Mark as new game for welcome screen
        gm.isNewGame = true;

        // Reset event history
        if (gm.eventManager != null)
        {
            gm.eventManager.ResetTriggeredEvents();
        }

        Debug.Log("✅ Game state reset complete");
    }
}