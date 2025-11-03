using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;

/// <summary>
/// CharacterObject - Visual representation of a character in a room socket
/// 
/// WHAT IT DOES:
/// - Displays full-body character sprite with shadow
/// - Handles mouse hover/click interaction (borrowed from CharacterDisplay pattern)
/// - Applies VFX (HSV lighting, future effects)
/// - Manages fade in/out transitions
/// - Tracks which character this represents
/// 
/// HIERARCHY:
/// CharacterObject (root with this script + CanvasGroup)
///   â"œâ"€ CharacterImage (Image component)
///   â""â"€ ShadowImage (Image component, offset below)
/// 
/// USAGE:
/// - Attach to character prefab root
/// - Assign character/shadow image references
/// - Set myCharacter reference when spawning
/// - applyVFX toggle for editor preview
/// </summary>
public class CharacterObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    // ============================================
    // INSPECTOR REFERENCES
    // ============================================

    [Title("Character References", bold: true)]
    [Tooltip("The character data this visual represents")]
    [ShowInInspector, ReadOnly]
    public CharacterSlotState myCharacter;

    [FoldoutGroup("Visual Components")]
    [Required, Tooltip("Full-body character sprite")]
    public Image characterImage;

    [FoldoutGroup("Visual Components")]
    [Required, Tooltip("Shadow sprite (positioned below character)")]
    public Image shadowImage;

    [FoldoutGroup("Visual Components")]
    [Tooltip("CanvasGroup for fade in/out (auto-assigned if not set)")]
    public CanvasGroup canvasGroup;

    // ============================================
    // VFX SETTINGS
    // ============================================

    [Title("Visual Effects", bold: true)]
    [ToggleLeft, OnValueChanged("OnVFXToggled")]
    [Tooltip("Apply VFX (color tint, future effects) - toggle for editor preview")]
    public bool applyVFX = true;

    [FoldoutGroup("Room Lighting")]
    [Tooltip("Color tint from room lighting (will be set by RoomData/Socket)")]
    public Color roomLightingTint = Color.white;

    // ============================================
    // SHADOW SETTINGS
    // ============================================

    [FoldoutGroup("Shadow")]
    [Tooltip("Shadow offset from character position")]
    public Vector2 shadowOffset = new Vector2(0f, -20f);

    // ============================================
    // INTERACTION SETTINGS (borrowed from CharacterDisplay)
    // ============================================

    [Title("Mouse Interaction", bold: true)]
    [Tooltip("Color multiplier when hovering")]
    public Color hoverTintColor = new Color(1.2f, 1.2f, 1.2f, 1f);

    [Tooltip("Color tint when selected")]
    public Color selectedTintColor = new Color(0.5f, 1f, 1f, 1f);

    [Tooltip("Scale multiplier on hover")]
    [Range(1.0f, 1.2f)]
    public float hoverScaleMultiplier = 1.05f;

    // ============================================
    // PRIVATE STATE
    // ============================================

    private Color originalCharacterColor = Color.white;
    private Vector3 originalScale = Vector3.one;

    // Interaction state
    private bool isHovered = false;
    private bool isSelected = false;
    private bool isBusy = false;

    // ============================================
    // INITIALIZATION
    // ============================================

    void Awake()
    {
        // Auto-assign CanvasGroup if not set
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        // Store original values
        originalScale = transform.localScale;
        if (characterImage != null)
        {
            originalCharacterColor = characterImage.color;
        }

        // Position shadow
        UpdateShadowPosition();
    }

    void Start()
    {
        // Apply initial VFX
        if (applyVFX)
        {
            ApplyRoomLighting();
        }
    }

    // ============================================
    // PUBLIC API
    // ============================================

    /// <summary>
    /// Set which character this visual represents
    /// Called when spawning at a socket
    /// </summary>
    public void SetCharacter(CharacterSlotState character)
    {
        myCharacter = character;

        // Update sprite
        if (characterImage != null && character != null && character.slotData != null)
        {
            characterImage.sprite = character.slotData.sprite;
            Debug.Log($"✅ CharacterObject set to display: {character.slotData.displayName}");
        }
    }

    /// <summary>
    /// Set room lighting tint from RoomData or Socket
    /// </summary>
    public void SetRoomLighting(Color lightingTint)
    {
        roomLightingTint = lightingTint;

        if (applyVFX)
        {
            ApplyRoomLighting();
        }
    }

    /// <summary>
    /// Fade in (when entering room)
    /// </summary>
    public void FadeIn(float duration = 0.3f)
    {
        if (canvasGroup != null)
        {
            // TODO: Use DOTween for smooth fade
            // For now, instant
            canvasGroup.alpha = 1f;
        }
    }

    /// <summary>
    /// Fade out (when leaving room)
    /// </summary>
    public void FadeOut(float duration = 0.3f)
    {
        if (canvasGroup != null)
        {
            // TODO: Use DOTween for smooth fade
            // For now, instant
            canvasGroup.alpha = 0f;
        }
    }

    /// <summary>
    /// Set busy state (affects visual tint)
    /// </summary>
    public void SetBusyState(bool busy)
    {
        isBusy = busy;
        UpdateCharacterColor();
    }

    /// <summary>
    /// Set selection state (called by UIController or room manager)
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateCharacterColor();
    }

    // ============================================
    // MOUSE INTERACTION (Pattern from CharacterDisplay)
    // ============================================

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        UpdateCharacterColor();

        // Scale up slightly
        transform.localScale = originalScale * hoverScaleMultiplier;

        // TODO: Notify UIController or RoomManager
        // (Once we have room system hooked up)

        Debug.Log($"🖱️ Hover: {(myCharacter != null ? myCharacter.slotData.displayName : "Unknown")}");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdateCharacterColor();

        // Scale back
        transform.localScale = originalScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // TODO: Notify UIController or RoomManager for selection
        // For now, just log
        Debug.Log($"🖱️ Click: {(myCharacter != null ? myCharacter.slotData.displayName : "Unknown")}");
    }

    // ============================================
    // VISUAL UPDATES
    // ============================================

    /// <summary>
    /// Update character image color based on state
    /// Priority: Busy > Selected > Hover > Normal (with room lighting)
    /// </summary>
    private void UpdateCharacterColor()
    {
        if (characterImage == null) return;

        // Priority 1: Busy state
        if (isBusy)
        {
            Color busyColor = originalCharacterColor * new Color(0.7f, 0.7f, 0.7f, 1f);
            characterImage.color = busyColor;
        }
        // Priority 2: Selected
        else if (isSelected)
        {
            characterImage.color = selectedTintColor;
        }
        // Priority 3: Hover
        else if (isHovered)
        {
            Color hoverColor = originalCharacterColor * hoverTintColor;
            characterImage.color = hoverColor;
        }
        // Priority 4: Normal - apply room lighting
        else
        {
            ApplyRoomLighting();
        }
    }

    /// <summary>
    /// Apply room lighting tint to character image
    /// Only applies if character is not busy/selected/hovered
    /// </summary>
    private void ApplyRoomLighting()
    {
        if (characterImage == null || !applyVFX) return;

        // Only apply room lighting if in neutral state
        // (hover/select/busy states override this)
        if (!isHovered && !isSelected && !isBusy)
        {
            Color tintedColor = originalCharacterColor * roomLightingTint;
            characterImage.color = tintedColor;
        }
    }

    /// <summary>
    /// Update shadow position relative to character
    /// </summary>
    private void UpdateShadowPosition()
    {
        if (shadowImage == null) return;

        // Shadow is child of this GameObject, so position relative to parent
        shadowImage.rectTransform.anchoredPosition = shadowOffset;
    }

    // ============================================
    // EDITOR HELPERS
    // ============================================

#if UNITY_EDITOR
    /// <summary>
    /// Called when applyVFX is toggled in editor (Odin OnValueChanged)
    /// </summary>
    private void OnVFXToggled()
    {
        if (characterImage != null)
        {
            if (applyVFX)
            {
                ApplyRoomLighting();
            }
            else
            {
                // Reset to original color when VFX disabled
                characterImage.color = originalCharacterColor;
            }
        }
    }

    void OnValidate()
    {
        // Update shadow position
        if (shadowImage != null)
        {
            UpdateShadowPosition();
        }

        // Update room lighting when color changes
        if (applyVFX && characterImage != null)
        {
            ApplyRoomLighting();
        }
    }

    [Button("Test Fade In"), FoldoutGroup("Editor Testing")]
    private void TestFadeIn()
    {
        FadeIn();
    }

    [Button("Test Fade Out"), FoldoutGroup("Editor Testing")]
    private void TestFadeOut()
    {
        FadeOut();
    }
#endif
}