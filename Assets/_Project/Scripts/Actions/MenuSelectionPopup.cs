using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

/// <summary>
/// Popup for selecting which band members participate in an action
/// Matches Unity hierarchy: Panel - MemberSelectionPopup structure
/// Handles material states (Unselected/Selected/Unavailable) and text colors
/// </summary>
public class MemberSelectionPopup : MonoBehaviour
{
    [Header("Panel Reference")]
    [Tooltip("The popup panel GameObject (contains all UI) - will be shown/hidden and animated")]
    public GameObject popupPanel;

    [Header("Info Display")]
    [Tooltip("Text - Title")]
    public TextMeshProUGUI titleText;

    [Tooltip("Text - Description")]
    public TextMeshProUGUI descriptionText;

    [Tooltip("Text - Money (shows cost amount like '$150')")]
    public TextMeshProUGUI moneyText;

    [Tooltip("Text - Time (shows duration like '50 SECONDS')")]
    public TextMeshProUGUI timeText;

    [Tooltip("Text - Min Members (shows '(2 MEMBERS MINIMUM)' or hide if min=1)")]
    public TextMeshProUGUI minMembersText;

    [Header("Character Grid")]
    [Tooltip("Panel - MemberSelection (container for character slots)")]
    public Transform characterGridContainer;

    [Tooltip("Prefab: miniPortrait with Image, Text-Name, Text-Status, Button")]
    public GameObject characterSlotPrefab;

    [Header("Portrait Materials")]
    [Tooltip("Material for unselected/available state")]
    public Material unselectedMaterial;

    [Tooltip("Material for selected state (green glow)")]
    public Material selectedMaterial;

    [Tooltip("Material for unavailable/busy state (striped pattern)")]
    public Material unavailableMaterial;

    [Header("Text Colors")]
    [Tooltip("Color for name/status when available (white)")]
    public Color availableTextColor = Color.white;

    [Tooltip("Color for name/status when unavailable (gray)")]
    public Color unavailableTextColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    [Header("Buttons")]
    [Tooltip("Button - OK")]
    public Button okButton;

    [Tooltip("Button - Cancel")]
    public Button cancelButton;

    [Header("Animation Settings")]
    public float popupDuration = 0.3f;
    public Ease popupEase = Ease.OutBack;

    // ============================================
    // RUNTIME DATA
    // ============================================

    private ActionData currentAction;
    private CharacterSlot[] characterSlots; // Tracks each character slot's components and state

    // Why: Callback when player confirms selection
    public System.Action<ActionData, List<int>> OnConfirm;

    // ============================================
    // HELPER CLASS - Character Slot Wrapper
    // ============================================

    private class CharacterSlot
    {
        public GameObject slotObject;
        public int characterIndex; // Index in GameManager.characterStates[]
        public Image portraitImage;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI statusText;
        public Button button;
        public bool isAvailable;
        public bool isSelected;
    }

    // ============================================
    // LIFECYCLE
    // ============================================

    void Awake()
    {
        // Why: Start hidden
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }

