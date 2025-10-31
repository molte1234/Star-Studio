using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

/// <summary>
/// Simple component for action buttons
/// Attach to any button GameObject, assign an ActionData asset, and it handles the rest
/// 
/// WHY THIS EXISTS:
/// - Each button knows what action it triggers
/// - No manual wiring needed in UIController
/// - Easy to create new action buttons (just add this component + assign ActionData)
/// </summary>
[RequireComponent(typeof(Button))]
public class ActionButton : MonoBehaviour
{
    // ============================================
    // INSPECTOR FIELDS
    // ============================================

    [Title("Action Button Setup")]
    [InfoBox("Attach this to a Button. Assign an ActionData asset. Done!", InfoMessageType.Info)]

    [Required]
    [AssetsOnly]
    [Tooltip("The action this button will trigger (drag ActionData ScriptableObject here)")]
    public ActionData actionData;

    [Header("Optional: Member Selection Popup")]
    [Tooltip("If action requires members, this popup will open. Leave null to use default popup.")]
    public MemberSelectionPopup memberSelectionPopup;

    // ============================================
    // PRIVATE REFERENCES
    // ============================================

    private Button button;

    // ============================================
    // INITIALIZATION
    // ============================================

    private void Awake()
    {
        // Why: Get button component
        button = GetComponent<Button>();

        if (button == null)
        {
            Debug.LogError($"❌ ActionButton on {gameObject.name}: No Button component found!");
            return;
        }

        // Why: Hook up button click
        button.onClick.AddListener(OnButtonClicked);
    }

    private void Start()
    {
        // Why: Validate setup
        if (actionData == null)
        {
            Debug.LogError($"❌ ActionButton on {gameObject.name}: No ActionData assigned! Drag an ActionData ScriptableObject into the inspector.");
            if (button != null)
            {
                button.interactable = false; // Disable button to show it's broken
            }
        }
    }

    // ============================================
    // BUTTON CLICK HANDLER
    // ============================================

    private void OnButtonClicked()
    {
        if (actionData == null)
        {
            Debug.LogError($"❌ ActionButton: Cannot start action - no ActionData assigned!");
            return;
        }

        Debug.Log($"🎬 Action button clicked: {actionData.actionName}");

        // ============================================
        // CHECK IF ACTION REQUIRES MEMBER SELECTION
        // ============================================

        if (actionData.requiresMembers)
        {
            // Why: Open member selection popup
            OpenMemberSelectionPopup();
        }
        else
        {
            // Why: Start action immediately (no members needed)
            StartActionImmediately();
        }
    }

    // ============================================
    // MEMBER SELECTION POPUP
    // ============================================

    private void OpenMemberSelectionPopup()
    {
        // Why: Find popup if not assigned
        if (memberSelectionPopup == null)
        {
            memberSelectionPopup = FindObjectOfType<MemberSelectionPopup>();
        }

        if (memberSelectionPopup == null)
        {
            Debug.LogError($"❌ ActionButton: Cannot find MemberSelectionPopup! Make sure there's a MemberSelectionPopup in the scene.");
            return;
        }

        // Why: Subscribe to confirmation callback
        memberSelectionPopup.OnConfirm = OnMembersConfirmed;

        // Why: Open popup with this action
        memberSelectionPopup.ShowPopup(actionData);
    }

    // ============================================
    // CALLBACK FROM MEMBER SELECTION POPUP
    // ============================================

    private void OnMembersConfirmed(ActionData action, System.Collections.Generic.List<int> selectedMemberIndices)
    {
        if (ActionManager.Instance == null)
        {
            Debug.LogError($"❌ ActionButton: ActionManager.Instance is null!");
            return;
        }

        Debug.Log($"✅ Members confirmed: {selectedMemberIndices.Count} members selected for {action.actionName}");

        // Why: Start action with selected members
        ActionManager.Instance.StartAction(action, selectedMemberIndices);
    }

    // ============================================
    // IMMEDIATE ACTION START (No members needed)
    // ============================================

    private void StartActionImmediately()
    {
        if (ActionManager.Instance == null)
        {
            Debug.LogError($"❌ ActionButton: ActionManager.Instance is null!");
            return;
        }

        // Why: Start action with empty member list (no characters needed)
        ActionManager.Instance.StartAction(actionData, new System.Collections.Generic.List<int>());
    }

    // ============================================
    // ODIN INSPECTOR - DEBUG VIEW
    // ============================================

#if UNITY_EDITOR
    [Title("Debug Info", horizontalLine: true)]
    [ShowInInspector, ReadOnly, DisplayAsString]
    [PropertyOrder(100)]
    private string DebugInfo
    {
        get
        {
            if (actionData == null)
            {
                return "⚠️ NO ACTION DATA ASSIGNED!";
            }

            string info = $"Action: {actionData.actionName}\n";
            info += $"Requires Members: {(actionData.requiresMembers ? "Yes" : "No")}\n";

            if (actionData.requiresMembers)
            {
                info += $"Min/Max Members: {actionData.minMembers}-{actionData.maxMembers}\n";
            }

            info += $"Base Cost: ${actionData.baseCost}\n";
            info += $"Base Time: {actionData.baseTime}s";

            return info;
        }
    }
#endif
}