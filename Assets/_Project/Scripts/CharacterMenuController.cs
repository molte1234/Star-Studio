using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
#endif

/// <summary>
/// Manages the character selection panel that appears when clicking on a character.
/// Shows action buttons with Persona 5-style popup animations.
/// Character is moved temporarily to the socket without affecting room data.
/// </summary>
public class CharacterMenuController : MonoBehaviour
{
    public static CharacterMenuController Instance { get; private set; }

#if UNITY_EDITOR
    [FoldoutGroup("Panel References")]
#endif
    [SerializeField] private GameObject characterSelectPanel;

#if UNITY_EDITOR
    [FoldoutGroup("Panel References")]
#endif
    [SerializeField] private Transform characterSocket;

#if UNITY_EDITOR
    [FoldoutGroup("Panel References")]
#endif
    [SerializeField] private RectTransform buttonsSubPanel;

#if UNITY_EDITOR
    [FoldoutGroup("Action Buttons")]
    [Tooltip("Assign buttons in order: Move To, Practice, Music, Action")]
#endif
    [SerializeField] private UIButton moveToButton;

#if UNITY_EDITOR
    [FoldoutGroup("Action Buttons")]
#endif
    [SerializeField] private UIButton practiceButton;

#if UNITY_EDITOR
    [FoldoutGroup("Action Buttons")]
#endif
    [SerializeField] private UIButton musicButton;

#if UNITY_EDITOR
    [FoldoutGroup("Action Buttons")]
#endif
    [SerializeField] private UIButton actionButton;

#if UNITY_EDITOR
    [FoldoutGroup("Action Buttons")]
#endif
    [SerializeField] private UIButton closeButton;

#if UNITY_EDITOR
    [FoldoutGroup("Animation Settings")]
    [Tooltip("Persona 5-style quick popup animation")]
#endif
    [SerializeField] private float panelFadeInDuration = 0.3f;

#if UNITY_EDITOR
    [FoldoutGroup("Animation Settings")]
#endif
    [SerializeField] private float buttonPopDuration = 0.4f;

#if UNITY_EDITOR
    [FoldoutGroup("Animation Settings")]
#endif
    [SerializeField] private float buttonStaggerDelay = 0.05f;

#if UNITY_EDITOR
    [FoldoutGroup("Animation Settings")]
#endif
    [SerializeField] private Ease buttonPopEase = Ease.OutBack;

#if UNITY_EDITOR
    [FoldoutGroup("Animation Settings")]
#endif
    [SerializeField] private float buttonRotationAmount = 15f;

#if UNITY_EDITOR
    [FoldoutGroup("Character Movement")]
#endif
    [SerializeField] private float characterMoveDuration = 0.5f;

#if UNITY_EDITOR
    [FoldoutGroup("Character Movement")]
#endif
    [SerializeField] private Ease characterMoveEase = Ease.OutCubic;

    // State tracking
    private CharacterObject currentExpandedCharacter;
    private Vector3 originalCharacterPosition;
    private Vector3 originalCharacterScale;
    private Transform originalCharacterParent;
    private int originalSocketIndex = -1;
    private RoomController currentRoomController;
    private bool isMenuOpen = false;

    private List<UIButton> allActionButtons;

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

