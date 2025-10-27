using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the Band Setup scene
/// Handles character browsing, selection, and money transactions
/// </summary>
public class BandSetupManager : MonoBehaviour
{
    [Header("Character Pool")]
    public SlotData[] availableCharacters; // Drag characters/items here in Inspector
    private int currentCharacterIndex = 0;

    [Header("Selected Band")]
    public SlotData[] selectedBand = new SlotData[6]; // Why: 6 slots (characters + items)
    private int nextEmptySlot = 0; // Which slot to fill next

    [Header("UI References")]
    public CharacterViewer characterViewer;
    public BandSlotDisplay[] bandSlotDisplays; // Why: Can be 3, 4, 6... whatever is visible
    public Button addToBandButton;
    public Button startGameButton;
    public TMP_InputField bandNameInput;

    private int maxSlots; // Why: Calculated based on how many displays are connected

    void Start()
    {
        // Why: Max slots = however many displays are wired up (3, 4, or 6)
        maxSlots = bandSlotDisplays != null ? bandSlotDisplays.Length : 6;

        // Why: Show the first character
        ShowCurrentCharacter();

        // Why: Start button disabled until at least 1 character/item added
        startGameButton.interactable = false;
    }

    // ============================================
    // CHARACTER BROWSING
    // ============================================

    /// <summary>
    /// Called by UIController_Setup when NEXT button clicked
    /// </summary>
    public void ShowNextCharacter()
    {
        currentCharacterIndex = (currentCharacterIndex + 1) % availableCharacters.Length;
        ShowCurrentCharacter();
    }

    /// <summary>
    /// Called by UIController_Setup when PREV button clicked
    /// </summary>
    public void ShowPreviousCharacter()
    {
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
        bool alreadyInBand = IsCharacterInBand(character);

        // Why: Pass both character data and "already hired" status
        characterViewer.DisplayCharacter(character, alreadyInBand);

        // Why: Disable add button if already in band, all visible slots are full, OR not enough money
        bool bandIsFull = (nextEmptySlot >= maxSlots);
        bool canAfford = (GameManager.Instance.money >= character.hireCost);
        addToBandButton.interactable = !alreadyInBand && !bandIsFull && canAfford;
    }

    // ============================================
    // ADD CHARACTER (WITH MONEY TRANSACTION)
    // ============================================

    /// <summary>
    /// Called by UIController_Setup when ADD button clicked
    /// Deducts hire cost from player's money
    /// </summary>
    public void AddCurrentCharacterToBand()
    {
        // Why: Safety checks
        if (nextEmptySlot >= maxSlots) return; // All visible slots are full

        SlotData character = availableCharacters[currentCharacterIndex];
        if (IsCharacterInBand(character)) return; // Already added

        // Why: Check if player can afford this character
        if (GameManager.Instance.money < character.hireCost)
        {
            Debug.LogWarning($"❌ Cannot hire {character.displayName} - Not enough money! (Need ${character.hireCost}, have ${GameManager.Instance.money})");

            // Why: Play error sound (using button hover as negative feedback)
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonHover();
            }

            return;
        }

        // Why: DEDUCT HIRE COST FROM PLAYER'S MONEY
        GameManager.Instance.money -= character.hireCost;
        Debug.Log($"💰 Hired {character.displayName} for ${character.hireCost} (Remaining: ${GameManager.Instance.money})");

        // Why: Add character to band
        selectedBand[nextEmptySlot] = character;
        bandSlotDisplays[nextEmptySlot].DisplayCharacter(character);
        nextEmptySlot++;

        // Why: Play money OUT sound + character select sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMoneyOut();
            AudioManager.Instance.PlayCharacterSelect();
        }

        // Why: Enable start button when at least 1 character/item is in the band
        if (nextEmptySlot >= 1)
        {
            startGameButton.interactable = true;
        }

        // Why: Update viewer (hired stamp + button states)
        ShowCurrentCharacter();
    }

    // ============================================
    // REMOVE CHARACTER (WITH REFUND)
    // ============================================

    /// <summary>
    /// Called by BandSlotDisplay when X button clicked
    /// Refunds the hire cost back to player
    /// </summary>
    public void RemoveCharacterFromBand(SlotData characterToRemove)
    {
        // Why: Find which slot this character/item is in
        int foundIndex = -1;
        for (int i = 0; i < nextEmptySlot; i++)
        {
            if (selectedBand[i] == characterToRemove)
            {
                foundIndex = i;
                break;
            }
        }

        // Why: If not found, do nothing
        if (foundIndex == -1) return;

        // Why: REFUND HIRE COST TO PLAYER'S MONEY
        GameManager.Instance.money += characterToRemove.hireCost;
        Debug.Log($"💰 Removed {characterToRemove.displayName}, refunded ${characterToRemove.hireCost} (Total: ${GameManager.Instance.money})");

        // Why: Play money IN sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMoneyIn();
        }

        // Why: Shift all characters after this one to the left
        for (int i = foundIndex; i < nextEmptySlot - 1; i++)
        {
            selectedBand[i] = selectedBand[i + 1];
        }

        // Why: Clear the last slot
        selectedBand[nextEmptySlot - 1] = null;
        nextEmptySlot--;

        // Why: Refresh all slot displays
        RefreshBandSlotDisplays();

        // Why: Disable start button if no characters left, otherwise keep it enabled
        startGameButton.interactable = (nextEmptySlot >= 1);

        // Why: Update character viewer (hired stamp should disappear)
        ShowCurrentCharacter();
    }

    private void RefreshBandSlotDisplays()
    {
        // Why: Update all visible slot displays (works with 3, 4, or 6 slots)
        for (int i = 0; i < maxSlots; i++)
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
        // Why: Check if character/item is already selected
        for (int i = 0; i < nextEmptySlot; i++)
        {
            if (selectedBand[i] == character) return true;
        }
        return false;
    }

    // ============================================
    // START GAME
    // ============================================

    /// <summary>
    /// Called by UIController_Setup when START button clicked
    /// </summary>
    public void StartGame()
    {
        // Why: Play game start sound effect (using band complete sound for now)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBandComplete();
        }

        // Why: Save band selection to GameManager and load main game
        string bandName = bandNameInput.text;
        if (string.IsNullOrEmpty(bandName))
        {
            bandName = "The Unnamed Band"; // Default name
        }

        // Why: Pass data to GameManager (all 6 slots, some may be null)
        GameManager.Instance.SetupNewGame(selectedBand, bandName);

        // Why: Load Game scene
        SceneLoader.Instance.LoadGame();
    }
}