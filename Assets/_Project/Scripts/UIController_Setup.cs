using UnityEngine;

public class UIController_Setup : MonoBehaviour
{
    // Why: This script lives in the Setup scene
    // All Setup scene buttons reference this GameObject

    [Header("Manager Reference")]
    public BandSetupManager bandSetupManager;

    void Start()
    {
        // Why: Auto-find BandSetupManager if not assigned
        if (bandSetupManager == null)
        {
            bandSetupManager = FindObjectOfType<BandSetupManager>();
        }
    }

    // Why: NEXT button - browse to next character
    public void OnNextCharacterClicked()
    {
        bandSetupManager.ShowNextCharacter();
    }

    // Why: PREV button - browse to previous character
    public void OnPreviousCharacterClicked()
    {
        bandSetupManager.ShowPreviousCharacter();
    }

    // Why: ADD button - add current character to band
    public void OnAddCharacterClicked()
    {
        bandSetupManager.AddCurrentCharacterToBand();
    }

    // Why: READY/START button - begin the game
    public void OnStartPlayingClicked()
    {
        bandSetupManager.StartGame();
    }

    // Why: Back button - return to main menu
    public void OnBackToMenuClicked()
    {
        SceneLoader.Instance.LoadMainMenu();
    }
}