using UnityEngine;
using TMPro;

/// <summary>
/// Helper script for mini stats panel
/// Shows character name and 8 stat bars
/// Attach this to your stats panel root GameObject
/// Wire up name text and stat bars in Inspector
/// </summary>
public class MiniStats : MonoBehaviour
{
    [Header("Character Info")]
    public TextMeshProUGUI characterNameText;

    [Header("Stat Bars (HorizontalBar rotated 90°)")]
    public HorizontalBar charismaBar;
    public HorizontalBar stagePerformanceBar;
    public HorizontalBar vocalBar;
    public HorizontalBar instrumentBar;
    public HorizontalBar songwritingBar;
    public HorizontalBar productionBar;
    public HorizontalBar managementBar;
    public HorizontalBar practicalBar;

    /// <summary>
    /// Display stats for a character
    /// Stats are 0-10, bars expect 0.0-1.0
    /// </summary>
    public void ShowCharacter(SlotData character)
    {
        if (character == null)
        {
            Hide();
            return;
        }

        gameObject.SetActive(true);

        // Update name
        if (characterNameText != null)
            characterNameText.text = character.displayName;

        // Update stat bars (convert 0-10 to 0.0-1.0)
        if (charismaBar != null)
            charismaBar.SetProgress(character.charisma / 10f);

        if (stagePerformanceBar != null)
            stagePerformanceBar.SetProgress(character.stagePerformance / 10f);

        if (vocalBar != null)
            vocalBar.SetProgress(character.vocal / 10f);

        if (instrumentBar != null)
            instrumentBar.SetProgress(character.instrument / 10f);

        if (songwritingBar != null)
            songwritingBar.SetProgress(character.songwriting / 10f);

        if (productionBar != null)
            productionBar.SetProgress(character.production / 10f);

        if (managementBar != null)
            managementBar.SetProgress(character.management / 10f);

        if (practicalBar != null)
            practicalBar.SetProgress(character.practical / 10f);
    }

    /// <summary>
    /// Hide the stats panel
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}