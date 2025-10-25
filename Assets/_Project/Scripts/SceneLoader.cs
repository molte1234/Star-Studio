using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private static string currentSceneName = "MainMenu"; // Track current active scene

    public static void LoadScene(string sceneName)
    {
        // Why: Unload previous scene, load new scene additively
        if (!string.IsNullOrEmpty(currentSceneName) && currentSceneName != "Bootstrap")
        {
            SceneManager.UnloadSceneAsync(currentSceneName);
        }

        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        currentSceneName = sceneName;

        // Why: Play appropriate music for each scene
        PlaySceneMusic(sceneName);

        Debug.Log($"🎬 Loaded scene: {sceneName}");
    }

    private static void PlaySceneMusic(string sceneName)
    {
        // Why: Change music based on which scene we loaded
        if (AudioManager.Instance == null) return;

        switch (sceneName)
        {
            case "MainMenu":
                if (AudioManager.Instance.menuMusic != null)
                    AudioManager.Instance.PlayMusic(AudioManager.Instance.menuMusic);
                break;

            case "GameSetup":
                if (AudioManager.Instance.setupMusic != null)
                    AudioManager.Instance.PlayMusic(AudioManager.Instance.setupMusic);
                break;

            case "MainGame":
                if (AudioManager.Instance.gameMusic != null)
                    AudioManager.Instance.PlayMusic(AudioManager.Instance.gameMusic);
                break;
        }
    }

    public static void QuitGame()
    {
        Debug.Log("👋 Quitting game...");
        Application.Quit();
    }
}