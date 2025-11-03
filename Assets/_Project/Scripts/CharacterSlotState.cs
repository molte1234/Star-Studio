using System.Collections.Generic;

/// <summary>
/// Runtime wrapper for character state
/// Separates the unchanging template data (SlotData) from gameplay state (busy, current action, etc.)
/// 
/// WHY: SlotData is a ScriptableObject (asset file), we can't change those at runtime
/// CharacterSlotState wraps the data and adds runtime info we CAN change
/// 
/// UPDATED: Room-based system - tracks which room character is in
/// </summary>
public class CharacterSlotState
{
    // ============================================
    // REFERENCE TO TEMPLATE DATA
    // ============================================

    /// <summary>
    /// The ScriptableObject template - this is the CHARACTER DATA (stats, name, sprite)
    /// </summary>
    public SlotData slotData;

    // ============================================
    // RUNTIME STATE (changes during gameplay)
    // ============================================

    /// <summary>
    /// Which room is this character currently in? (null if not in any room yet)
    /// </summary>
    public RoomData currentRoom = null;

    /// <summary>
    /// Is this character currently doing an action?
    /// </summary>
    public bool isBusy = false;

    /// <summary>
    /// Which action are they currently doing? (null if not busy)
    /// </summary>
    public ActionData currentAction = null;

    /// <summary>
    /// How much time left on current action (in seconds)
    /// Counts down from actionTotalDuration to 0
    /// </summary>
    public float actionTimeRemaining = 0f;

    /// <summary>
    /// Total duration of current action (accounts for stat bonuses)
    /// Used for accurate progress bar calculation
    /// Example: baseTime=30s, but with bonuses this might be 20s
    /// </summary>
    public float actionTotalDuration = 0f;

    // ============================================
    // CONSTRUCTOR
    // ============================================

    /// <summary>
    /// Create a new runtime state for a character
    /// </summary>
    public CharacterSlotState(SlotData data)
    {
        slotData = data;
    }

    // ============================================
    // HELPER METHODS
    // ============================================

    /// <summary>
    /// Is this character available to start a new action?
    /// </summary>
    public bool IsAvailable()
    {
        return !isBusy;
    }
}