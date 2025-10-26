using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BandSlotDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    public Image portraitImage;
    public TextMeshProUGUI nameText;
    public Button removeButton;           // Why: The X button
    public GameObject removeButtonShadow; // Why: Shadow for the X button
    public GameObject emptySlotVisual;    // Why: "Empty" text or icon

    [Header("Slot Settings")]
    public int mySlotIndex;               // Why: Set this to 0, 1, 2, or 3 in Inspector (for display order)

    private BandSetupManager setupManager;
    private SlotData currentCharacter;    // Why: Track which character this slot is holding

    void Start()
    {
        // Why: Find the manager and hook up remove button
        setupManager = FindObjectOfType<BandSetupManager>();
        removeButton.onClick.AddListener(OnRemoveClicked);
        DisplayEmpty(); // Start empty
    }

    public void DisplayCharacter(SlotData character)
    {
        // Why: Show this character in the slot and remember who it is
        currentCharacter = character;

        portraitImage.sprite = character.sprite;
        portraitImage.enabled = true;
        nameText.text = character.displayName;
        nameText.enabled = true;
        removeButton.gameObject.SetActive(true);

        // Why: Show shadow if it exists
        if (removeButtonShadow != null)
        {
            removeButtonShadow.SetActive(true);
        }

        emptySlotVisual.SetActive(false);
    }

    public void DisplayEmpty()
    {
        // Why: Show this slot is empty
        currentCharacter = null;

        portraitImage.enabled = false;
        nameText.enabled = false;
        removeButton.gameObject.SetActive(false);

        // Why: Hide shadow if it exists
        if (removeButtonShadow != null)
        {
            removeButtonShadow.SetActive(false);
        }

        emptySlotVisual.SetActive(true);
    }

    private void OnRemoveClicked()
    {
        // Why: Tell the manager to remove THIS specific character
        if (currentCharacter != null)
        {
            setupManager.RemoveCharacterFromBand(currentCharacter);
        }
    }
}