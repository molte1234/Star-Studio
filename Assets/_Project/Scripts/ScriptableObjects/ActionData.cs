using UnityEngine;

/// <summary>
/// ScriptableObject for action definitions (Practice, Tour, Record, etc.)
/// STUB VERSION - Will be expanded later with full action system
/// For now this just lets CharacterSlotState compile
/// </summary>
[CreateAssetMenu(fileName = "NewAction", menuName = "Band Manager/Action Data")]
public class ActionData : ScriptableObject
{
    [Header("Basic Info")]
    [Tooltip("Action name shown in UI")]
    public string actionName;

    [Tooltip("Description of what this action does")]
    [TextArea(3, 5)]
    public string actionDescription;

    // TODO: Add all the action properties later:
    // - requiresMembers, minMembers, maxMembers
    // - timeDuration, costs, rewards
    // - bonuses, etc.
    // See ACTION_SYSTEM_PLAN.md for full spec
}