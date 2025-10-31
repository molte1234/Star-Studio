using UnityEngine;

/// <summary>
/// DEBUG HELPER: Populates band with test characters
/// Attach this to GameManager in Bootstrap scene
/// UIController_Game calls PopulateTestBandIfEnabled() before initializing displays
/// </summary>
public class TestBandHelper : MonoBehaviour
{
    [Header("Test Band Settings")]
    [Tooltip("Enable this to auto-fill band with test characters")]
    public bool useTestBand = true;

    [Tooltip("Drag up to 6 different characters here - empty slots will be left empty")]
    public SlotData[] testCharacters = new SlotData[6];

    [Header("Test Band Name")]
    [Tooltip("Band name to use for test band")]
    public string testBandName = "Test Band";

    /// <summary>
    /// Called by UIController_Game to populate test band if enabled
    /// </summary>
    public void PopulateTestBandIfEnabled()
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
    }
}