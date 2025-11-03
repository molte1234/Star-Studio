using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;

/// <summary>
/// Room positioning direction for comic panel transitions
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
/// - Handles comic-style panel transitions between rooms
/// 
/// USAGE:
/// - Attach one to each room panel/GameObject
/// - Manually assign socket transforms in inspector
/// - Set roomLightingTint for this room's lighting color
/// - Set roomDirection for where this room slides in from
/// - Assign horizontal/vertical gutter images for transitions
/// 
/// NOTE:
/// - Alpha is controlled per-character via CharacterObject.FadeIn/FadeOut
/// - Not controlled at room level
/// 
/// HIERARCHY:
/// Room GameObject (this script)
///   ├─ Socket_01 (empty Transform)
///   ├─ Socket_02 (empty Transform)
///   └─ Socket_03 (empty Transform)
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
    [Tooltip("Horizontal comic gutter image (appears between vertical transitions)")]
    public GameObject horizontalGutter;

    [Tooltip("Vertical comic gutter image (appears between horizontal transitions)")]
    public GameObject verticalGutter;

    [Tooltip("Distance to offset room when positioning (e.g., screen height/width)")]
    public float transitionDistance = 1920f; // Default for 1080p height

    // Track current centered room (static so all rooms know which is active)
    private static RoomController currentCenteredRoom = null;

    // ============================================
    // SOCKET SETUP
    // ============================================

    [Title("Sockets", bold: true)]
    [Tooltip("Array of socket transforms (empty GameObjects marking spawn positions)")]
    [InfoBox("Manually assign empty GameObjects as socket positions. Characters spawn here.")]
    public Transform[] sockets;

    [Tooltip("Flip character horizontally at each socket (faces opposite direction)")]
    [InfoBox("Check boxes to flip characters at specific sockets (scale.x = -1)")]
    public bool[] socketFlipX;

    // ============================================
    // CHARACTER TRACKING
    // ============================================

    [Title("Character Tracking", bold: true)]
    [ShowInInspector, ReadOnly, ListDrawerSettings(Expanded = true)]
    [Tooltip("Visual display of which character is in each socket")]
    private string[] SocketOccupancyDisplay
    {
        get
        {
            if (socketOccupants == null || sockets == null) return new string[0];

            string[] display = new string[socketOccupants.Length];
            for (int i = 0; i < socketOccupants.Length; i++)
            {
                if (socketOccupants[i] != null && socketOccupants[i].slotData != null)
                {
                    display[i] = $"Socket {i}: {socketOccupants[i].slotData.displayName}";
                }
                else
                {
                    display[i] = $"Socket {i}: EMPTY";
                }
            }
            return display;
        }
    }

    // Internal tracking arrays
    private CharacterSlotState[] socketOccupants;
    private CharacterObject[] characterVisuals;

    [Required, Tooltip("CharacterObject prefab to spawn")]
    public GameObject characterPrefab;

    [Title("Editor Testing", bold: true)]
    [Tooltip("Test character to use for populating sockets in editor")]
    public SlotData testCharacter;

    // ============================================
    // VISUAL SETTINGS
    // ============================================

    [Title("Room Lighting", bold: true)]
    [Tooltip("Universal color tint applied to all characters in this room")]
    [OnValueChanged("UpdateRoomLighting")]
    public Color roomLightingTint = Color.white;

    // ============================================
    // INITIALIZATION
    // ============================================

    void Awake()
    {
        // Initialize arrays based on socket count
        if (sockets != null)
        {
            int socketCount = sockets.Length;
            socketOccupants = new CharacterSlotState[socketCount];
            characterVisuals = new CharacterObject[socketCount];

            // Initialize flip array if needed
            if (socketFlipX == null || socketFlipX.Length != socketCount)
            {
                socketFlipX = new bool[socketCount];
            }

            Debug.Log($"🏠 RoomController initialized: {socketCount} sockets");
        }
        else
        {
            Debug.LogError("❌ RoomController: No sockets assigned!");
        }
    }

    // ============================================
    // CHARACTER MANAGEMENT
    // ============================================

    /// <summary>
    /// Assign a character to a specific socket
    /// Spawns CharacterObject prefab at socket position
    /// </summary>
    public void AssignCharacterToSocket(int socketIndex, CharacterSlotState character)
    {
        if (!IsValidSocketIndex(socketIndex)) return;

        // Check if socket already occupied
        if (socketOccupants[socketIndex] != null)
        {
            Debug.LogWarning($"⚠️ Socket {socketIndex} already occupied by {socketOccupants[socketIndex].slotData.displayName}!");
            return;
        }

        // Track character in socket
        socketOccupants[socketIndex] = character;

        // Spawn visual
        SpawnCharacterVisual(socketIndex, character);

        Debug.Log($"✅ Assigned {character.slotData.displayName} to socket {socketIndex}");
    }

    /// <summary>
    /// Remove character from socket
    /// Destroys CharacterObject prefab
    /// </summary>
    public void ClearSocket(int socketIndex)
    {
        if (!IsValidSocketIndex(socketIndex)) return;

        // Destroy visual if exists
        if (characterVisuals[socketIndex] != null)
        {
            Destroy(characterVisuals[socketIndex].gameObject);
            characterVisuals[socketIndex] = null;
        }

        // Clear tracking
        CharacterSlotState clearedCharacter = socketOccupants[socketIndex];
        socketOccupants[socketIndex] = null;

        if (clearedCharacter != null)
        {
            Debug.Log($"🧹 Cleared {clearedCharacter.slotData.displayName} from socket {socketIndex}");
        }
    }

    /// <summary>
    /// Find which socket a character is in
    /// Returns -1 if not found
    /// </summary>
    public int FindCharacterSocket(CharacterSlotState character)
    {
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
    /// Find first empty socket
    /// Returns -1 if all full
    /// </summary>
    public int FindEmptySocket()
    {
        for (int i = 0; i < socketOccupants.Length; i++)
        {
            if (socketOccupants[i] == null)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Is this socket currently occupied?
    /// </summary>
    public bool IsSocketOccupied(int socketIndex)
    {
        if (!IsValidSocketIndex(socketIndex)) return false;
        return socketOccupants[socketIndex] != null;
    }

    // ============================================
    // VISUAL SPAWNING
    // ============================================

    /// <summary>
    /// Spawn CharacterObject prefab at socket position
    /// </summary>
    private void SpawnCharacterVisual(int socketIndex, CharacterSlotState character)
    {
        if (characterPrefab == null)
        {
            Debug.LogError("❌ RoomController: No characterPrefab assigned!");
            return;
        }

        Transform socketTransform = sockets[socketIndex];

        // Instantiate prefab
        GameObject instance = Instantiate(characterPrefab);

        // Parent to socket FIRST
        instance.transform.SetParent(socketTransform, false);

        // Reset local transform
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        // Apply flip if needed
        if (socketFlipX != null && socketIndex < socketFlipX.Length && socketFlipX[socketIndex])
        {
            Vector3 flippedScale = instance.transform.localScale;
            flippedScale.x *= -1f;
            instance.transform.localScale = flippedScale;
        }

        // Copy socket RectTransform size to character
        RectTransform socketRect = socketTransform.GetComponent<RectTransform>();
        RectTransform charRect = instance.GetComponent<RectTransform>();
        if (socketRect != null && charRect != null)
        {
            charRect.sizeDelta = socketRect.sizeDelta; // Match socket size!
        }

        // Get CharacterObject component
        CharacterObject charObj = instance.GetComponent<CharacterObject>();
        if (charObj != null)
        {
            // Set character data
            charObj.SetCharacter(character);

            // Apply room lighting
            charObj.SetRoomLighting(roomLightingTint);

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
    // ROOM TRANSITIONS (Comic Panel Style)
    // ============================================

    /// <summary>
    /// Transition from current centered room to this room
    /// Positions this room in specified direction, then slides both panels
    /// </summary>
    public void TransitionToThisRoom(float duration = 0.5f)
    {
        // If this is already centered, do nothing
        if (currentCenteredRoom == this)
        {
            Debug.Log($"ℹ️ {roomData.roomName} already centered");
            return;
        }

        RoomController oldRoom = currentCenteredRoom;

        Debug.Log($"🎬 Transitioning: {oldRoom?.roomData.roomName ?? "NONE"} → {roomData.roomName}");

        // ============================================
        // 1. POSITION THIS ROOM IN SPECIFIED DIRECTION
        // ============================================

        RectTransform thisRect = GetComponent<RectTransform>();
        Vector2 startPosition = Vector2.zero;
        Vector2 slideDirection = Vector2.zero;
        bool useHorizontalGutter = false;

        switch (roomDirection)
        {
            case RoomDirection.Top:
                startPosition = new Vector2(0, transitionDistance);
                slideDirection = Vector2.down;
                useHorizontalGutter = true;
                break;

            case RoomDirection.Bottom:
                startPosition = new Vector2(0, -transitionDistance);
                slideDirection = Vector2.up;
                useHorizontalGutter = true;
                break;

            case RoomDirection.Left:
                startPosition = new Vector2(-transitionDistance, 0);
                slideDirection = Vector2.right;
                useHorizontalGutter = false;
                break;

            case RoomDirection.Right:
                startPosition = new Vector2(transitionDistance, 0);
                slideDirection = Vector2.left;
                useHorizontalGutter = false;
                break;
        }

        thisRect.anchoredPosition = startPosition;

        // ============================================
        // 2. POSITION & SHOW GUTTER IMAGE
        // ============================================

        GameObject activeGutter = useHorizontalGutter ? horizontalGutter : verticalGutter;
        if (activeGutter != null)
        {
            activeGutter.SetActive(true);
            RectTransform gutterRect = activeGutter.GetComponent<RectTransform>();

            // Position gutter between current center and this room
            gutterRect.anchoredPosition = startPosition * 0.5f; // Halfway between

            // TODO: Make gutter follow during slide animation
        }

        // ============================================
        // 3. SLIDE BOTH PANELS (using DOTween)
        // ============================================

        // Slide this room to center
        thisRect.DOAnchorPos(Vector2.zero, duration).SetEase(Ease.OutCubic);

        // Slide old room out
        if (oldRoom != null)
        {
            RectTransform oldRect = oldRoom.GetComponent<RectTransform>();
            Vector2 oldTargetPosition = slideDirection * transitionDistance;
            oldRect.DOAnchorPos(oldTargetPosition, duration).SetEase(Ease.OutCubic);
        }

        // Hide gutter after animation
        DOVirtual.DelayedCall(duration + 0.1f, () => {
            if (activeGutter != null) activeGutter.SetActive(false);
        });

        // ============================================
        // 4. UPDATE CENTERED ROOM TRACKING
        // ============================================

        currentCenteredRoom = this;

        Debug.Log($"✅ Transition complete: {roomData.roomName} now centered");

        // TODO: Hide gutter after animation completes
    }

    /// <summary>
    /// Set this room as the initial centered room (no animation)
    /// Call this for the starting room (usually Breakroom)
    /// </summary>
    public void SetAsCenteredRoom()
    {
        RectTransform thisRect = GetComponent<RectTransform>();
        thisRect.anchoredPosition = Vector2.zero;

        currentCenteredRoom = this;

        Debug.Log($"📍 {roomData.roomName} set as centered room");
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

        // Initialize arrays if needed
        if (socketOccupants == null || socketOccupants.Length != sockets.Length)
        {
            socketOccupants = new CharacterSlotState[sockets.Length];
            characterVisuals = new CharacterObject[sockets.Length];
        }

        int populatedCount = 0;

        // Populate all empty sockets
        for (int i = 0; i < sockets.Length; i++)
        {
            if (socketOccupants[i] == null) // Only fill empty sockets
            {
                // Create CharacterSlotState from test character
                CharacterSlotState testState = new CharacterSlotState(testCharacter);

                // Spawn visual
                Transform socketTransform = sockets[i];
                GameObject instance = UnityEditor.PrefabUtility.InstantiatePrefab(characterPrefab) as GameObject;

                if (instance != null)
                {
                    // Parent to socket
                    instance.transform.SetParent(socketTransform, false);

                    // Reset local transform
                    instance.transform.localPosition = Vector3.zero;
                    instance.transform.localRotation = Quaternion.identity;
                    instance.transform.localScale = Vector3.one;

                    // Apply flip if needed
                    if (socketFlipX != null && i < socketFlipX.Length && socketFlipX[i])
                    {
                        Vector3 flippedScale = instance.transform.localScale;
                        flippedScale.x *= -1f;
                        instance.transform.localScale = flippedScale;
                    }

                    // Copy socket RectTransform size to character
                    RectTransform socketRect = socketTransform.GetComponent<RectTransform>();
                    RectTransform charRect = instance.GetComponent<RectTransform>();
                    if (socketRect != null && charRect != null)
                    {
                        charRect.sizeDelta = socketRect.sizeDelta; // Match socket size!
                    }

                    CharacterObject charObj = instance.GetComponent<CharacterObject>();
                    if (charObj != null)
                    {
                        charObj.SetCharacter(testState);
                        charObj.SetRoomLighting(roomLightingTint); // Apply room tint!

                        socketOccupants[i] = testState;
                        characterVisuals[i] = charObj;

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
        if (characterVisuals == null) return;

        for (int i = 0; i < characterVisuals.Length; i++)
        {
            if (characterVisuals[i] != null)
            {
                DestroyImmediate(characterVisuals[i].gameObject);
                characterVisuals[i] = null;
            }
        }

        if (socketOccupants != null)
        {
            for (int i = 0; i < socketOccupants.Length; i++)
            {
                socketOccupants[i] = null;
            }
        }

        Debug.Log("🧹 All sockets cleared (editor mode)");
        UnityEditor.EditorUtility.SetDirty(gameObject);
    }

    [Button("Test Transition To This Room"), PropertyOrder(-1)]
    [InfoBox("Preview full transition animation in editor (uses DOTween)")]
    private void EditorTestTransition()
    {
        if (Application.isPlaying)
        {
            // In play mode, use the real transition
            TransitionToThisRoom();
            return;
        }

        // EDITOR MODE PREVIEW
        Debug.Log($"🎬 Testing transition to {roomData?.roomName ?? "this room"}");

        RectTransform thisRect = GetComponent<RectTransform>();

        // Find a room to simulate as "current centered" (any room at center position)
        RoomController simulatedOldRoom = null;
        RoomController[] allRooms = FindObjectsOfType<RoomController>();
        foreach (var room in allRooms)
        {
            if (room != this && room.GetComponent<RectTransform>().anchoredPosition == Vector2.zero)
            {
                simulatedOldRoom = room;
                break;
            }
        }

        // Calculate positions
        Vector2 startPosition = Vector2.zero;
        Vector2 slideDirection = Vector2.zero;
        bool useHorizontalGutter = false;

        switch (roomDirection)
        {
            case RoomDirection.Top:
                startPosition = new Vector2(0, transitionDistance);
                slideDirection = Vector2.down;
                useHorizontalGutter = true;
                break;
            case RoomDirection.Bottom:
                startPosition = new Vector2(0, -transitionDistance);
                slideDirection = Vector2.up;
                useHorizontalGutter = true;
                break;
            case RoomDirection.Left:
                startPosition = new Vector2(-transitionDistance, 0);
                slideDirection = Vector2.right;
                useHorizontalGutter = false;
                break;
            case RoomDirection.Right:
                startPosition = new Vector2(transitionDistance, 0);
                slideDirection = Vector2.left;
                useHorizontalGutter = false;
                break;
        }

        // Position this room at start
        thisRect.anchoredPosition = startPosition;

        // Show appropriate gutter
        if (horizontalGutter != null && useHorizontalGutter)
        {
            horizontalGutter.SetActive(true);
            horizontalGutter.GetComponent<RectTransform>().anchoredPosition = startPosition * 0.5f;
        }
        if (verticalGutter != null && !useHorizontalGutter)
        {
            verticalGutter.SetActive(true);
            verticalGutter.GetComponent<RectTransform>().anchoredPosition = startPosition * 0.5f;
        }

        // ANIMATE using DOTween (works in edit mode!)
#if UNITY_EDITOR
        DOTween.Init();
        DOTween.defaultAutoPlay = AutoPlay.All;

        // Slide this room to center
        var thisTween = thisRect.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutCubic);
        thisTween.SetUpdate(true); // Works in edit mode

        // Slide old room out (if exists)
        if (simulatedOldRoom != null)
        {
            RectTransform oldRect = simulatedOldRoom.GetComponent<RectTransform>();
            Vector2 oldTargetPosition = slideDirection * transitionDistance;
            var oldTween = oldRect.DOAnchorPos(oldTargetPosition, 0.5f).SetEase(Ease.OutCubic);
            oldTween.SetUpdate(true);

            Debug.Log($"   Sliding out: {simulatedOldRoom.roomData?.roomName ?? "old room"}");
        }

        // Hide gutter after animation
        if (useHorizontalGutter && horizontalGutter != null)
        {
            DOVirtual.DelayedCall(0.6f, () => {
                if (horizontalGutter != null) horizontalGutter.SetActive(false);
            }).SetUpdate(true);
        }
        if (!useHorizontalGutter && verticalGutter != null)
        {
            DOVirtual.DelayedCall(0.6f, () => {
                if (verticalGutter != null) verticalGutter.SetActive(false);
            }).SetUpdate(true);
        }

        UnityEditor.EditorUtility.SetDirty(gameObject);
        if (simulatedOldRoom != null) UnityEditor.EditorUtility.SetDirty(simulatedOldRoom.gameObject);
#endif
    }

    [Button("Reset To Center"), PropertyOrder(-1)]
    private void EditorResetToCenter()
    {
        RectTransform thisRect = GetComponent<RectTransform>();
        thisRect.anchoredPosition = Vector2.zero;

        // Hide gutters
        if (horizontalGutter != null) horizontalGutter.SetActive(false);
        if (verticalGutter != null) verticalGutter.SetActive(false);

        UnityEditor.EditorUtility.SetDirty(gameObject);
    }
#endif
}