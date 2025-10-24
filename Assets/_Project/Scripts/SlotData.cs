using UnityEngine;

[CreateAssetMenu(fileName = "NewSlot", menuName = "Band Manager/Slot Data")]
public class SlotData : ScriptableObject
{
    [Header("Basic Info")]
    public string displayName;
    public Sprite sprite;
    public SlotType slotType;

    [Header("Stats")]
    public int technical;
    public int performance;
    public int charisma;

    [Header("Character Specific")]
    public int morale = 5; // Only for characters
    public string trait; // One-liner description

    [Header("Support Specific")]
    public int hireCost; // Only for support characters
}

public enum SlotType
{
    MainMember,
    Support,
    Equipment
}