using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;

/// <summary>
/// Room positioning direction for panel transitions
/// </summary>
public enum RoomDirection
{
    Top,
    Bottom,
    Left,
    Right
}

/// <summary>
/// RoomController - Manages character visuals and sockets for a single room
/// 
/// WHAT IT DOES:
/// - Tracks all socket positions in this room
/// - Tracks which characters are in which sockets
/// - Controls universal color tint for all characters in room
/// - Spawns/manages CharacterObject prefabs at sockets
/// - Handles panel slide transitions between rooms
/// 
/// USAGE:
/// - Attach one to each room panel/GameObject
/// - Manually assign socket transforms in inspector
/// - Set roomLightingTint for this room's lighting color
/// - Set roomDirection for where this room slides in from
/// </summary>
public class RoomController : MonoBehaviour
{
    // ============================================
    // ROOM IDENTITY
    // ============================================

    [Title("Room Info", bold: true)]
    [Tooltip("Reference to the RoomData ScriptableObject for this room")]
    public RoomData roomData;

    [Tooltip("Where this room positions itself when called (relative to center)")]
    public RoomDirection roomDirection = RoomDirection.Bottom;

    // ============================================
    // TRANSITION SETUP
    // ============================================

    [Title("Transitions", bold: true)]
    [Tooltip("Distance to offset room when positioning (e.g., screen height/width)")]
    public float transitionDistance = 1920f; // Default for 1080p height

    [Tooltip("Duration of slide animation in seconds")]
    public float transitionDuration = 0.5f;

    // ============================================
    // SOCKET SETUP
    // ============================================

    [Title("Sockets", bold: true)]
    [Tooltip("Array of socket transforms (empty GameObjects marking spawn positions)")]
    [InfoBox("Manually assign empty GameObjects as socket positions. Characters spawn here.")]
    public Transform[] sockets;

    [Tooltip("Per-socket size settings - defines the width and height each character should scale to")]
    [InfoBox("Set X = width and Y = height for each socket. Characters will scale to fit these dimensions while maintaining aspect ratio.")]
    public Vector2[] socketSizes;

    [Tooltip("Special socket for focused character view (when character menu is open)")]
    [InfoBox("Assign a Transform where the character will move to when clicked")]
    public Transform focusSocket;

    // ============================================
    // VISUAL SETUP
    // ============================================

    [Title("Visuals", bold: true)]
    [Tooltip("CharacterObject prefab to spawn at sockets")]
    public GameObject characterPrefab;

    [Tooltip("Room lighting tint applied to all characters in this room")]
    [ColorUsage(false)]
    public Color roomLightingTint = Color.white;

    [Title("Editor Testing", bold: true)]
    [Tooltip("Test character to populate sockets with (editor only)")]
    public SlotData testCharacter;

    // ============================================
    // RUNTIME STATE
    // ============================================

    // Why: Track which characters are in which sockets
    private CharacterSlotState[] socketOccupants;

    // Why: Track visual GameObjects (CharacterObject components)
    private CharacterObject[] characterVisuals;

    // ============================================
    // INITIALIZATION
    // ============================================

