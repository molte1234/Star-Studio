using UnityEngine;

public class UIController_Setup : MonoBehaviour
{
    // Why: This script lives in the Setup scene
    // All Setup scene buttons reference this GameObject

    public void OnStartPlayingClicked()
    {
        // Why: Load the main game scene
        SceneLoader.Instance.LoadGame();
    }

    public void OnBackToMenuClicked()
    {
        // Why: Return to main menu
        SceneLoader.Instance.LoadMainMenu();
    }
}