using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Why: Singleton so any script can call SceneLoader.Instance.LoadScene()
    public static SceneLoader Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(string sceneName)
    {
        // Why: Since all scenes are already loaded, we just activate/deactivate
        Debug.Log($"🔄 Switching to scene: {sceneName}");

        // Deactivate all game scenes
        DeactivateAllScenes();

        // Activate target scene
        ActivateScene(sceneName);
    }

    private void DeactivateAllScenes()
    {
        // Why: Turn off all game scenes except Bootstrap
        DeactivateScene("MainMenu");
        DeactivateScene("Setup");
        DeactivateScene("Game");
    }

    private void ActivateScene(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.isLoaded)
        {
            Debug.LogError($"❌ Scene '{sceneName}' is not loaded!");
            return;
        }

        // Why: Enable all root objects
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            rootObject.SetActive(true);
        }

        Debug.Log($"🟢 Activated: {sceneName}");
    }

    private void DeactivateScene(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.isLoaded) return;

        // Why: Disable all root objects
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            rootObject.SetActive(false);
        }
    }

    // Why: Convenience methods for buttons and code
    public void LoadMainMenu() => LoadScene("MainMenu");
    public void LoadSetup() => LoadScene("Setup");
    public void LoadGame() => LoadScene("Game");
}