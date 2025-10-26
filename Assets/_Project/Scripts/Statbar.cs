using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Reusable stat bar component that displays any 0-X value as a filled bar
/// Works for 0-10 stats (Technical, Performance, Charisma) and 0-100 stats (Unity)
/// </summary>
public class StatBar : MonoBehaviour
{
    [Header("References")]
    public Image fillImage; // The UI Image component that will fill (set to Image Type: Filled)

    /// <summary>
    /// Updates the visual fill of the stat bar
    /// </summary>
    /// <param name="currentValue">Current stat value</param>
    /// <param name="maxValue">Maximum possible value for this stat</param>
    public void SetValue(int currentValue, int maxValue)
    {
        // Why: Convert any range (0-10 or 0-100) to fill amount (0.0-1.0)
        // Example: 7 out of 10 = 0.7 fill, 50 out of 100 = 0.5 fill
        float fillAmount = (float)currentValue / (float)maxValue;

        // Clamp to prevent values outside 0-1 range
        fillAmount = Mathf.Clamp01(fillAmount);

        fillImage.fillAmount = fillAmount;
    }
}