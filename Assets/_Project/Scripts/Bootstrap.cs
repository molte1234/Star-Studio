using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    [Header("Persistent Managers")]
    public GameObject gameManagerPrefab; // Optional: use prefab

    void Awake()
    {
        // Why: Create persistent GameManager that lives forever
        CreateGameManager();

        // Why: Immediately load the band setup scene
        SceneManager.LoadScene("BandSetupScene");
    }

    private void CreateGameManager()
    {
        // Why: Check if GameManager already exists (prevents duplicates)
        if (GameManager.Instance != null)
        {
            return; // Already exists from previous run
        }

        // Why: Create the persistent manager objects
        GameObject managers = new GameObject("--- PERSISTENT MANAGERS ---");

        // Add GameManager
        GameObject gmObject = new GameObject("GameManager");
        gmObject.transform.SetParent(managers.transform);
        gmObject.AddComponent<GameManager>();

        // Add EventManager to same object
        gmObject.AddComponent<EventManager>();

        DontDestroyOnLoad(managers);

        Debug.Log("✅ Bootstrap: Created persistent managers");
    }
}
