using UnityEngine;

public class UIController_Game : MonoBehaviour
{
    // Why: This script lives in the Game scene
    // All Game scene buttons and actions reference this GameObject

    // Why: Game action buttons
    public void OnRecordClicked()
    {
        GameManager.Instance.DoAction(ActionType.Record);
    }

    public void OnTourClicked()
    {
        GameManager.Instance.DoAction(ActionType.Tour);
    }

    public void OnPracticeClicked()
    {
        GameManager.Instance.DoAction(ActionType.Practice);
    }

    public void OnRestClicked()
    {
        GameManager.Instance.DoAction(ActionType.Rest);
    }

    public void OnReleaseClicked()
    {
        GameManager.Instance.DoAction(ActionType.Release);
    }

    public void OnPauseClicked()
    {
        // Why: Open pause menu (implement later)
        Debug.Log("Pause clicked - not yet implemented");
    }

    public void OnBackToMenuClicked()
    {
        // Why: Return to main menu
        SceneLoader.Instance.LoadMainMenu();
    }
}