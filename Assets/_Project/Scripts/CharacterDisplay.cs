using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages a single character portrait display
/// Attach this to each band member portrait GameObject
/// </summary>
public class CharacterDisplay : MonoBehaviour
{
    [Header("References")]
    public Image portraitImage; // The UI Image that shows the character sprite

    /// <summary>
    /// Updates this display with a character from SlotData
    /// Automatically shows if character exists, hides if null
    /// </summary>
    /// <param name="slotData">The character data to display (null to hide)</param>
    public void SetCharacter(SlotData slotData)
    {
        if (slotData == null)
        {
            // Why: No character in this slot, hide it
            Clear();
            return;
        }

        // Why: We have a character, show it
        if (portraitImage != null)
        {
            portraitImage.sprite = slotData.sprite;
            portraitImage.enabled = true; // ✅ FIX: Enable the Image component instead of the whole GameObject
            Debug.Log($"✅ CharacterDisplay showing: {slotData.displayName}");
        }
        else
        {
            Debug.LogWarning("portraitImage is not assigned in CharacterDisplay Inspector!");
        }
    }

    /// <summary>
    /// Clears this display (hides it)
    /// </summary>
    public void Clear()
    {
        if (portraitImage != null)
        {
            portraitImage.enabled = false; // ✅ FIX: Disable the Image component instead of the whole GameObject
        }
        Debug.Log($"⚫ CharacterDisplay hidden (empty slot)");
    }
}