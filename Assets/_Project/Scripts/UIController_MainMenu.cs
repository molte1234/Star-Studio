using UnityEngine;

public class UIController_MainMenu : MonoBehaviour
{
    // Why: This script lives in the MainMenu scene
    // All MainMenu buttons reference this GameObject

    public void OnStartGameClicked()
    {
        // Why: Load the band setup scene
        SceneLoader.Instance.LoadSetup();
    }

    public void OnSettingsClicked()
    {
        // Why: Open settings panel (implement later)
        Debug.Log("Settings clicked - not yet implemented");
    }

    public void OnQuitClicked()
    {
        // Why: Quit the game
        Debug.Log("Quit clicked");
        Application.Quit();

#if UNITY_EDITOR
        // Why: In editor, stop play mode instead
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}