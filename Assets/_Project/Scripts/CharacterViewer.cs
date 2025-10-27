using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays a character's info in the Band Setup scene
/// Shows portrait, name, real name, costs, bio, all 8 stats as BARS (no text needed), and trait
/// Simplified: Bars show the values visually, no need for duplicate text
/// </summary>
public class CharacterViewer : MonoBehaviour
{
    [Header("Character Info")]
    public Image portraitImage;
    public TextMeshProUGUI nameText;          // Display name / Stage name
    public TextMeshProUGUI realNameText;      // Character's real name
    public TextMeshProUGUI traitText;         // One-liner personality trait

    [Header("Cost Display")]
    [Tooltip("Shows one-time recruitment/hire cost")]
    public TextMeshProUGUI hireCostText;
    [Tooltip("Shows ongoing salary/upkeep per quarter")]
    public TextMeshProUGUI upkeepCostText;

    [Header("Character Bio")]
    [Tooltip("Multi-line text area for character backstory/description")]
    public TextMeshProUGUI bioText;           // Character biography/backstory

    [Header("Stat Bars (0-10 Visual Display)")]
    [Tooltip("Drag the HorizontalBar component for each stat here")]
    public HorizontalBar charismaBar;         // Social, look, fan appeal
    public HorizontalBar stagePerformanceBar; // Live show entertainment
    public HorizontalBar vocalBar;            // Singing ability
    public HorizontalBar instrumentBar;       // Playing instrument
    public HorizontalBar songwritingBar;      // Creating music
    public HorizontalBar productionBar;       // Studio/technical skills
    public HorizontalBar managementBar;       // Business/organization
    public HorizontalBar practicalBar;        // General utility

    [Header("UI Feedback")]
    public Image hiredStampImage;             // Shows when character is already in band

    [Header("Animation Options")]
    [Tooltip("Should bars animate when switching characters?")]
    public bool animateBars = true;           // Toggle animation on/off

    /// <summary>
    /// Display all character info - called when browsing characters
    /// Simplified to use bars only (no redundant text)
    /// </summary>
    public void DisplayCharacter(SlotData character, bool isAlreadyInBand)
    {
        // Why: Safety check
        if (character == null)
        {
            Debug.LogError("DisplayCharacter called with null character!");
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

        // Why: Update all 8 stat BARS - bars show the values visually, no text needed!
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
    /// </summary>
    private void UpdateStatBar(HorizontalBar bar, int value, string statName)
    {
        if (bar != null)
        {
            // Why: All stats are 0-10 range, bars show this visually
            bar.SetValue(value, 10, instant: !animateBars);

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

        // Clear all bars (instant to 0)
        if (charismaBar != null) charismaBar.SetValue(0, 10, instant: true);
        if (stagePerformanceBar != null) stagePerformanceBar.SetValue(0, 10, instant: true);
        if (vocalBar != null) vocalBar.SetValue(0, 10, instant: true);
        if (instrumentBar != null) instrumentBar.SetValue(0, 10, instant: true);
        if (songwritingBar != null) songwritingBar.SetValue(0, 10, instant: true);
        if (productionBar != null) productionBar.SetValue(0, 10, instant: true);
        if (managementBar != null) managementBar.SetValue(0, 10, instant: true);
        if (practicalBar != null) practicalBar.SetValue(0, 10, instant: true);

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