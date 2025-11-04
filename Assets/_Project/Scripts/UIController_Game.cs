using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Why: Main UI controller for the Game scene - ROOM NAVIGATION SYSTEM
/// Handles room switching, stats display, and menu controls
/// 
/// CLEANED UP: Removed old CharacterDisplay system - now uses RoomController + CharacterObject
/// </summary>
public class UIController_Game : MonoBehaviour
{
    [Header("Room Controllers - Assign in Inspector")]
    // Why: References to room controllers for navigation
    public RoomController breakroomController;
    public RoomController practiceRoomController;
    public RoomController productionRoomController;
    public RoomController meetingRoomController;
    public RoomController headOfficeController;
    public RoomController outsideController;  // Default "no menu" view
    public RoomController lobbyController;

    [Header("Menu Toggle Group")]
    public ToggleGroup menuToggleGroup;

    [Header("Stats Display")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI fansText;
    public TextMeshProUGUI unityText;
    public TextMeshProUGUI bandNameText;
    public TextMeshProUGUI quarterText;
    public TextMeshProUGUI yearText;

    [Header("Mini Stats Panel (Optional)")]
    public GameObject miniStatsPanel;
    public MiniStats miniStats;

    // Why: Track current active room
    private RoomController currentRoom;

    void Start()
    {
        // Why: Register with GameManager so it can call our RefreshUI()
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterUIController(this);
        }
        else
        {
            Debug.LogError("❌ UIController_Game: GameManager.Instance is null!");
        }

        // Why: Show Outside view by default (when no menu is toggled)
        ShowOutside();

        // Why: Do initial UI refresh
        RefreshUI();
    }

    // ============================================
    // MAIN REFRESH METHOD (called by GameManager on state changes)
    // ============================================

    /// <summary>
    /// Refreshes all UI elements - called by GameManager when state changes
    /// </summary>
    public void RefreshUI()
    {
        RefreshStats();
    }

    /// <summary>
    /// Compatibility wrapper for GameManager
    /// </summary>
    public void RefreshDisplay()
    {
        RefreshUI();
    }

    private void RefreshStats()
    {
        if (GameManager.Instance == null) return;

        GameManager gm = GameManager.Instance;

        if (moneyText != null) moneyText.text = "$" + gm.money.ToString("N0");
        if (fansText != null) fansText.text = gm.fans.ToString("N0");
        if (unityText != null) unityText.text = gm.unity.ToString("F0") + "%";
        if (bandNameText != null) bandNameText.text = gm.bandName;
        if (quarterText != null) quarterText.text = "Q" + gm.currentQuarter;
        if (yearText != null) yearText.text = "Year " + gm.currentYear;
    }

    // ============================================
    // ROOM NAVIGATION (replaces old panel toggles)
    // ============================================

    /// <summary>
    /// Show Outside view (default when all menus are off)
    /// </summary>
    private void ShowOutside()
    {
        if (outsideController != null)
        {
            SwitchToRoom(outsideController);
        }
        else
        {
            Debug.LogWarning("⚠️ Outside controller not assigned!");
        }
    }

    /// <summary>
    /// Switch to a specific room controller
    /// </summary>
    private void SwitchToRoom(RoomController targetRoom)
    {
        if (targetRoom == null)
        {
            Debug.LogWarning("⚠️ Target room is null!");
            return;
        }

        Debug.Log($"🚪 Switching to room: {targetRoom.roomData?.roomName ?? "UNKNOWN"}");

        // Why: Tell the room controller to show itself
        targetRoom.ShowRoom(currentRoom);

        // Why: Update current room reference
        currentRoom = targetRoom;
    }

    // ============================================
    // MENU TOGGLE HANDLERS (Called by Toggle buttons - NOW SWITCHES ROOMS)
    // ============================================

    public void OnPracticeToggle(bool isOn)
    {
        if (isOn && practiceRoomController != null)
        {
            SwitchToRoom(practiceRoomController);
        }
        else if (!isOn)
        {
            ShowOutside();
        }
    }

    public void OnProduceToggle(bool isOn)
    {
        if (isOn && productionRoomController != null)
        {
            SwitchToRoom(productionRoomController);
        }
        else if (!isOn)
        {
            ShowOutside();
        }
    }

    public void OnTourToggle(bool isOn)
    {
        if (isOn && lobbyController != null)
        {
            SwitchToRoom(lobbyController);
        }
        else if (!isOn)
        {
            ShowOutside();
        }
    }

    public void OnReleaseToggle(bool isOn)
    {
        // Why: Release might use lobby or production room - adjust as needed
        if (isOn && productionRoomController != null)
        {
            SwitchToRoom(productionRoomController);
        }
        else if (!isOn)
        {
            ShowOutside();
        }
    }

    public void OnFinanceToggle(bool isOn)
    {
        if (isOn && headOfficeController != null)
        {
            SwitchToRoom(headOfficeController);
        }
        else if (!isOn)
        {
            ShowOutside();
        }
    }

    public void OnMarketingToggle(bool isOn)
    {
        if (isOn && meetingRoomController != null)
        {
            SwitchToRoom(meetingRoomController);
        }
        else if (!isOn)
        {
            ShowOutside();
        }
    }

    public void OnManagingToggle(bool isOn)
    {
        if (isOn && headOfficeController != null)
        {
            SwitchToRoom(headOfficeController);
        }
        else if (!isOn)
        {
            ShowOutside();
        }
    }

    // ============================================
    // MENU CONTROLS
    // ============================================

    /// <summary>
    /// Close all menus and return to Outside view
    /// </summary>
    public void CloseAllMenus()
    {
        if (menuToggleGroup != null)
        {
            if (!menuToggleGroup.allowSwitchOff)
            {
                menuToggleGroup.allowSwitchOff = true;
                Debug.Log("✅ Enabled allowSwitchOff on menuToggleGroup");
            }

            menuToggleGroup.SetAllTogglesOff();
            Debug.Log("✅ All menu toggles turned off");
        }
        else
        {
            Debug.LogWarning("UIController_Game: menuToggleGroup is null - cannot untoggle menu buttons!");
        }

        // Why: Show outside view when all menus are closed
        ShowOutside();
    }
}