        // Why: Hook up button clicks
        if (okButton != null)
        {
            okButton.onClick.AddListener(OnOKClicked);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelClicked);
        }
    }

    // ============================================
    // SHOW POPUP
    // ============================================

    /// <summary>
    /// Opens the popup for the given action
    /// </summary>
    public void ShowPopup(ActionData action)
    {
        if (action == null)
        {
            Debug.LogError("MemberSelectionPopup: Cannot show popup - action is null!");
            return;
        }

        currentAction = action;

        // Populate info
        PopulateActionInfo(action);

        // Create character grid
        CreateCharacterGrid();

        // Show with animation
        ShowWithAnimation();

        // Update OK button state
        ValidateSelection();
    }

    private void PopulateActionInfo(ActionData action)
    {
        // Why: Fill in action details

        if (titleText != null)
        {
            titleText.text = action.actionName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = action.actionDescription;
        }

        // TODO: When ActionData has cost/time/minMembers fields, use real values
        // For now, placeholder values
        if (moneyText != null)
        {
            moneyText.text = "$???"; // TODO: "$" + action.initialCost
        }

        if (timeText != null)
        {
            timeText.text = "?? SECONDS"; // TODO: action.timeDuration + " SECONDS"
        }

        if (minMembersText != null)
        {
            // TODO: Hide if action.minMembers <= 1
            // TODO: Show "(X MEMBERS MINIMUM)" if action.minMembers > 1
            minMembersText.text = "(2 MEMBERS MINIMUM)"; // Placeholder
            // minMembersText.gameObject.SetActive(action.minMembers > 1);
        }
    }

    // ============================================
    // CHARACTER GRID
    // ============================================

    private void CreateCharacterGrid()
    {
        // Why: Clean up old slots first
        ClearGrid();

        GameManager gm = GameManager.Instance;
        if (gm == null)
        {
            Debug.LogError("MemberSelectionPopup: GameManager is null!");
            return;
        }

        // Count how many characters we have
        int characterCount = 0;
        for (int i = 0; i < gm.characterStates.Length; i++)
        {
            if (gm.characterStates[i] != null && gm.characterStates[i].slotData != null)
            {
                characterCount++;
            }
        }

        // Create array for tracking slots
        characterSlots = new CharacterSlot[characterCount];

        int slotIndex = 0;

        // Create a slot for each character
        for (int i = 0; i < gm.characterStates.Length; i++)
        {
            CharacterSlotState charState = gm.characterStates[i];

            // Skip empty slots
            if (charState == null || charState.slotData == null)
                continue;

            // Create slot UI
            GameObject slotObj = Instantiate(characterSlotPrefab, characterGridContainer);

            // Create slot wrapper
            CharacterSlot slot = new CharacterSlot();
            slot.slotObject = slotObj;
            slot.characterIndex = i;
            slot.isAvailable = charState.IsAvailable();
            slot.isSelected = false;

            // Find components in the prefab structure
            // Structure: Image (root), Text-Name (child), Text-Status (child)

            // Portrait Image is on the root GameObject
            slot.portraitImage = slotObj.GetComponent<Image>();

            // Find the two text children
            TextMeshProUGUI[] texts = slotObj.GetComponentsInChildren<TextMeshProUGUI>();

            // Assign based on child count (assumes first child is Name, second is Status)
            if (texts.Length >= 2)
            {
                slot.nameText = texts[0];     // First text child = Name
                slot.statusText = texts[1];   // Second text child = Status
            }
            else if (texts.Length == 1)
            {
                slot.nameText = texts[0];
                Debug.LogWarning("MemberSelectionPopup: Character slot only has 1 text child - Status text missing!");
            }
            else
            {
                Debug.LogError("MemberSelectionPopup: Character slot has no text children!");
            }

            // Button is also on root (same GameObject as Image)
            slot.button = slotObj.GetComponent<Button>();

            // Set portrait sprite
            if (slot.portraitImage != null && charState.slotData.sprite != null)
            {
                slot.portraitImage.sprite = charState.slotData.sprite;
            }

            // Set name
            if (slot.nameText != null)
            {
                slot.nameText.text = charState.slotData.displayName;
            }

            // Hook up button click
            if (slot.button != null)
            {
                int capturedIndex = slotIndex; // Capture for closure
                slot.button.onClick.AddListener(() => OnCharacterSlotClicked(capturedIndex));
            }

            // Set initial visual state
            UpdateSlotVisualState(slot);

            // Store in array
            characterSlots[slotIndex] = slot;
            slotIndex++;
        }
    }

    private void UpdateSlotVisualState(CharacterSlot slot)
    {
        // Why: Update material, text colors, and status text based on slot state

        if (!slot.isAvailable)
        {
            // UNAVAILABLE STATE (busy with another action)
            if (slot.portraitImage != null && unavailableMaterial != null)
            {
                slot.portraitImage.material = unavailableMaterial;
            }

            if (slot.nameText != null)
            {
                slot.nameText.color = unavailableTextColor;
            }

            if (slot.statusText != null)
            {
                slot.statusText.text = "BUSY";
                slot.statusText.color = unavailableTextColor;
                slot.statusText.gameObject.SetActive(true);
            }

            // Disable button
            if (slot.button != null)
            {
                slot.button.interactable = false;
            }
        }
        else if (slot.isSelected)
        {
            // SELECTED STATE (added to action)
            if (slot.portraitImage != null && selectedMaterial != null)
            {
                slot.portraitImage.material = selectedMaterial;
            }

            if (slot.nameText != null)
            {
                slot.nameText.color = availableTextColor;
            }

            if (slot.statusText != null)
            {
                slot.statusText.text = "ADDED";
                slot.statusText.color = availableTextColor;
                slot.statusText.gameObject.SetActive(true);
            }

            // Keep button enabled so they can deselect
            if (slot.button != null)
            {
                slot.button.interactable = true;
            }
        }
        else
        {
            // UNSELECTED STATE (available but not added)
            if (slot.portraitImage != null && unselectedMaterial != null)
            {
                slot.portraitImage.material = unselectedMaterial;
            }

            if (slot.nameText != null)
            {
                slot.nameText.color = availableTextColor;
            }

            if (slot.statusText != null)
            {
                slot.statusText.gameObject.SetActive(false); // Hide status text when unselected
            }

            if (slot.button != null)
            {
                slot.button.interactable = true;
            }
        }
    }

    private void ClearGrid()
    {
        // Why: Destroy old slot UI objects
        if (characterSlots != null)
        {
            foreach (CharacterSlot slot in characterSlots)
            {
                if (slot != null && slot.slotObject != null)
                {
                    Destroy(slot.slotObject);
                }
            }
        }

        characterSlots = null;
    }

    // ============================================
    // CHARACTER SLOT INTERACTION
    // ============================================

    private void OnCharacterSlotClicked(int slotIndex)
    {
        // Why: Toggle selection state when player clicks a character portrait

        if (characterSlots == null || slotIndex >= characterSlots.Length)
            return;

        CharacterSlot slot = characterSlots[slotIndex];

        // Can't select unavailable characters
        if (!slot.isAvailable)
            return;

        // Toggle selection
        slot.isSelected = !slot.isSelected;

        // Update visual state
        UpdateSlotVisualState(slot);

        // Play sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        // Update OK button state
        ValidateSelection();
    }

    // ============================================
    // VALIDATION
    // ============================================

    private void ValidateSelection()
    {
        // Why: Check if current selection is valid, enable/disable OK button

        if (okButton == null) return;

        // Count selected members
        int selectedCount = 0;
        if (characterSlots != null)
        {
            foreach (CharacterSlot slot in characterSlots)
            {
                if (slot != null && slot.isSelected)
                {
                    selectedCount++;
                }
            }
        }

        // TODO: Check against action.minMembers and action.maxMembers
        // For now, just require at least 1 member
        bool isValid = (selectedCount >= 1);

        // TODO: Check if player has enough money (action.initialCost)
        // For now, assume always valid

        okButton.interactable = isValid;
    }

    // ============================================
    // BUTTON CALLBACKS
    // ============================================

    private void OnOKClicked()
    {
        // Why: Gather selected member indices and pass to callback

        List<int> selectedIndices = new List<int>();

        if (characterSlots != null)
        {
            foreach (CharacterSlot slot in characterSlots)
            {
                if (slot != null && slot.isSelected)
                {
                    selectedIndices.Add(slot.characterIndex);
                }
            }
        }

        // Play sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        // Hide popup
        HidePopup();

        // Invoke callback
        OnConfirm?.Invoke(currentAction, selectedIndices);
    }

    private void OnCancelClicked()
    {
        // Play sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        // Just close
        HidePopup();
    }

    // ============================================
    // ANIMATION
    // ============================================

    private void ShowWithAnimation()
    {
        if (popupPanel == null) return;

        popupPanel.SetActive(true);

        // Scale from 0 to 1 with bounce
        RectTransform panelTransform = popupPanel.GetComponent<RectTransform>();
        if (panelTransform != null)
        {
            panelTransform.localScale = Vector3.zero;
            panelTransform.DOScale(Vector3.one, popupDuration).SetEase(popupEase);
        }
    }

    public void HidePopup()
    {
        if (popupPanel == null) return;

        // Scale down
        RectTransform panelTransform = popupPanel.GetComponent<RectTransform>();
        if (panelTransform != null)
        {
            panelTransform.DOScale(Vector3.zero, popupDuration * 0.5f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    popupPanel.SetActive(false);
                    ClearGrid();
                });
        }
        else
        {
            popupPanel.SetActive(false);
            ClearGrid();
        }
    }
}