using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Bootstrap : MonoBehaviour
{
    [Header("Game Rules")]
    public GameRules gameRules;

    [Header("Event Database")]
    public EventData[] allEvents;

    [Header("Scene Names")]
    public string mainMenuScene = "MainMenu";
    public string gameSetupScene = "GameSetup";
    public string mainGameScene = "MainGame";

    private void Awake()
    {
        // Why: Create persistent managers first
        CreatePersistentManagers();

        // Why: Load all scenes upfront
        StartCoroutine(LoadAllScenesUpfront());
    }

    private void CreatePersistentManagers()
    {
        // Why: Check if GameManager already exists (prevents duplicates)
        if (GameManager.Instance != null)
        {
            Debug.Log("⚠️ Bootstrap: Managers already exist, skipping creation");
            return;
        }

        // Why: Create the persistent manager container
        GameObject managers = new GameObject("--- PERSISTENT MANAGERS ---");
        managers.transform.SetParent(transform); // Child of Bootstrap

        // Create GameManager
        GameObject gmObject = new GameObject("GameManager");
        gmObject.transform.SetParent(managers.transform);
        GameManager gm = gmObject.AddComponent<GameManager>();

        // Create EventManager
        EventManager em = gmObject.AddComponent<EventManager>();

        // Create AudioManager
        GameObject audioObject = new GameObject("AudioManager");
        audioObject.transform.SetParent(managers.transform);
        AudioManager am = audioObject.AddComponent<AudioManager>();

        // Why: Connect all references
        gm.rules = gameRules;
        gm.eventManager = em;
        gm.audioManager = am;
        em.allEvents = new System.Collections.Generic.List<EventData>(allEvents);

        Debug.Log("✅ Bootstrap: Created persistent managers");
        Debug.Log($"📋 Bootstrap: Loaded {allEvents.Length} events");
    }

    private IEnumerator LoadAllScenesUpfront()
    {
        // Why: Detect what scene was already open in Editor
        string startingScene = DetermineStartingScene();

        Debug.Log($"🎬 Bootstrap: Starting scene is '{startingScene}'");

        // Why: Load all game scenes additively
        yield return LoadSceneIfNotLoaded(mainMenuScene);
        yield return LoadSceneIfNotLoaded(gameSetupScene);
        yield return LoadSceneIfNotLoaded(mainGameScene);

        // Why: Deactivate all scenes initially
        DeactivateScene(mainMenuScene);
        DeactivateScene(gameSetupScene);
        DeactivateScene(mainGameScene);

        // Why: Activate only the starting scene
        ActivateScene(startingScene);

        Debug.Log("✅ Bootstrap: All scenes loaded and ready!");
    }

    private string DetermineStartingScene()
    {
#if UNITY_EDITOR
        // Why: In Editor, check what scene was already open
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name == gameSetupScene)
            {
                Debug.Log("🔧 Editor Mode: Detected GameSetup scene already open");
                return gameSetupScene;
            }
            if (scene.name == mainGameScene)
            {
                Debug.Log("🔧 Editor Mode: Detected MainGame scene already open");
                return mainGameScene;
            }
        }
#endif
        // Why: Default to MainMenu (or in builds, always MainMenu)
        return mainMenuScene;
    }

    private IEnumerator LoadSceneIfNotLoaded(string sceneName)
    {
        // Why: Check if scene is already loaded
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (scene.isLoaded)
        {
            Debug.Log($"⏩ Scene '{sceneName}' already loaded, skipping");
            yield break;
        }

        // Why: Load scene additively
        Debug.Log($"📥 Loading scene '{sceneName}'...");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        Debug.Log($"✅ Scene '{sceneName}' loaded");
    }

    private void ActivateScene(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.isLoaded) return;

        // Why: Enable all root objects in the scene
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            rootObject.SetActive(true);
        }

        Debug.Log($"🟢 Activated scene: {sceneName}");
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