    void Awake()
    {
        // Why: Initialize arrays to match socket count
        if (sockets != null)
        {
            socketOccupants = new CharacterSlotState[sockets.Length];
            characterVisuals = new CharacterObject[sockets.Length];

            // Initialize socketSizes array if not set (default to 200x300)
            if (socketSizes == null || socketSizes.Length != sockets.Length)
            {
                socketSizes = new Vector2[sockets.Length];
                for (int i = 0; i < socketSizes.Length; i++)
                {
                    socketSizes[i] = new Vector2(200f, 300f); // Default size
                }
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ {gameObject.name}: No sockets assigned!");
        }
    }

    // ============================================
    // SOCKET QUERIES
    // ============================================

    /// <summary>
    /// Find first empty socket index (-1 if all full)
    /// </summary>
    public int FindEmptySocket()
    {
        if (socketOccupants == null) return -1;

        for (int i = 0; i < socketOccupants.Length; i++)
        {
            if (socketOccupants[i] == null)
            {
                return i;
            }
        }

        return -1; // All sockets full
    }

    /// <summary>
    /// Find which socket a specific character is in (-1 if not found)
    /// </summary>
    public int FindCharacterSocket(CharacterSlotState character)
    {
        if (socketOccupants == null || character == null) return -1;

        for (int i = 0; i < socketOccupants.Length; i++)
        {
            if (socketOccupants[i] == character)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Check if socket is occupied
    /// </summary>
    public bool IsSocketOccupied(int socketIndex)
    {
        if (!IsValidSocketIndex(socketIndex)) return false;
        return socketOccupants[socketIndex] != null;
    }

    /// <summary>
    /// Get character at specific socket (null if empty)
    /// </summary>
    public CharacterSlotState GetCharacterAtSocket(int socketIndex)
    {
        if (!IsValidSocketIndex(socketIndex)) return null;
        return socketOccupants[socketIndex];
    }

    // ============================================
    // SOCKET MANAGEMENT
    // ============================================

    /// <summary>
    /// Assign a character to a specific socket
    /// Spawns CharacterObject prefab and positions it
    /// </summary>
    public void AssignCharacterToSocket(int socketIndex, CharacterSlotState character)
    {
        if (!IsValidSocketIndex(socketIndex))
        {
            Debug.LogError($"❌ Invalid socket index: {socketIndex}");
            return;
        }

        if (character == null)
        {
            Debug.LogError("❌ Cannot assign null character to socket!");
            return;
        }

        if (socketOccupants[socketIndex] != null)
        {
            Debug.LogWarning($"⚠️ Socket {socketIndex} already occupied! Clearing first.");
            ClearSocket(socketIndex);
        }

        // Update socket occupancy
        socketOccupants[socketIndex] = character;

        // Spawn visual
        SpawnCharacterVisual(socketIndex, character);

        Debug.Log($"✅ {character.slotData.displayName} assigned to socket {socketIndex} in {roomData.roomName}");
    }

    /// <summary>
    /// Clear a socket (removes character and destroys visual)
    /// </summary>
    public void ClearSocket(int socketIndex)
    {
        if (!IsValidSocketIndex(socketIndex)) return;

        // Destroy visual GameObject
        if (characterVisuals[socketIndex] != null)
        {
            Destroy(characterVisuals[socketIndex].gameObject);
            characterVisuals[socketIndex] = null;
        }

        // Clear occupancy
        socketOccupants[socketIndex] = null;

        Debug.Log($"🧹 Socket {socketIndex} cleared");
    }

    /// <summary>
    /// Spawns CharacterObject prefab at socket position
    /// </summary>
    private void SpawnCharacterVisual(int socketIndex, CharacterSlotState character)
    {
        if (characterPrefab == null)
        {
            Debug.LogError("❌ No character prefab assigned!");
            return;
        }

        // Get socket transform
        Transform socketTransform = sockets[socketIndex];

        // Instantiate prefab at socket position
        GameObject instance = Instantiate(characterPrefab, socketTransform.position, Quaternion.identity, socketTransform);

        // Get CharacterObject component
        CharacterObject charObj = instance.GetComponent<CharacterObject>();
        if (charObj != null)
        {
            // Set character data
            charObj.SetCharacter(character);

            // Set room controller reference
            charObj.SetRoomController(this);

            // Apply room lighting
            charObj.SetRoomLighting(roomLightingTint);

            // Set transform values (anchors/pivot are defined in prefab)
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = new Vector3(1.1f, 1f, 1f);

            // Match width and height to socket's RectTransform
            RectTransform socketRect = socketTransform.GetComponent<RectTransform>();
            RectTransform charRect = instance.GetComponent<RectTransform>();
            if (socketRect != null && charRect != null)
            {
                charRect.sizeDelta = socketRect.sizeDelta;
            }

            // Store reference
            characterVisuals[socketIndex] = charObj;

            // Fade in
            charObj.FadeIn();
        }
        else
        {
            Debug.LogError("❌ CharacterObject component not found on prefab!");
        }
    }

    // ============================================
    // VISUAL CONTROLS
    // ============================================

    /// <summary>
    /// Update room lighting tint on all characters
    /// Called when roomLightingTint changes in inspector
    /// </summary>
    private void UpdateRoomLighting()
    {
        if (characterVisuals == null) return;

        for (int i = 0; i < characterVisuals.Length; i++)
        {
            if (characterVisuals[i] != null)
            {
                characterVisuals[i].SetRoomLighting(roomLightingTint);
            }
        }
    }

    // ============================================
    // UTILITY
    // ============================================

    /// <summary>
    /// Validate socket index is within bounds
    /// </summary>
    private bool IsValidSocketIndex(int index)
    {
        if (sockets == null || index < 0 || index >= sockets.Length)
        {
            Debug.LogError($"❌ Invalid socket index: {index} (valid range: 0-{sockets?.Length - 1})");
            return false;
        }
        return true;
    }

    /// <summary>
    /// Calculate the scale factor to fit a sprite within socket dimensions
    /// Maintains aspect ratio and scales to fit both width and height
    /// </summary>
    private float CalculateSocketScale(int socketIndex, Sprite characterSprite)
    {
        if (!IsValidSocketIndex(socketIndex) || characterSprite == null)
        {
            return 1f;
        }

        // Get the socket target size
        Vector2 targetSize = socketSizes[socketIndex];
        if (targetSize.x <= 0 || targetSize.y <= 0)
        {
            Debug.LogWarning($"⚠️ Socket {socketIndex} has invalid size: {targetSize}. Using scale 1.0");
            return 1f;
        }

        // Get the sprite's native size in pixels
        Rect spriteRect = characterSprite.rect;
        float spriteWidth = spriteRect.width;
        float spriteHeight = spriteRect.height;

        if (spriteWidth <= 0 || spriteHeight <= 0)
        {
            Debug.LogWarning($"⚠️ Character sprite has invalid dimensions: {spriteWidth}x{spriteHeight}");
            return 1f;
        }

        // Calculate scale factors for width and height
        float scaleX = targetSize.x / spriteWidth;
        float scaleY = targetSize.y / spriteHeight;

        // Use the smaller scale to ensure the sprite fits within both dimensions
        float finalScale = Mathf.Min(scaleX, scaleY);

        Debug.Log($"📏 Socket {socketIndex}: Target={targetSize}, Sprite={spriteWidth}x{spriteHeight}, Scale={finalScale:F3}");

        return finalScale;
    }

    /// <summary>
    /// Clear all sockets (for room reset)
    /// </summary>
    public void ClearAllSockets()
    {
        for (int i = 0; i < socketOccupants.Length; i++)
        {
            ClearSocket(i);
        }
        Debug.Log("🧹 All sockets cleared");
    }

    // ============================================
    // ROOM TRANSITIONS (Panel Slide Style)
    // ============================================

    /// <summary>
    /// Show this room with slide transition
    /// Called by UIController_Game when switching rooms
    /// </summary>
    /// <param name="previousRoom">Room that was previously shown (can be null)</param>
    public void ShowRoom(RoomController previousRoom)
    {
        Debug.Log($"🚪 ShowRoom: {roomData?.roomName ?? "UNKNOWN"}");

        // ============================================
        // ENABLE THIS ROOM
        // ============================================

        gameObject.SetActive(true);

        // ============================================
        // POSITION THIS ROOM OFF-SCREEN BASED ON DIRECTION
        // ============================================

        RectTransform thisRect = GetComponent<RectTransform>();
        Vector2 startPosition = Vector2.zero;

        switch (roomDirection)
        {
            case RoomDirection.Top:
                startPosition = new Vector2(0, transitionDistance);
                break;

            case RoomDirection.Bottom:
                startPosition = new Vector2(0, -transitionDistance);
                break;

            case RoomDirection.Left:
                startPosition = new Vector2(-transitionDistance, 0);
                break;

            case RoomDirection.Right:
                startPosition = new Vector2(transitionDistance, 0);
                break;
        }

        thisRect.anchoredPosition = startPosition;

        // ============================================
        // SLIDE THIS ROOM TO CENTER
        // ============================================

        thisRect.DOAnchorPos(Vector2.zero, transitionDuration).SetEase(Ease.OutCubic);

        // ============================================
        // SLIDE OLD ROOM OUT AND DISABLE IT
        // ============================================

        if (previousRoom != null)
        {
            RectTransform oldRect = previousRoom.GetComponent<RectTransform>();

            // Calculate opposite direction
            Vector2 exitDirection = -startPosition;

            // Slide out
            oldRect.DOAnchorPos(exitDirection, transitionDuration)
                .SetEase(Ease.OutCubic)
                .OnComplete(() => {
                    // Disable old room after animation completes
                    previousRoom.gameObject.SetActive(false);
                    Debug.Log($"⚫ Disabled: {previousRoom.roomData?.roomName ?? "UNKNOWN"}");
                });
        }

        Debug.Log($"✅ {roomData?.roomName ?? "UNKNOWN"} now visible");
    }

    /// <summary>
    /// Set this room as visible without animation (for initial setup)
    /// </summary>
    public void ShowRoomImmediate()
    {
        gameObject.SetActive(true);
        RectTransform thisRect = GetComponent<RectTransform>();
        thisRect.anchoredPosition = Vector2.zero;
        Debug.Log($"📍 {roomData?.roomName ?? "UNKNOWN"} set as visible (no animation)");
    }

    // ============================================
    // EDITOR HELPERS
    // ============================================

#if UNITY_EDITOR
    [Button("Populate All Sockets (Editor Test)", ButtonSizes.Large), PropertyOrder(-1)]
    [InfoBox("Spawns test character in all empty sockets. Works in editor without playing!")]
    private void EditorPopulateAllSockets()
    {
        if (testCharacter == null)
        {
            Debug.LogError("❌ No test character assigned! Drag a SlotData to 'Test Character' field.");
            return;
        }

        if (characterPrefab == null)
        {
            Debug.LogError("❌ No character prefab assigned!");
            return;
        }

        if (sockets == null || sockets.Length == 0)
        {
            Debug.LogError("❌ No sockets assigned!");
            return;
        }

        // Create runtime state wrapper for test character
        CharacterSlotState testState = new CharacterSlotState(testCharacter);

        int populatedCount = 0;

        // Populate empty sockets
        for (int i = 0; i < sockets.Length; i++)
        {
            if (sockets[i] != null)
            {
                // Spawn prefab
                GameObject instance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(characterPrefab, sockets[i]);

                if (instance != null)
                {
                    // Get CharacterObject component
                    CharacterObject charObj = instance.GetComponent<CharacterObject>();
                    if (charObj != null)
                    {
                        charObj.SetCharacter(testState);
                        charObj.SetRoomLighting(roomLightingTint);

                        // Set transform values (anchors/pivot are defined in prefab)
                        instance.transform.localPosition = Vector3.zero;
                        instance.transform.localRotation = Quaternion.identity;
                        instance.transform.localScale = new Vector3(1.1f, 1f, 1f);

                        // Match width and height to socket's RectTransform
                        RectTransform socketRect = sockets[i].GetComponent<RectTransform>();
                        RectTransform charRect = instance.GetComponent<RectTransform>();
                        if (socketRect != null && charRect != null)
                        {
                            charRect.sizeDelta = socketRect.sizeDelta;
                        }

                        populatedCount++;
                    }
                }
            }
        }

        Debug.Log($"✅ Populated {populatedCount} sockets with {testCharacter.displayName}");

        // Mark scene dirty so Unity knows to save changes
        UnityEditor.EditorUtility.SetDirty(gameObject);
    }

    [Button("Clear All Sockets"), PropertyOrder(-1)]
    private void EditorClearAllSockets()
    {
        if (sockets == null) return;

        // Find and destroy all child CharacterObject instances
        for (int i = 0; i < sockets.Length; i++)
        {
            if (sockets[i] != null)
            {
                // Destroy all children of socket
                while (sockets[i].childCount > 0)
                {
                    DestroyImmediate(sockets[i].GetChild(0).gameObject);
                }
            }
        }

        Debug.Log("🧹 All sockets cleared (editor mode)");
        UnityEditor.EditorUtility.SetDirty(gameObject);
    }
#endif
}