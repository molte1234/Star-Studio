using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

/// <summary>
/// Manages the expanded character menu that appears when clicking on a character.
/// Handles moving the character to the focus position, showing action buttons, and returning the character.
/// </summary>
public class CharacterMenuController : MonoBehaviour
{
    public static CharacterMenuController Instance { get; private set; }

    [Header("References")]
    [SerializeField] private RoomController currentRoomController;
    [SerializeField] private Transform menuButtonsContainer;
    [SerializeField] private List<UIButton> actionButtons = new List<UIButton>(4);
    [SerializeField] private UIButton closeButton;

    [Header("Animation Settings")]
    [SerializeField] private float buttonAnimationDuration = 0.3f;
    [SerializeField] private float buttonSpacing = 100f;
    [SerializeField] private Ease buttonEase = Ease.OutBack;

    [Header("Character Movement")]
    [SerializeField] private float characterMoveDuration = 0.5f;
    [SerializeField] private Ease characterMoveEase = Ease.OutCubic;

    // State tracking
    private CharacterObject currentExpandedCharacter;
    private int originalSocketIndex = -1;
    private bool isMenuOpen = false;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple CharacterMenuController instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Hide menu buttons initially
        if (menuButtonsContainer != null)
        {
            menuButtonsContainer.gameObject.SetActive(false);
        }

