using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BandSlotDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    public Image portraitImage;
    public TextMeshProUGUI nameText;
    public Button removeButton; // The X button
    public GameObject emptySlotVisual; // "Empty" text or icon

    private int mySlotIndex; // Set this in Inspector (0, 1, or 2)
    private BandSetupManager setupManager;

    void Start()
    {
        setupManager = FindObjectOfType<BandSetupManager>();
        removeButton.onClick.AddListener(OnRemoveClicked);
        DisplayEmpty(); // Start empty
    }

    public void DisplayCharacter(SlotData character)
    {
        // Why: Show this character in the slot
        portraitImage.sprite = character.sprite;
        portraitImage.enabled = true;
        nameText.text = character.displayName;
        nameText.enabled = true;
        removeButton.gameObject.SetActive(true);
        emptySlotVisual.SetActive(false);
    }

    public void DisplayEmpty()
    {
        // Why: Show this slot is empty
        portraitImage.enabled = false;
        nameText.enabled = false;
        removeButton.gameObject.SetActive(false);
        emptySlotVisual.SetActive(true);
    }

    private void OnRemoveClicked()
    {
        // Why: Tell the manager to remove this slot
        setupManager.RemoveFromBand(mySlotIndex);
    }
}