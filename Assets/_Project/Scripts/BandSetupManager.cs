using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BandSetupManager : MonoBehaviour
{
    [Header("Character Pool")]
    public SlotData[] availableCharacters; // Drag 8-10 characters here in Inspector
    private int currentCharacterIndex = 0;

    [Header("Selected Band")]
    public SlotData[] selectedBand = new SlotData[4]; // The 4 chosen members
    private int nextEmptySlot = 0; // Which slot to fill next (0, 1, 2, or 3)

    [Header("UI References")]
    public CharacterViewer characterViewer;
    public BandSlotDisplay[] bandSlotDisplays; // 4 slots
    public Button addToBandButton;
    public Button startGameButton;
    public TMP_InputField bandNameInput;

    void Start()
    {
        // Show the first character
        ShowCurrentCharacter();

        // Start button disabled until band is full
        startGameButton.interactable = false;
    }

    public void ShowNextCharacter()
    {
        // Why: Cycle forward through available characters
        currentCharacterIndex = (currentCharacterIndex + 1) % availableCharacters.Length;
        ShowCurrentCharacter();
    }

    public void ShowPreviousCharacter()
    {
        // Why: Cycle backward through available characters
        currentCharacterIndex--;
        if (currentCharacterIndex < 0)
        {
            currentCharacterIndex = availableCharacters.Length - 1;
        }
        ShowCurrentCharacter();
    }

    private void ShowCurrentCharacter()
    {
        // Why: Update the character viewer to show current selection
        SlotData character = availableCharacters[currentCharacterIndex];
        characterViewer.DisplayCharacter(character);

        // Why: Disable add button if character already in band or band is full
        bool alreadyInBand = IsCharacterInBand(character);
        bool bandIsFull = (nextEmptySlot >= 4);
        addToBandButton.interactable = !alreadyInBand && !bandIsFull;
    }

    public void AddCurrentCharacterToBand()
    {
        // Why: Add the displayed character to the next empty band slot
        if (nextEmptySlot >= 4) return; // Band is full

        SlotData character = availableCharacters[currentCharacterIndex];
        if (IsCharacterInBand(character)) return; // Already added

        selectedBand[nextEmptySlot] = character;
        bandSlotDisplays[nextEmptySlot].DisplayCharacter(character);
        nextEmptySlot++;

        // Why: Play character select sound effect
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCharacterSelect();
        }

        // Why: Enable start button when band is full (4 members)
        if (nextEmptySlot >= 4)
        {
            startGameButton.interactable = true;

            // Why: Play band complete sound effect
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBandComplete();
            }
        }

        ShowCurrentCharacter(); // Update button states
    }

    public void RemoveFromBand(int slotIndex)
    {
        // Why: Player can click X to remove a character from band
        if (slotIndex >= nextEmptySlot) return; // Slot is empty

        // Shift remaining characters left to fill the gap
        for (int i = slotIndex; i < 3; i++)
        {
            selectedBand[i] = selectedBand[i + 1];
        }
        selectedBand[3] = null;

        nextEmptySlot--;

        // Update all displays
        RefreshBandSlotDisplays();
        startGameButton.interactable = false;
        ShowCurrentCharacter();
    }

    private void RefreshBandSlotDisplays()
    {
        // Why: Update all 4 slot displays after removal
        for (int i = 0; i < 4; i++)
        {
            if (i < nextEmptySlot)
            {
                bandSlotDisplays[i].DisplayCharacter(selectedBand[i]);
            }
            else
            {
                bandSlotDisplays[i].DisplayEmpty();
            }
        }
    }

    private bool IsCharacterInBand(SlotData character)
    {
        // Why: Check if character is already selected
        for (int i = 0; i < nextEmptySlot; i++)
        {
            if (selectedBand[i] == character) return true;
        }
        return false;
    }

    public void StartGame()
    {
        // Why: Save band selection to GameManager and load main game
        string bandName = bandNameInput.text;
        if (string.IsNullOrEmpty(bandName))
        {
            bandName = "The Unnamed Band"; // Default name
        }

        // Pass data to GameManager
        GameManager.Instance.SetupNewGame(selectedBand, bandName);

        // Load MainGame scene additively
        SceneLoader.Instance.LoadScene("MainGame");
    }
}