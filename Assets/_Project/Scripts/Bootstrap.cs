using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Bootstrap : MonoBehaviour
{
    [Header("Scene Names")]
    public string mainMenuScene = "MainMenu";
    public string setupScene = "Setup";
    public string gameScene = "Game";

    private string activeSceneName;

    private void Awake()
    {
        // Why: Determine which scene should be active BEFORE loading anything
        activeSceneName = DetermineActiveScene();
        Debug.Log($"🎬 Bootstrap: Active scene is '{activeSceneName}'");

        // Why: Load missing scenes
        StartCoroutine(LoadMissingScenes());
    }

    private string DetermineActiveScene()
    {
        // Why: Check what scenes are already loaded
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            // Skip Bootstrap scene
            if (scene.name == "Bootstrap") continue;

            // Check if it's one of our game scenes
            if (scene.name == setupScene)
            {
                Debug.Log("🔧 Editor Mode: Setup scene detected as active");
                return setupScene;
            }
            if (scene.name == gameScene)
            {
                Debug.Log("🔧 Editor Mode: Game scene detected as active");
                return gameScene;
            }
            if (scene.name == mainMenuScene)
            {
                Debug.Log("🔧 Editor Mode: MainMenu scene detected as active");
                return mainMenuScene;
            }
        }

        // Why: Default to MainMenu if none detected
        Debug.Log("🔧 No game scene detected, defaulting to MainMenu");
        return mainMenuScene;
    }

    private IEnumerator LoadMissingScenes()
    {
        // Why: Load each scene only if it's NOT already loaded
        yield return LoadSceneIfMissing(mainMenuScene);
        yield return LoadSceneIfMissing(setupScene);
        yield return LoadSceneIfMissing(gameScene);

        // Why: Now deactivate all scenes except the active one
        DeactivateInactiveScenes();

        // Why: Tell AudioManager to play music for the active scene
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.OnSceneActivated(activeSceneName);
        }

        Debug.Log($"✅ Bootstrap: Setup complete! '{activeSceneName}' is visible");
    }

    private IEnumerator LoadSceneIfMissing(string sceneName)
    {
        // Why: Check if scene is already loaded
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (scene.isLoaded)
        {
            Debug.Log($"⏩ Scene '{sceneName}' already loaded, skipping");
            yield break;
        }

        // Why: Load scene additively if not already loaded
        Debug.Log($"📥 Loading scene '{sceneName}'...");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        Debug.Log($"✅ Scene '{sceneName}' loaded");
    }

    private void DeactivateInactiveScenes()
    {
        // Why: Deactivate all scenes that aren't the active one
        if (mainMenuScene != activeSceneName)
        {
            DeactivateScene(mainMenuScene);
        }
        if (setupScene != activeSceneName)
        {
            DeactivateScene(setupScene);
        }
        if (gameScene != activeSceneName)
        {
            DeactivateScene(gameScene);
        }
    }

    private void DeactivateScene(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.isLoaded) return;

        // Why: Disable all root objects in the scene
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            rootObject.SetActive(false);
        }

        Debug.Log($"⚫ Deactivated scene: {sceneName}");
    }
}