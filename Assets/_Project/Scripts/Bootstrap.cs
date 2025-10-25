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
    public string setupScene = "Setup";
    public string gameScene = "Game";

    private string activeSceneName;

    private void Awake()
    {
        // Why: Create persistent managers first
        CreatePersistentManagers();

        // Why: Load missing scenes and set up active scene
        StartCoroutine(LoadMissingScenesAndSetupActive());
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

    private IEnumerator LoadMissingScenesAndSetupActive()
    {
        // Why: Detect which scene should be active
        activeSceneName = DetermineActiveScene();
        Debug.Log($"🎬 Bootstrap: Active scene will be '{activeSceneName}'");

        // Why: Load scenes one by one, deactivating them immediately if they're not the active scene
        yield return LoadAndSetupScene(mainMenuScene);
        yield return LoadAndSetupScene(setupScene);
        yield return LoadAndSetupScene(gameScene);

        Debug.Log($"✅ Bootstrap: Setup complete! '{activeSceneName}' is active");
    }

    private string DetermineActiveScene()
    {
        // Why: Check what scenes are already loaded to determine which one you're working on
        bool mainMenuLoaded = SceneManager.GetSceneByName(mainMenuScene).isLoaded;
        bool setupLoaded = SceneManager.GetSceneByName(setupScene).isLoaded;
        bool gameLoaded = SceneManager.GetSceneByName(gameScene).isLoaded;

        // Why: Return the first game scene that's already loaded (priority: Setup > Game > MainMenu)
        if (setupLoaded)
        {
            Debug.Log("🔧 Editor Mode: Setup scene already open");
            return setupScene;
        }
        if (gameLoaded)
        {
            Debug.Log("🔧 Editor Mode: Game scene already open");
            return gameScene;
        }
        if (mainMenuLoaded)
        {
            Debug.Log("🔧 Editor Mode: MainMenu scene already open");
            return mainMenuScene;
        }

        // Why: Default to MainMenu if none are loaded yet
        return mainMenuScene;
    }

    private IEnumerator LoadAndSetupScene(string sceneName)
    {
        // Why: Check if scene is already loaded
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (scene.isLoaded)
        {
            Debug.Log($"⏩ Scene '{sceneName}' already loaded");

            // Why: Still need to deactivate it if it's not the active scene
            if (sceneName != activeSceneName)
            {
                DeactivateScene(sceneName);
            }

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

        // Why: Immediately deactivate it if it's not the active scene
        if (sceneName != activeSceneName)
        {
            DeactivateScene(sceneName);
        }
        else
        {
            Debug.Log($"🟢 '{sceneName}' is the active scene - keeping it visible");
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