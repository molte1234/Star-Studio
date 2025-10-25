using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    [Header("Persistent Managers (Optional)")]
    public GameObject gameManagerPrefab; // Optional: use prefab for GameManager
    public GameObject audioManagerPrefab; // Drag AudioManager prefab here after creating it

    void Awake()
    {
        // Why: Create managers in Bootstrap scene (they stay alive forever)
        CreateManagers();

        // Why: Start menu music (if AudioManager has it assigned)
        StartMenuMusic();

        // Why: Load MainMenu scene ADDITIVELY (Bootstrap stays loaded)
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Additive);
    }

    private void CreateManagers()
    {
        // Why: Check if GameManager already exists (prevents duplicates)
        if (GameManager.Instance != null)
        {
            return; // Already exists
        }

        // Why: No need for DontDestroyOnLoad - Bootstrap scene never unloads!

        // ===== GameManager =====
        GameObject gmObject = new GameObject("GameManager");
        gmObject.AddComponent<GameManager>();
        gmObject.AddComponent<EventManager>();

        // ===== AudioManager =====
        if (audioManagerPrefab != null)
        {
            Instantiate(audioManagerPrefab);
            Debug.Log("✅ Bootstrap: Created AudioManager from prefab");
        }
        else
        {
            GameObject audioObject = new GameObject("AudioManager");
            audioObject.AddComponent<AudioManager>();
            Debug.Log("⚠️ Bootstrap: Created empty AudioManager (no prefab assigned)");
        }

        Debug.Log("✅ Bootstrap: All managers created in Bootstrap scene");
    }

    private void StartMenuMusic()
    {
        // Why: Play menu music if AudioManager has it assigned
        if (AudioManager.Instance != null && AudioManager.Instance.menuMusic != null)
        {
            AudioManager.Instance.PlayMusic(AudioManager.Instance.menuMusic);
        }
    }
}