        // Build action buttons list
        allActionButtons = new List<UIButton>();
        if (moveToButton != null) allActionButtons.Add(moveToButton);
        if (practiceButton != null) allActionButtons.Add(practiceButton);
        if (musicButton != null) allActionButtons.Add(musicButton);
        if (actionButton != null) allActionButtons.Add(actionButton);
    }

    private void Start()
    {
        // Hide character select panel initially
        if (characterSelectPanel != null)
        {
            characterSelectPanel.SetActive(false);
        }

        // Setup button listeners
        SetupButtonListeners();
    }

    private void SetupButtonListeners()
    {
        if (closeButton != null && closeButton.onClick != null)
        {
            closeButton.onClick.AddListener(CloseMenu);
        }

        if (moveToButton != null && moveToButton.onClick != null)
        {
            moveToButton.onClick.AddListener(() => OnMoveToClicked());
        }

        if (practiceButton != null && practiceButton.onClick != null)
        {
            practiceButton.onClick.AddListener(() => OnPracticeClicked());
        }

        if (musicButton != null && musicButton.onClick != null)
        {
            musicButton.onClick.AddListener(() => OnMusicClicked());
        }

        if (actionButton != null && actionButton.onClick != null)
        {
            actionButton.onClick.AddListener(() => OnActionClicked());
        }
    }

    /// <summary>
    /// Opens the character menu for the specified character.
    /// Moves the character to the socket and shows action buttons with Persona 5-style animation.
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

        // Store original character data (so we can restore without affecting room data)
        originalCharacterPosition = character.transform.position;
        originalCharacterScale = character.transform.localScale;
        originalCharacterParent = character.transform.parent;
        originalSocketIndex = roomController.FindCharacterSocket(character.GetCharacter());

        if (originalSocketIndex == -1)
        {
            Debug.LogError("Character not found in any socket!");
            ResetMenuState();
            return;
        }

        // Show panel and animate character
        ShowPanelWithAnimation();
        MoveCharacterToSocket(character);
    }

    /// <summary>
    /// Shows the character select panel with a quick fade-in
    /// </summary>
    private void ShowPanelWithAnimation()
    {
        if (characterSelectPanel == null)
        {
            Debug.LogError("Character select panel not assigned!");
            return;
        }

        // Show panel
        characterSelectPanel.SetActive(true);

        // Fade in panel with CanvasGroup
        CanvasGroup panelCanvasGroup = characterSelectPanel.GetComponent<CanvasGroup>();
        if (panelCanvasGroup == null)
        {
            panelCanvasGroup = characterSelectPanel.AddComponent<CanvasGroup>();
        }

        panelCanvasGroup.alpha = 0f;
        panelCanvasGroup.DOFade(1f, panelFadeInDuration).SetEase(Ease.OutQuad);

        // Animate buttons in with stagger
        AnimateButtonsIn();
    }

    /// <summary>
    /// Persona 5-style button popup animation
    /// Quick, snappy, with rotation and overshoot
    /// </summary>
    private void AnimateButtonsIn()
    {
        for (int i = 0; i < allActionButtons.Count; i++)
        {
            if (allActionButtons[i] == null) continue;

            GameObject buttonObj = allActionButtons[i].gameObject;
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();

            // Store original rotation
            Vector3 originalRotation = buttonRect.localEulerAngles;

            // Start invisible, scaled down, and rotated
            buttonRect.localScale = Vector3.zero;
            buttonRect.localEulerAngles = originalRotation + new Vector3(0, 0, buttonRotationAmount);

            float delay = i * buttonStaggerDelay;

            // Pop in with scale
            buttonRect.DOScale(1f, buttonPopDuration)
                .SetDelay(delay)
                .SetEase(buttonPopEase);

            // Rotate back to normal
            buttonRect.DOLocalRotate(originalRotation, buttonPopDuration)
                .SetDelay(delay)
                .SetEase(buttonPopEase);
        }

        // Animate close button last
        if (closeButton != null)
        {
            GameObject closeObj = closeButton.gameObject;
            RectTransform closeRect = closeObj.GetComponent<RectTransform>();
            Vector3 originalRotation = closeRect.localEulerAngles;

            closeRect.localScale = Vector3.zero;
            closeRect.localEulerAngles = originalRotation + new Vector3(0, 0, -buttonRotationAmount);

            float delay = allActionButtons.Count * buttonStaggerDelay;

            closeRect.DOScale(1f, buttonPopDuration)
                .SetDelay(delay)
                .SetEase(buttonPopEase);

            closeRect.DOLocalRotate(originalRotation, buttonPopDuration)
                .SetDelay(delay)
                .SetEase(buttonPopEase);
        }
    }

    /// <summary>
    /// Moves the character to the socket position (temporarily, without affecting room data)
    /// </summary>
    private void MoveCharacterToSocket(CharacterObject character)
    {
        if (characterSocket == null)
        {
            Debug.LogError("Character socket not assigned!");
            return;
        }

        // Animate character to socket
        character.transform.DOMove(characterSocket.position, characterMoveDuration)
            .SetEase(characterMoveEase);

        // Optional: Scale up slightly for emphasis
        character.transform.DOScale(originalCharacterScale * 1.1f, characterMoveDuration)
            .SetEase(characterMoveEase);
    }

    /// <summary>
    /// Closes the menu and returns the character to their original position
    /// </summary>
    public void CloseMenu()
    {
        if (!isMenuOpen)
        {
            return;
        }

        // Animate out
        AnimatePanelOut(() =>
        {
            // After animation completes, restore character
            ReturnCharacterToOriginalPosition();
        });
    }

    /// <summary>
    /// Animates the panel fading out with buttons popping out
    /// </summary>
    private void AnimatePanelOut(System.Action onComplete)
    {
        if (characterSelectPanel == null)
        {
            onComplete?.Invoke();
            return;
        }

        // Animate buttons out (reverse order)
        for (int i = allActionButtons.Count - 1; i >= 0; i--)
        {
            if (allActionButtons[i] == null) continue;

            GameObject buttonObj = allActionButtons[i].gameObject;
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();

            float delay = (allActionButtons.Count - 1 - i) * buttonStaggerDelay * 0.5f;

            buttonRect.DOScale(0f, buttonPopDuration * 0.7f)
                .SetDelay(delay)
                .SetEase(Ease.InBack);

            buttonRect.DOLocalRotate(buttonRect.localEulerAngles + new Vector3(0, 0, -buttonRotationAmount), buttonPopDuration * 0.7f)
                .SetDelay(delay)
                .SetEase(Ease.InBack);
        }

        // Animate close button
        if (closeButton != null)
        {
            closeButton.transform.DOScale(0f, buttonPopDuration * 0.7f)
                .SetEase(Ease.InBack);
        }

        // Fade out panel
        CanvasGroup panelCanvasGroup = characterSelectPanel.GetComponent<CanvasGroup>();
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.DOFade(0f, panelFadeInDuration)
                .SetDelay(buttonPopDuration * 0.5f)
                .OnComplete(() =>
                {
                    characterSelectPanel.SetActive(false);
                    onComplete?.Invoke();
                });
        }
        else
        {
            characterSelectPanel.SetActive(false);
            onComplete?.Invoke();
        }
    }

    /// <summary>
    /// Returns the character to their original position without affecting room data
    /// </summary>
    private void ReturnCharacterToOriginalPosition()
    {
        if (currentExpandedCharacter == null)
        {
            ResetMenuState();
            return;
        }

        // Animate character back to original position
        currentExpandedCharacter.transform.DOMove(originalCharacterPosition, characterMoveDuration)
            .SetEase(characterMoveEase);

        // Scale back to normal
        currentExpandedCharacter.transform.DOScale(originalCharacterScale, characterMoveDuration)
            .SetEase(characterMoveEase)
            .OnComplete(() =>
            {
                ResetMenuState();
            });
    }

    /// <summary>
    /// Resets the menu state after closing
    /// </summary>
    private void ResetMenuState()
    {
        currentExpandedCharacter = null;
        currentRoomController = null;
        originalSocketIndex = -1;
        originalCharacterPosition = Vector3.zero;
        originalCharacterScale = Vector3.one;
        originalCharacterParent = null;
        isMenuOpen = false;
    }

    /// <summary>
    /// Checks if the menu is currently open
    /// </summary>
    public bool IsMenuOpen()
    {
        return isMenuOpen;
    }

    /// <summary>
    /// Gets the currently expanded character
    /// </summary>
    public CharacterObject GetCurrentExpandedCharacter()
    {
        return currentExpandedCharacter;
    }

    // ============================================
    // BUTTON ACTION HANDLERS
    // ============================================

    private void OnMoveToClicked()
    {
        Debug.Log($"Move To clicked for character: {currentExpandedCharacter?.GetCharacter()?.characterName ?? "None"}");

        if (UIController_Game.Instance != null)
        {
            UIController_Game.Instance.OnCharacterAction_MoveTo(currentExpandedCharacter);
        }

        CloseMenu();
    }

    private void OnPracticeClicked()
    {
        Debug.Log($"Practice clicked for character: {currentExpandedCharacter?.GetCharacter()?.characterName ?? "None"}");

        if (UIController_Game.Instance != null)
        {
            UIController_Game.Instance.OnCharacterAction_Practice(currentExpandedCharacter);
        }

        CloseMenu();
    }

    private void OnMusicClicked()
    {
        Debug.Log($"Music clicked for character: {currentExpandedCharacter?.GetCharacter()?.characterName ?? "None"}");

        if (UIController_Game.Instance != null)
        {
            UIController_Game.Instance.OnCharacterAction_Music(currentExpandedCharacter);
        }

        CloseMenu();
    }

    private void OnActionClicked()
    {
        Debug.Log($"Action clicked for character: {currentExpandedCharacter?.GetCharacter()?.characterName ?? "None"}");

        if (UIController_Game.Instance != null)
        {
            UIController_Game.Instance.OnCharacterAction_Action(currentExpandedCharacter);
        }

        CloseMenu();
    }

    private void OnDestroy()
    {
        // Clean up listeners
        if (closeButton != null && closeButton.onClick != null)
        {
            closeButton.onClick.RemoveListener(CloseMenu);
        }

        if (moveToButton != null && moveToButton.onClick != null)
        {
            moveToButton.onClick.RemoveAllListeners();
        }

        if (practiceButton != null && practiceButton.onClick != null)
        {
            practiceButton.onClick.RemoveAllListeners();
        }

        if (musicButton != null && musicButton.onClick != null)
        {
            musicButton.onClick.RemoveAllListeners();
        }

        if (actionButton != null && actionButton.onClick != null)
        {
            actionButton.onClick.RemoveAllListeners();
        }

        // Kill any ongoing tweens
        DOTween.Kill(transform);
        if (characterSelectPanel != null)
        {
            DOTween.Kill(characterSelectPanel.transform);
        }
    }
}
