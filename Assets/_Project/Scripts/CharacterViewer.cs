using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays a character's info in the Band Setup scene
/// Shows portrait, name, all 8 stats, and trait
/// </summary>
public class CharacterViewer : MonoBehaviour
{
    [Header("UI Elements")]
    public Image portraitImage;
    public TextMeshProUGUI nameText;

    [Header("NEW 8-Stat System")]
    public TextMeshProUGUI charismaText;         // Social, look, fan appeal
    public TextMeshProUGUI stagePerformanceText; // Live show entertainment
    public TextMeshProUGUI vocalText;            // Singing ability
    public TextMeshProUGUI instrumentText;       // Playing instrument
    public TextMeshProUGUI songwritingText;      // Creating music
    public TextMeshProUGUI productionText;       // Studio/technical skills
    public TextMeshProUGUI managementText;       // Business/organization
    public TextMeshProUGUI practicalText;        // General utility

    public TextMeshProUGUI traitText;

    [Header("Hired Stamp")]
    public Image hiredStampImage;                // Why: Shows when character is already in band

    /// <summary>
    /// Display all character info - called when browsing characters
    /// </summary>
    public void DisplayCharacter(SlotData character, bool isAlreadyInBand)
    {
        // Why: Show portrait and name
        portraitImage.sprite = character.sprite;
        nameText.text = character.displayName;

        // Why: Display all 8 stat values (labels are separate UI elements)
        charismaText.text = character.charisma.ToString();
        stagePerformanceText.text = character.stagePerformance.ToString();
        vocalText.text = character.vocal.ToString();
        instrumentText.text = character.instrument.ToString();
        songwritingText.text = character.songwriting.ToString();
        productionText.text = character.production.ToString();
        managementText.text = character.management.ToString();
        practicalText.text = character.practical.ToString();

        traitText.text = character.trait;

        // Why: Show "Hired" stamp if character is already in the band
        if (hiredStampImage != null)
        {
            hiredStampImage.gameObject.SetActive(isAlreadyInBand);
        }
    }
}