using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays character details in Band Setup scene
/// Shows: Portrait, name, stats (8-stat system), hire cost, trait, bio
/// </summary>
public class CharacterViewer : MonoBehaviour
{
    [Header("Character Info Display")]
    public Image portraitImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI realNameText;
    public TextMeshProUGUI traitText;
    public TextMeshProUGUI hireCostText;
    public TextMeshProUGUI upkeepCostText;
    public TextMeshProUGUI bioText;

    [Header("8-Stat System Bars (NEW)")]
    public HorizontalBar charismaBar;
    public HorizontalBar stagePerformanceBar;
    public HorizontalBar vocalBar;
    public HorizontalBar instrumentBar;
    public HorizontalBar songwritingBar;
    public HorizontalBar productionBar;
    public HorizontalBar managementBar;
    public HorizontalBar practicalBar;

    [Header("Visual Feedback")]
    public Image hiredStampImage; // Shows "HIRED" if already in band
    public bool animateBars = true; // Enable/disable bar animations

    /// <summary>
    /// Display a character's full details
    /// </summary>
    public void DisplayCharacter(SlotData character, bool isAlreadyInBand = false)
    {
        if (character == null)
        {
            Debug.LogWarning("⚠️ Tried to display null character!");
            ClearDisplay();
            return;
        }

        // Why: Show portrait
        if (portraitImage != null)
            portraitImage.sprite = character.sprite;

        // Why: Show display name (stage name)
        if (nameText != null)
            nameText.text = character.displayName;

        // Why: Show real name (if available)
        if (realNameText != null)
        {
            realNameText.text = !string.IsNullOrEmpty(character.realName)
                ? character.realName
                : "";  // Leave blank if no real name set
        }

        // Why: Show hire cost with $ prefix
        if (hireCostText != null)
        {
            hireCostText.text = character.hireCost > 0
                ? $"${character.hireCost}"
                : "$0";
        }

        // Why: Show upkeep cost with $ prefix
        if (upkeepCostText != null)
        {
            upkeepCostText.text = character.upkeepCost > 0
                ? $"${character.upkeepCost}"
                : "$0";
        }

        // Why: Show trait (one-liner personality)
        if (traitText != null)
            traitText.text = character.trait;

        // Why: Show biography/description
        if (bioText != null)
        {
            // Use the description field from SlotData for character backstory
            bioText.text = !string.IsNullOrEmpty(character.description)
                ? character.description
                : "No bio available.";
        }

        // ⭐ NEW UNIFIED API - All stats use SetProgress(0-1)
        // Convert 0-10 stat values to 0-1 progress
        UpdateStatBar(charismaBar, character.charisma, "Charisma");
        UpdateStatBar(stagePerformanceBar, character.stagePerformance, "Stage Performance");
        UpdateStatBar(vocalBar, character.vocal, "Vocal");
        UpdateStatBar(instrumentBar, character.instrument, "Instrument");
        UpdateStatBar(songwritingBar, character.songwriting, "Songwriting");
        UpdateStatBar(productionBar, character.production, "Production");
        UpdateStatBar(managementBar, character.management, "Management");
        UpdateStatBar(practicalBar, character.practical, "Practical");

        // Why: Show "Hired" stamp if character is already in the band
        if (hiredStampImage != null)
        {
            hiredStampImage.gameObject.SetActive(isAlreadyInBand);
        }

        Debug.Log($"🎭 CharacterViewer updated: {character.displayName}");
        Debug.Log($"   Real Name: {character.realName}");
        Debug.Log($"   Hire Cost: ${character.hireCost}");
        Debug.Log($"   Upkeep: ${character.upkeepCost}/quarter");
        Debug.Log($"   Trait: {character.trait}");
        Debug.Log($"   Bio: {(string.IsNullOrEmpty(character.description) ? "None" : "Present")}");
        Debug.Log($"   Hired: {isAlreadyInBand}");
    }

    /// <summary>
    /// Helper to update bar display safely with optional animation
    /// Uses NEW UNIFIED API: SetProgress(0-1)
    /// </summary>
    private void UpdateStatBar(HorizontalBar bar, int value, string statName)
    {
        if (bar != null)
        {
            // ⭐ NEW: Convert 0-10 stat to 0-1 progress
            // Example: 7 out of 10 → 0.7 progress
            float progress = value / 10f;
            bar.SetProgress(progress, instant: !animateBars);

            // Debug only in verbose mode
#if UNITY_EDITOR
            if (animateBars && value > 0)
            {
                Debug.Log($"   📊 {statName}: ▓▓▓▓▓░░░░░ ({value}/10)");
            }
#endif
        }
        else if (Application.isPlaying)
        {
            // Only log warning once to avoid spam
            Debug.LogWarning($"   ⚠️ {statName} Bar not assigned in CharacterViewer!", this);
        }
    }

    /// <summary>
    /// Clear all displays - useful for empty slots or reset
    /// </summary>
    public void ClearDisplay()
    {
        // Clear portrait
        if (portraitImage != null)
            portraitImage.sprite = null;

        // Clear texts
        if (nameText != null)
            nameText.text = "";

        if (realNameText != null)
            realNameText.text = "";

        if (hireCostText != null)
            hireCostText.text = "";

        if (upkeepCostText != null)
            upkeepCostText.text = "";

        if (traitText != null)
            traitText.text = "";

        if (bioText != null)
            bioText.text = "";

        // ⭐ NEW: Clear all bars using unified API (instant to 0)
        if (charismaBar != null) charismaBar.SetProgress(0f, instant: true);
        if (stagePerformanceBar != null) stagePerformanceBar.SetProgress(0f, instant: true);
        if (vocalBar != null) vocalBar.SetProgress(0f, instant: true);
        if (instrumentBar != null) instrumentBar.SetProgress(0f, instant: true);
        if (songwritingBar != null) songwritingBar.SetProgress(0f, instant: true);
        if (productionBar != null) productionBar.SetProgress(0f, instant: true);
        if (managementBar != null) managementBar.SetProgress(0f, instant: true);
        if (practicalBar != null) practicalBar.SetProgress(0f, instant: true);

        // Hide hired stamp
        if (hiredStampImage != null)
        {
            hiredStampImage.gameObject.SetActive(false);
        }

        Debug.Log("🗑️ CharacterViewer cleared");
    }

    /// <summary>
    /// Quick test method for editor
    /// </summary>
    [ContextMenu("Test Random Character")]
    private void TestRandomCharacter()
    {
        // Why: Create a test character with random stats
        SlotData testChar = ScriptableObject.CreateInstance<SlotData>();
        testChar.displayName = "Luna Nova";
        testChar.realName = "Lucia Martinez";
        testChar.hireCost = Random.Range(50, 500);
        testChar.upkeepCost = Random.Range(10, 100);
        testChar.trait = "Always testing, never resting";
        testChar.description = "This is a test character created for debugging purposes. They have random stats and love to help developers test their UI systems.";

        // Random stats 0-10
        testChar.charisma = Random.Range(0, 11);
        testChar.stagePerformance = Random.Range(0, 11);
        testChar.vocal = Random.Range(0, 11);
        testChar.instrument = Random.Range(0, 11);
        testChar.songwriting = Random.Range(0, 11);
        testChar.production = Random.Range(0, 11);
        testChar.management = Random.Range(0, 11);
        testChar.practical = Random.Range(0, 11);

        DisplayCharacter(testChar, Random.value > 0.5f);

        // Clean up test object
        DestroyImmediate(testChar);
    }
}