using UnityEngine;

/// <summary>
/// DEBUG HELPER: Auto-populates band with test characters when Game scene is loaded directly
/// Attach this to GameManager in Bootstrap scene
/// Only works if no band exists (characterStates are all null)
/// </summary>
public class TestBandHelper : MonoBehaviour
{
    [Header("Test Band Settings")]
    [Tooltip("Enable this to auto-fill band with test characters when Game scene loads with empty band")]
    public bool useTestBand = true;

    [Tooltip("Drag up to 6 different characters here - empty slots will be left empty")]
    public SlotData[] testCharacters = new SlotData[6];

    [Header("Test Band Name")]
    [Tooltip("Band name to use for test band")]
    public string testBandName = "Test Band";

    /// <summary>
    /// Call this from Start() when this component is in the Game scene
    /// </summary>
    void Start()
    {
        // Why: Auto-check when Game scene loads
        CheckAndCreateTestBand();
    }

    /// <summary>
    /// Call this from GameManager.Start() or whenever Game scene loads
    /// </summary>
    public void CheckAndCreateTestBand()
    {
        // Why: Only create test band if feature is enabled
        if (!useTestBand)
        {
            Debug.Log("🔧 TestBandHelper: useTestBand is disabled");
            return;
        }

        // Why: Safety check - make sure at least one character is assigned
        bool hasAnyCharacter = false;
        for (int i = 0; i < testCharacters.Length; i++)
        {
            if (testCharacters[i] != null)
            {
                hasAnyCharacter = true;
                break;
            }
        }

        if (!hasAnyCharacter)
        {
            Debug.LogWarning("⚠️ TestBandHelper: No test characters assigned! Drag SlotData ScriptableObjects into the testCharacters array.");
            return;
        }

        // Why: Only create test band if no band exists
        if (HasExistingBand())
        {
            Debug.Log("🔧 TestBandHelper: Band already exists, skipping test band creation");
            return;
        }

        // Why: Create test band
        CreateTestBand();
    }

    private bool HasExistingBand()
    {
        // Why: Check if any character slots are filled
        GameManager gm = GameManager.Instance;
        if (gm == null) return false;

        for (int i = 0; i < gm.characterStates.Length; i++)
        {
            if (gm.characterStates[i] != null && gm.characterStates[i].slotData != null)
            {
                return true; // Found at least one character
            }
        }

        return false; // No characters found
    }

    private void CreateTestBand()
    {
        Debug.Log("========================================");
        Debug.Log("🔧 TEST BAND HELPER: Creating test band");

        int characterCount = 0;
        for (int i = 0; i < testCharacters.Length; i++)
        {
            if (testCharacters[i] != null)
            {
                characterCount++;
                Debug.Log($"   Slot {i}: {testCharacters[i].displayName}");
            }
        }

        Debug.Log($"   Total characters: {characterCount}");
        Debug.Log($"   Band Name: {testBandName}");
        Debug.Log("========================================");

        GameManager gm = GameManager.Instance;

        // Why: Use the testCharacters array directly AND pass testBandName
        gm.SetupNewGame(testCharacters, testBandName);

        Debug.Log("✅ Test band created successfully!");

        // Why: Force UI refresh to show characters
        if (gm.uiController != null)
        {
            gm.uiController.RefreshUI();
        }
    }
}