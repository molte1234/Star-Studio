using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Makes UI Image only clickable on opaque pixels, transparent areas are ignored
/// Just drag this onto any GameObject with an Image component
/// </summary>
[RequireComponent(typeof(Image))]
public class AlphaRaycast : MonoBehaviour
{
    [Header("Alpha Threshold")]
    [Tooltip("Pixels below this opacity are not clickable (0 = all clickable, 1 = only fully opaque)")]
    [Range(0f, 1f)]
    public float minimumAlpha = 0.1f;

    void Awake()
    {
        Image img = GetComponent<Image>();
        if (img != null)
        {
            img.alphaHitTestMinimumThreshold = minimumAlpha;
            Debug.Log($"✅ AlphaRaycast enabled on {gameObject.name} (threshold: {minimumAlpha})");
        }
        else
        {
            Debug.LogError($"❌ AlphaRaycast: No Image component found on {gameObject.name}!");
        }
    }
}