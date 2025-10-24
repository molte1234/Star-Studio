using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterViewer : MonoBehaviour
{
    [Header("UI Elements")]
    public Image portraitImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI technicalText;
    public TextMeshProUGUI performanceText;
    public TextMeshProUGUI charismaText;
    public TextMeshProUGUI traitText;

    public void DisplayCharacter(SlotData character)
    {
        // Why: Show all of this character's info
        portraitImage.sprite = character.sprite;
        nameText.text = character.displayName;

        // Why: Show stats clearly (assuming SlotData has these)
        technicalText.text = "Technical: " + character.technical;
        performanceText.text = "Performance: " + character.performance;
        charismaText.text = "Charisma: " + character.charisma;

        traitText.text = character.trait;
    }
}