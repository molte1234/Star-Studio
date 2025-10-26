using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterViewer : MonoBehaviour
{
    [Header("UI Elements")]
    public Image portraitImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI technicalText;      // Why: Just the number
    public TextMeshProUGUI performanceText;    // Why: Just the number
    public TextMeshProUGUI charismaText;       // Why: Just the number
    public TextMeshProUGUI traitText;

    [Header("Hired Stamp")]
    public Image hiredStampImage;              // Why: Shows when character is already in band

    public void DisplayCharacter(SlotData character, bool isAlreadyInBand)
    {
        // Why: Show all of this character's info
        portraitImage.sprite = character.sprite;
        nameText.text = character.displayName;

        // Why: Display stat values only (labels are separate UI elements)
        technicalText.text = character.technical.ToString();
        performanceText.text = character.performance.ToString();
        charismaText.text = character.charisma.ToString();

        traitText.text = character.trait;

        // Why: Show "Hired" stamp if character is already in the band
        if (hiredStampImage != null)
        {
            hiredStampImage.gameObject.SetActive(isAlreadyInBand);
        }
    }
}