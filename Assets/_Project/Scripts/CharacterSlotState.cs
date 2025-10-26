using System.Collections.Generic;

/// <summary>
/// Runtime wrapper for character state
/// Separates the unchanging template data (SlotData) from gameplay state (busy, current action, etc.)
/// 
/// WHY: SlotData is a ScriptableObject (asset file), we can't change those at runtime
/// CharacterSlotState wraps the data and adds runtime info we CAN change
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
    /// Is this character currently doing an action?
    /// </summary>
    public bool isBusy = false;

    /// <summary>
    /// Which action are they currently doing? (null if not busy)
    /// </summary>
    public ActionData currentAction = null;

    /// <summary>
    /// How much time left on current action (in seconds)
    /// </summary>
    public float actionTimeRemaining = 0f;

    /// <summary>
    /// Which other character slots are in this action group with them?
    /// Example: If slots 0, 1, 2 are all touring together, each would have [0,1,2] here
    /// </summary>
    public List<int> groupedWithSlots = new List<int>();

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

    /// <summary>
    /// Start a new action for this character
    /// </summary>
    public void StartAction(ActionData action, float duration, List<int> group)
    {
        isBusy = true;
        currentAction = action;
        actionTimeRemaining = duration;
        groupedWithSlots = new List<int>(group); // Make a copy of the list
    }

    /// <summary>
    /// Complete the current action and reset to available
    /// </summary>
    public void CompleteAction()
    {
        isBusy = false;
        currentAction = null;
        actionTimeRemaining = 0f;
        groupedWithSlots.Clear();
    }

    /// <summary>
    /// Cancel the current action (same as complete, but might have different logic later)
    /// </summary>
    public void CancelAction()
    {
        CompleteAction(); // For now, same as complete
    }
}