        // Setup close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseMenu);
        }
    }

    /// <summary>
    /// Opens the character menu for the specified character.
    /// Moves the character to the focus socket and animates in the action buttons.
    /// </summary>
    public void OpenCharacterMenu(CharacterObject character, RoomController roomController)
    {
        if (isMenuOpen)
        {
            Debug.LogWarning("Menu is already open. Close current menu before opening a new one.");
            return;
        }

        if (character == null || roomController == null)
        {
            Debug.LogError("Cannot open menu: character or roomController is null.");
            return;
        }

        currentExpandedCharacter = character;
        currentRoomController = roomController;
        isMenuOpen = true;

        // Store original socket position
        originalSocketIndex = roomController.FindCharacterSocket(character.GetCharacter());

        if (originalSocketIndex == -1)
        {
            Debug.LogError("Character not found in any socket!");
            ResetMenuState();
            return;
        }

        // Move character to focus socket
        MoveCharacterToFocusSocket(character);

        // Animate in buttons
        AnimateButtonsIn();
    }

    /// <summary>
    /// Moves the character to the focus socket position with animation.
    /// </summary>
    private void MoveCharacterToFocusSocket(CharacterObject character)
    {
        if (currentRoomController == null || currentRoomController.focusSocket == null)
        {
            Debug.LogError("Focus socket not set in RoomController!");
            return;
        }

        // Get the focus socket position
        Vector3 focusPosition = currentRoomController.focusSocket.position;

        // Animate character movement
        character.transform.DOMove(focusPosition, characterMoveDuration)
            .SetEase(characterMoveEase);

        // Optional: Scale up slightly for emphasis
        character.transform.DOScale(1.1f, characterMoveDuration)
            .SetEase(characterMoveEase);
    }

    /// <summary>
    /// Animates the action buttons expanding from the character's position.
    /// </summary>
    private void AnimateButtonsIn()
    {
        if (menuButtonsContainer == null)
        {
            Debug.LogError("Menu buttons container not assigned!");
            return;
        }

        // Position the container at the character's position
        if (currentExpandedCharacter != null)
        {
            menuButtonsContainer.position = currentExpandedCharacter.transform.position;
        }

        // Show the container
        menuButtonsContainer.gameObject.SetActive(true);

        // Animate each button
        for (int i = 0; i < actionButtons.Count; i++)
        {
            if (actionButtons[i] == null) continue;

            GameObject buttonObj = actionButtons[i].gameObject;

            // Start from scale 0
            buttonObj.transform.localScale = Vector3.zero;

            // Calculate target position (spread horizontally)
            float xOffset = (i - (actionButtons.Count - 1) / 2f) * buttonSpacing;
            Vector3 targetLocalPos = new Vector3(xOffset, 0, 0);

            // Store original position
            Vector3 originalLocalPos = buttonObj.transform.localPosition;
            buttonObj.transform.localPosition = Vector3.zero;

            // Animate to target position and scale
            float delay = i * 0.05f; // Stagger the animations

            buttonObj.transform.DOLocalMove(targetLocalPos, buttonAnimationDuration)
                .SetDelay(delay)
                .SetEase(buttonEase);

            buttonObj.transform.DOScale(1f, buttonAnimationDuration)
                .SetDelay(delay)
                .SetEase(buttonEase);
        }

        // Animate close button (positioned above the action buttons)
        if (closeButton != null)
        {
            GameObject closeButtonObj = closeButton.gameObject;
            closeButtonObj.transform.localScale = Vector3.zero;

            closeButtonObj.transform.DOScale(1f, buttonAnimationDuration)
                .SetDelay(actionButtons.Count * 0.05f)
                .SetEase(buttonEase);
        }
    }

    /// <summary>
    /// Animates the buttons out and returns the character to their original socket.
    /// </summary>
    public void CloseMenu()
    {
        if (!isMenuOpen)
        {
            return;
        }

        // Animate buttons out
        AnimateButtonsOut(() =>
        {
            // After buttons are hidden, move character back
            ReturnCharacterToOriginalSocket();
        });
    }

    /// <summary>
    /// Animates buttons scaling down and hiding.
    /// </summary>
    private void AnimateButtonsOut(System.Action onComplete)
    {
        int completedAnimations = 0;
        int totalAnimations = actionButtons.Count + 1; // +1 for close button

        // Animate action buttons
        for (int i = 0; i < actionButtons.Count; i++)
        {
            if (actionButtons[i] == null) continue;

            GameObject buttonObj = actionButtons[i].gameObject;
            float delay = i * 0.03f;

            buttonObj.transform.DOScale(0f, buttonAnimationDuration * 0.7f)
                .SetDelay(delay)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    completedAnimations++;
                    if (completedAnimations >= totalAnimations)
                    {
                        menuButtonsContainer.gameObject.SetActive(false);
                        onComplete?.Invoke();
                    }
                });
        }

        // Animate close button
        if (closeButton != null)
        {
            closeButton.transform.DOScale(0f, buttonAnimationDuration * 0.7f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    completedAnimations++;
                    if (completedAnimations >= totalAnimations)
                    {
                        menuButtonsContainer.gameObject.SetActive(false);
                        onComplete?.Invoke();
                    }
                });
        }
    }

    /// <summary>
    /// Returns the character to their original socket position.
    /// </summary>
    private void ReturnCharacterToOriginalSocket()
    {
        if (currentExpandedCharacter == null || currentRoomController == null)
        {
            ResetMenuState();
            return;
        }

        // Get original socket position
        if (originalSocketIndex >= 0 && originalSocketIndex < currentRoomController.sockets.Length)
        {
            Vector3 originalPosition = currentRoomController.sockets[originalSocketIndex].position;

            // Animate character back
            currentExpandedCharacter.transform.DOMove(originalPosition, characterMoveDuration)
                .SetEase(characterMoveEase);

            // Scale back to normal
            currentExpandedCharacter.transform.DOScale(1f, characterMoveDuration)
                .SetEase(characterMoveEase)
                .OnComplete(() =>
                {
                    ResetMenuState();
                });
        }
        else
        {
            Debug.LogError($"Invalid original socket index: {originalSocketIndex}");
            ResetMenuState();
        }
    }

    /// <summary>
    /// Resets the menu state after closing.
    /// </summary>
    private void ResetMenuState()
    {
        currentExpandedCharacter = null;
        currentRoomController = null;
        originalSocketIndex = -1;
        isMenuOpen = false;
    }

    /// <summary>
    /// Checks if the menu is currently open.
    /// </summary>
    public bool IsMenuOpen()
    {
        return isMenuOpen;
    }

    /// <summary>
    /// Gets the currently expanded character.
    /// </summary>
    public CharacterObject GetCurrentExpandedCharacter()
    {
        return currentExpandedCharacter;
    }

    private void OnDestroy()
    {
        // Clean up listeners
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseMenu);
        }

        // Kill any ongoing tweens
        DOTween.Kill(transform);
        if (menuButtonsContainer != null)
        {
            DOTween.Kill(menuButtonsContainer);
        }
    }
}
