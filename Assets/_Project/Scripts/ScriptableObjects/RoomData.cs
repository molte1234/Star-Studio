using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// RoomData - ScriptableObject that defines a room in the studio building.
/// 
/// WHAT IT DOES:
/// - Defines room properties (name, capacity, etc.)
/// - Tracks which characters are currently in this room
/// - Handles room locking during actions
/// - Manages room-wide action progress
/// 
/// USAGE:
/// - Create via: Assets > Create > Band Manager > Room Data
/// - Assign to GameManager's allRooms array
/// - Reference from CharacterSlotState.currentRoom
/// 
/// PHILOSOPHY:
/// Rooms are physical locations. Characters move between rooms.
/// When an action starts, it affects ALL characters in the room.
/// The room locks until the action completes.
/// </summary>
[CreateAssetMenu(fileName = "NewRoom", menuName = "Band Manager/Room Data")]
public class RoomData : ScriptableObject
{
    [Header("Room Identity")]
    [Tooltip("Display name shown in UI")]
    public string roomName = "New Room";

    [Tooltip("Short description of what this room does")]
    [TextArea(2, 4)]
    public string roomDescription = "";

    [Header("Room Rules")]
    [Tooltip("Maximum characters allowed in this room (0 = unlimited)")]
    public int maxCapacity = 6;

    [Tooltip("Can this room be locked during actions?")]
    public bool canBeLocked = true;

    [Tooltip("Can actions be started with 0 characters? (Management actions)")]
    public bool allowsZeroCharacterActions = false;

    // ============================================
    // RUNTIME STATE (not serialized - resets on play)
    // ============================================

    [System.NonSerialized]
    public List<CharacterSlotState> charactersPresent = new List<CharacterSlotState>();

    [System.NonSerialized]
    public bool isLocked = false;

    [System.NonSerialized]
    public ActionData currentAction = null;

    [System.NonSerialized]
    public float actionProgress = 0f; // 0.0 to 1.0

    [System.NonSerialized]
    public float actionTimeRemaining = 0f; // Seconds left

    [System.NonSerialized]
    public float actionTotalTime = 0f; // Total action duration

    // ============================================
    // ROOM STATE QUERIES
    // ============================================

    /// <summary>
    /// Can a character enter this room right now?
    /// </summary>
    public bool CanEnter()
    {
        // Room is locked during actions
        if (isLocked) return false;

        // Check capacity (0 = unlimited)
        if (maxCapacity > 0 && charactersPresent.Count >= maxCapacity)
            return false;

        return true;
    }

    /// <summary>
    /// Is this room currently empty?
    /// </summary>
    public bool IsEmpty()
    {
        return charactersPresent.Count == 0;
    }

    /// <summary>
    /// Is this room at full capacity?
    /// </summary>
    public bool IsFull()
    {
        if (maxCapacity == 0) return false; // Unlimited capacity
        return charactersPresent.Count >= maxCapacity;
    }

    /// <summary>
    /// Can an action be started in this room?
    /// </summary>
    public bool CanStartAction()
    {
        // Already running an action
        if (isLocked) return false;

        // Check if room allows 0-character actions
        if (charactersPresent.Count == 0 && !allowsZeroCharacterActions)
            return false;

        return true;
    }

    /// <summary>
    /// How many characters are currently in this room?
    /// </summary>
    public int CharacterCount()
    {
        return charactersPresent.Count;
    }

    // ============================================
    // CHARACTER MANAGEMENT
    // ============================================

    /// <summary>
    /// Add a character to this room.
    /// Does NOT remove them from their previous room - caller handles that.
    /// </summary>
    public void AddCharacter(CharacterSlotState character)
    {
        if (!charactersPresent.Contains(character))
        {
            charactersPresent.Add(character);
            Debug.Log($"✅ {character.slotData.displayName} entered {roomName} ({charactersPresent.Count}/{maxCapacity})");
        }
    }

    /// <summary>
    /// Remove a character from this room.
    /// </summary>
    public void RemoveCharacter(CharacterSlotState character)
    {
        if (charactersPresent.Contains(character))
        {
            charactersPresent.Remove(character);
            Debug.Log($"🚪 {character.slotData.displayName} left {roomName} ({charactersPresent.Count}/{maxCapacity})");
        }
    }

    /// <summary>
    /// Remove all characters from this room (usually when action completes).
    /// </summary>
    public void ClearAllCharacters()
    {
        Debug.Log($"🧹 Clearing all characters from {roomName}");
        charactersPresent.Clear();
    }

    // ============================================
    // ACTION MANAGEMENT
    // ============================================

    /// <summary>
    /// Start an action in this room.
    /// Locks the room and sets up action tracking.
    /// </summary>
    public void StartAction(ActionData action)
    {
        if (!CanStartAction())
        {
            Debug.LogWarning($"⚠️ Cannot start action in {roomName}!");
            return;
        }

        currentAction = action;
        actionTotalTime = action.baseTime;
        actionTimeRemaining = action.baseTime;
        actionProgress = 0f;
        isLocked = true;

        // Mark all characters in room as busy
        foreach (var character in charactersPresent)
        {
            character.isBusy = true;
        }

        Debug.Log($"▶️ Started action '{action.actionName}' in {roomName} with {charactersPresent.Count} characters");
    }

    /// <summary>
    /// Update the action timer. Call from GameManager.Update().
    /// Returns true if action completed this frame.
    /// </summary>
    public bool UpdateAction(float deltaTime)
    {
        if (!isLocked || currentAction == null) return false;

        actionTimeRemaining -= deltaTime;
        actionProgress = 1f - (actionTimeRemaining / actionTotalTime);

        // Action complete?
        if (actionTimeRemaining <= 0f)
        {
            CompleteAction();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Complete the current action.
    /// Unlocks room, applies rewards, returns characters to Breakroom.
    /// </summary>
    private void CompleteAction()
    {
        if (currentAction == null) return;

        Debug.Log($"✅ Action '{currentAction.actionName}' completed in {roomName}!");

        // Mark characters as no longer busy
        foreach (var character in charactersPresent)
        {
            character.isBusy = false;
        }

        // TODO: Apply action rewards to GameManager
        // GameManager.Instance.AddMoney(currentAction.moneyReward);
        // etc.

        // Reset room state
        currentAction = null;
        actionProgress = 0f;
        actionTimeRemaining = 0f;
        actionTotalTime = 0f;
        isLocked = false;

        // Characters auto-return to Breakroom
        // (This will be handled by GameManager, not here)
    }

    /// <summary>
    /// Cancel the current action.
    /// Unlocks room, no rewards, returns characters to Breakroom.
    /// </summary>
    public void CancelAction()
    {
        if (currentAction == null) return;

        Debug.Log($"❌ Action '{currentAction.actionName}' cancelled in {roomName}");

        // Mark characters as no longer busy
        foreach (var character in charactersPresent)
        {
            character.isBusy = false;
        }

        // Reset room state (no rewards)
        currentAction = null;
        actionProgress = 0f;
        actionTimeRemaining = 0f;
        actionTotalTime = 0f;
        isLocked = false;
    }

    // ============================================
    // INITIALIZATION
    // ============================================

    /// <summary>
    /// Reset runtime state. Call when starting a new game or loading a scene.
    /// </summary>
    public void ResetRoomState()
    {
        charactersPresent.Clear();
        isLocked = false;
        currentAction = null;
        actionProgress = 0f;
        actionTimeRemaining = 0f;
        actionTotalTime = 0f;

        Debug.Log($"🔄 {roomName} state reset");
    }
}